// Copyright (c) 2024, LuiCat (as MaTech)
// 
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at https://mozilla.org/MPL/2.0/.

#nullable enable

using System;
using System.Collections.Generic;
using MaTech.Common.Algorithm;
using Optional;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

namespace MaTech.Common.Unity {
    [Serializable, InlineProperty]
    public struct ShaderProperty {
        public static ShaderProperty Empty => FromID(unknownID);

        public readonly string? Name => name;
        public readonly int? DirtyID => id;
        public int ID => nameOfID == name ? id : id = Shader.PropertyToID(nameOfID = name);
        
        public static ShaderProperty FromID(int id) => new() {
            id = id,
            nameOfID = null,
            name = null,
        };
        public static ShaderProperty FromName(string name) => string.IsNullOrWhiteSpace(name) ? Empty : new() {
            id = Shader.PropertyToID(name),
            nameOfID = name,
            name = name,
        };
        
        public static implicit operator ShaderProperty(int id) => FromID(id);
        public static implicit operator ShaderProperty(string name) => FromName(name);
        public static implicit operator int(ShaderProperty property) => property.ID;
        
        [SerializeField]
        private string? name;
        
        private string? nameOfID;
        private int id;
        
        private static int unknownID = Shader.PropertyToID("__Some_Absolutely_Unused_Name__Do_Not_Rely_On_This_Name");
        
        public override string ToString() {
            if (name is null) return id == unknownID ? "(empty)" : $"Name Unknown (id: {id})";
            if (name != nameOfID) return id == unknownID ? $"{name} (id unknown)" : $"{name} (id dirty)";
            return $"{name} (id: {id})";
        }
        
        public void Set<T>(in T value, Material? material = null) => genericProperty.Set((this, material), value);
        public Option<T> Get<T>(Material? material = null) => genericProperty.Get<T>((this, material));
        
        public float GetFloat(Material? material = null) => material?.GetFloat(ID) ?? Shader.GetGlobalFloat(ID);
        public int GetInteger(Material? material = null) => material?.GetInteger(ID) ?? Shader.GetGlobalInteger(ID);
        public Vector4 GetVector(Material? material = null) => material?.GetVector(ID) ?? Shader.GetGlobalVector(ID);
        public Color GetColor(Material? material = null) => material?.GetColor(ID) ?? Shader.GetGlobalColor(ID);
        public Matrix4x4 GetMatrix(Material? material = null) => material?.GetMatrix(ID) ?? Shader.GetGlobalMatrix(ID);
        public Texture GetTexture(Material? material = null) => material?.GetTexture(ID) ?? Shader.GetGlobalTexture(ID);
        public float[] GetFloatArray(Material? material = null) => material?.GetFloatArray(ID) ?? Shader.GetGlobalFloatArray(ID);
        public Vector4[] GetVectorArray(Material? material = null) => material?.GetVectorArray(ID) ?? Shader.GetGlobalVectorArray(ID);
        public Matrix4x4[] GetMatrixArray(Material? material = null) => material?.GetMatrixArray(ID) ?? Shader.GetGlobalMatrixArray(ID);

        public void GetFloatArray(List<float> values, Material? material = null) { if (material is not null) material.GetFloatArray(ID, values); else Shader.GetGlobalFloatArray(ID, values); }
        public void GetVectorArray(List<Vector4> values, Material? material = null) { if (material is not null) material.GetVectorArray(ID, values); else Shader.GetGlobalVectorArray(ID, values); }
        public void GetMatrixArray(List<Matrix4x4> values, Material? material = null) { if (material is not null) material.GetMatrixArray(ID, values); else Shader.GetGlobalMatrixArray(ID, values); }
        
        public void SetFloat(in float value, Material? material = null) { if (material is not null) material.SetFloat(ID, value); else Shader.SetGlobalFloat(ID, value); }
        public void SetInteger(in int value, Material? material = null) { if (material is not null) material.SetInteger(ID, value); else Shader.SetGlobalInteger(ID, value); }
        public void SetVector(in Vector4 value, Material? material = null) { if (material is not null) material.SetVector(ID, value); else Shader.SetGlobalVector(ID, value); }
        public void SetColor(in Color value, Material? material = null) { if (material is not null) material.SetColor(ID, value); else Shader.SetGlobalColor(ID, value); }
        public void SetMatrix(in Matrix4x4 value, Material? material = null) { if (material is not null) material.SetMatrix(ID, value); else Shader.SetGlobalMatrix(ID, value); }
        public void SetTexture(in Texture value, Material? material = null) { if (material is not null) material.SetTexture(ID, value); else Shader.SetGlobalTexture(ID, value); }
        public void SetBuffer(in ComputeBuffer value, Material? material = null) { if (material is not null) material.SetBuffer(ID, value); else Shader.SetGlobalBuffer(ID, value); }
        public void SetBuffer(in GraphicsBuffer value, Material? material = null) { if (material is not null) material.SetBuffer(ID, value); else Shader.SetGlobalBuffer(ID, value); }

        public void SetFloatArray(List<float> values, Material? material = null) { if (material is not null) material.SetFloatArray(ID, values); else Shader.SetGlobalFloatArray(ID, values); }
        public void SetFloatArray(float[] values, Material? material = null) { if (material is not null) material.SetFloatArray(ID, values); else Shader.SetGlobalFloatArray(ID, values); }
        public void SetVectorArray(List<Vector4> values, Material? material = null) { if (material is not null) material.SetVectorArray(ID, values); else Shader.SetGlobalVectorArray(ID, values); }
        public void SetVectorArray(Vector4[] values, Material? material = null) { if (material is not null) material.SetVectorArray(ID, values); else Shader.SetGlobalVectorArray(ID, values); }
        public void SetMatrixArray(List<Matrix4x4> values, Material? material = null) { if (material is not null) material.SetMatrixArray(ID, values); else Shader.SetGlobalMatrixArray(ID, values); }
        public void SetMatrixArray(Matrix4x4[] values, Material? material = null) { if (material is not null) material.SetMatrixArray(ID, values); else Shader.SetGlobalMatrixArray(ID, values); }
        
        public void SetTexture(RenderTexture value, RenderTextureSubElement element, Material? material = null) { if (material is not null) material.SetTexture(ID, value, element); else Shader.SetGlobalTexture(ID, value, element); }
        public void SetConstantBuffer(ComputeBuffer value, int offset, int size, Material? material = null) { if (material is not null) material.SetConstantBuffer(ID, value, offset, size); else Shader.SetGlobalConstantBuffer(ID, value, offset, size); }
        public void SetConstantBuffer(GraphicsBuffer value, int offset, int size, Material? material = null) { if (material is not null) material.SetConstantBuffer(ID, value, offset, size); else Shader.SetGlobalConstantBuffer(ID, value, offset, size); }

        private static readonly GenericProperty<(ShaderProperty property, Material? material)> genericProperty = new(p => p
            .Define(t => t.property.GetFloat(t.material), (t, value) => t.property.SetFloat(value, t.material))
            .Define(t => t.property.GetInteger(t.material), (t, value) => t.property.SetInteger(value, t.material))
            .Define(t => t.property.GetVector(t.material), (t, value) => t.property.SetVector(value, t.material))
            .Define(t => t.property.GetColor(t.material), (t, value) => t.property.SetColor(value, t.material))
            .Define(t => t.property.GetMatrix(t.material), (t, value) => t.property.SetMatrix(value, t.material))
            .Define(t => t.property.GetTexture(t.material), (t, value) => t.property.SetTexture(value, t.material))
            .Define(t => t.property.GetFloatArray(t.material), (t, value) => t.property.SetFloatArray(value, t.material))
            .Define(t => t.property.GetVectorArray(t.material), (t, value) => t.property.SetVectorArray(value, t.material))
            .Define(t => t.property.GetMatrixArray(t.material), (t, value) => t.property.SetMatrixArray(value, t.material))
            .Define(t => t.property.GetFloatArray(t.material), (t, value) => t.property.SetFloatArray(value, t.material))
            .Define(t => t.property.GetVectorArray(t.material), (t, value) => t.property.SetVectorArray(value, t.material))
            .Define(t => t.property.GetMatrixArray(t.material), (t, value) => t.property.SetMatrixArray(value, t.material))
            .Define<ComputeBuffer>((t, value) => t.property.SetBuffer(value, t.material))
            .Define<GraphicsBuffer>((t, value) => t.property.SetBuffer(value, t.material))
            .Define<(RenderTexture texture, RenderTextureSubElement element)>((t, value) => t.property.SetTexture(value.texture, value.element, t.material))
            .Define<(ComputeBuffer buffer, int offset, int size)>((t, value) => t.property.SetConstantBuffer(value.buffer, value.offset, value.size, t.material))
            .Define<(GraphicsBuffer buffer, int offset, int size)>((t, value) => t.property.SetConstantBuffer(value.buffer, value.offset, value.size, t.material))
        );
    }
}