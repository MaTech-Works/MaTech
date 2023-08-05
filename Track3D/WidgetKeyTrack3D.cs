// Copyright (c) 2023, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

using System.Linq;
using MaTech.Common.Unity;
using MaTech.Common.Utils;
using UnityEngine;

namespace MaTech.Track3D {
    public class WidgetKeyTrack3D : MonoBehaviour {
        [Header("Scene Parts")]
        [SerializeField] private Camera referenceCamera;

        [Tooltip("Put it on the judge line, local z perpendicular to the track, local x parallel to the judge line. " +
                 "The line from this origin to the far end need to be parallel to the track.")]
        [SerializeField] private Transform locatorTrackOrigin;

        [Tooltip("Put it on the far end of the track, local z perpendicular to the track. " +
                 "The line from the origin to this far end need to be parallel to the track.")]
        [SerializeField] private Transform locatorTrackFarEnd;

        [Space]
        [SerializeField] private Transform[] transformsToRotate;
        [SerializeField] private Transform[] transformsToScale;
        [SerializeField] private Transform[] transformsWithExtraScale;
        [SerializeField] private Transform[] transformsAsCover;

        [Header("Dimensions")]
        [Tooltip("The axis of local rotation on the anchor.")]
        [SerializeField] private Vector3 rotationAxis = Vector3.right;

        [Range(0, 1)]
        [SerializeField] private float extraScaleFactor = 1.0f;

        [Space]
        [SerializeField] private float offsetPerScrollDistance = 5;
        [SerializeField] private float distanceFarEnd = 10000;
        [SerializeField] private float distanceMax = 20000;

        [Space]
        [SerializeField] private AnimationCurve distanceScaleByDistanceInView = AnimationCurve.Linear(0, 1, 20000, 1);

        [Header("Dynamic Parameters (Preview in Editor)")]
        [Range(0, 90)]
        [SerializeField] private float trackAngle = 45;

        public float TrackAngle {
            get => trackAngle;
            set {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (trackAngle == value) return;
                trackAngle = Mathf.Clamp(value, 0, 90);
                UpdateTrackVisual();
            }
        }

        [Range(0.1f, 1)]
        [SerializeField] private float trackScale = 0.5f;

        public float TrackScale {
            get => trackScale;
            set {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (trackScale == value) return;
                trackScale = Mathf.Clamp(value, 0.1f, 1);
                UpdateTrackVisual();
            }
        }

        [Range(0, 1)]
        [SerializeField] private float compensationOnDistance = 0.2f;

        public float CompensationDistance {
            get => compensationOnDistance;
            set {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (compensationOnDistance == value) return;
                compensationOnDistance = Mathf.Clamp(value, 0, 1);
                UpdateTrackVisual();
            }
        }

        [Range(0, 1)]
        [SerializeField] private float compensationOnScale = 1.0f;

        public float CompensationScale {
            get => compensationOnScale;
            set {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (compensationOnScale == value) return;
                compensationOnScale = Mathf.Clamp(value, 0, 1);
                UpdateTrackVisual();
            }
        }

        [Range(0, 10)]
        [SerializeField] private float noteSpeedScale = 1.0f;

        public float NoteSpeedScale {
            get => noteSpeedScale;
            set {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (noteSpeedScale == value) return;
                noteSpeedScale = Mathf.Clamp(value, 0, 10);
                UpdateTrackVisual();
            }
        }

        [Range(0, 1)]
        [SerializeField] private float coverRatio = 0.8f;

        public float CoverRatio {
            get => coverRatio;
            set {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (coverRatio == value) return;
                coverRatio = Mathf.Clamp(value, 0, 1);
                UpdateTrackVisual();
            }
        }

        private CachedValueByFrame<Vector3> cameraDirection;
        private CachedValueByFrame<Vector3> cameraPosition;
        private CachedValueByFrame<Vector3> trackOriginPosition;
        private CachedValueByFrame<Vector3> trackFarEndPosition;

        private CachedValueByFrame<float> trackOriginDistanceToCamera;
        private CachedValueByFrame<float> trackOriginPositionOnCameraAxis;
        private CachedValueByFrame<float> trackOriginViewDirToCompensatedDirAngle;
        private CachedValueByFrame<float> compensationDistanceFactor;
        private CachedValueByFrame<float> scaleAtJudgeLine;
        private CachedValueByFrame<float> scaleOfOffset;

        public float GetOffsetFromScrollPositionToJudge(float scrollPositionToJudge, bool scaled = true) {
            float rawDist = scrollPositionToJudge * offsetPerScrollDistance * noteSpeedScale;
            float factoredDist = compensationDistanceFactor * rawDist;
            float calibratedDist = factoredDist >= 1 ? float.PositiveInfinity : rawDist / (1 - factoredDist);
            return Mathf.Min(calibratedDist * (scaled ? scaleOfOffset : 1.0f), distanceMax);
        }

        /// For covers.
        /// ratio = 0 at bottom of screen, = 1 at top of screen, linear on screen space, no clamping.
        public float GetOffsetFromRatio2D(float ratio, bool scaled = false) {
            // All radians below
            float angleToTop = referenceCamera.fieldOfView * Mathf.Deg2Rad * 0.5f;
            float angleToBottom = -angleToTop; // signed
            float angleToNote = Mathf.Atan(MathUtil.LinearMap(0, 1, Mathf.Tan(angleToBottom), Mathf.Tan(angleToTop), ratio));

            Vector3 offsetTrackOrigin = (Vector3)trackOriginPosition - cameraPosition;
            float angleToTrackOrigin = Mathf.Atan2(offsetTrackOrigin.y, offsetTrackOrigin.z);

            float worldLengthTrack = Vector3.Distance(trackFarEndPosition, trackOriginPosition);
            float worldDistance = offsetTrackOrigin.magnitude * MathUtil.RatioFromSinRadians(
                angleToNote - angleToTrackOrigin, Mathf.Max(1e-8f, Mathf.PI * 0.5f - Mathf.Deg2Rad * trackAngle - angleToNote));

            return Mathf.Min(worldDistance / worldLengthTrack * distanceFarEnd * (scaled ? scaleOfOffset : 1.0f), distanceMax);
        }

        public float GetHeightScaleFromPosition(Vector3 worldPos) {
            Vector3 dirA = Vector3.ProjectOnPlane(worldPos - cameraPosition, locatorTrackOrigin.right);
            float angleA = Vector3.Angle((Vector3)trackFarEndPosition - trackOriginPosition, dirA);
            float angleB = angleA + compensationOnScale * trackAngle;
            return Mathf.Max(0.1f, MathUtil.RatioFromSinDegrees(angleB, angleA));

            // Below is mathematically matching the distance function, but not good in looking.
            /*
            float distA = dirA.magnitude;
            float distB = trackOriginDistanceToCamera * MathUtil.RatioFromSinDegrees(trackOriginViewDirToCompensatedDirAngle, angleB);
            return Mathf.Max(1, (distA / distB) * MathUtil.RatioFromSinDegrees(angleB, angleA));
            */
        }

        public float GetHeightScaleAtJudgeLine() => scaleAtJudgeLine;

        public void ForceRefresh() {
            cameraDirection.Expire();
            cameraPosition.Expire();
            trackOriginPosition.Expire();
            trackFarEndPosition.Expire();

            trackOriginDistanceToCamera.Expire();
            trackOriginPositionOnCameraAxis.Expire();
            trackOriginViewDirToCompensatedDirAngle.Expire();

            compensationDistanceFactor.Expire();
            scaleAtJudgeLine.Expire();

            UpdateTrackVisual();
        }

        private void UpdateTrackVisual() {
            if (transformsToRotate == null || transformsToScale == null || referenceCamera == null) return;
            foreach (var tr in transformsToRotate) {
                tr.localRotation = Quaternion.AngleAxis(Mathf.Min(trackAngle, 89.99f), rotationAxis);
            }

            foreach (var tr in transformsToScale) {
                float scaleFactor = transformsWithExtraScale.Contains(tr) ? extraScaleFactor : 1;
                tr.localScale = Vector3.one * Mathf.Lerp(1, trackScale, scaleFactor);
            }

            foreach (var tr in transformsAsCover) {
                tr.localPosition = Vector3.up * GetOffsetFromRatio2D(coverRatio);
            }
        }

#if UNITY_EDITOR
        void OnValidate() {
            DefineCachedValues();
            UpdateTrackVisual();
        }
#endif // UNITY_EDITOR

        void Awake() {
            DefineCachedValues();
        }

        void DefineCachedValues() {
            if (cameraDirection != null || referenceCamera == null) return;

            cameraDirection = new CachedValueByFrame<Vector3>(() => referenceCamera.transform.forward);
            cameraPosition = new CachedValueByFrame<Vector3>(() => referenceCamera.transform.position);
            trackOriginPosition = new CachedValueByFrame<Vector3>(() => locatorTrackOrigin.position);
            trackFarEndPosition = new CachedValueByFrame<Vector3>(() => locatorTrackFarEnd.position);

            trackOriginDistanceToCamera = new CachedValueByFrame<float>(() => Vector3.Distance(trackOriginPosition, cameraPosition));
            trackOriginPositionOnCameraAxis = new CachedValueByFrame<float>(() => Vector3.Dot(locatorTrackOrigin.position - cameraPosition, cameraDirection));
            trackOriginViewDirToCompensatedDirAngle = new CachedValueByFrame<float>(() => (1 - compensationOnDistance) * trackAngle + 90 - referenceCamera.fieldOfView / 2);

            // The key ingredient for 3D compensation magics
            compensationDistanceFactor = new CachedValueByFrame<float>(() => MathUtil.RatioFromSinDegrees(compensationOnDistance * trackAngle,
                trackOriginViewDirToCompensatedDirAngle) / trackOriginDistanceToCamera * Vector3.Distance(trackFarEndPosition, trackOriginPosition) / distanceFarEnd);

            scaleAtJudgeLine = new CachedValueByFrame<float>(() => GetHeightScaleFromPosition(trackOriginPosition));
            scaleOfOffset = new CachedValueByFrame<float>(() => distanceScaleByDistanceInView.Evaluate(GetOffsetFromRatio2D(1.0f)));
        }
    }
}