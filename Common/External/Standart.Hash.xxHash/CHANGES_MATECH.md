# Changes from original version to MaTech

- Removed `xxHash3` and `xxHash128` since lack of SIMD intrinsic in Unity
- Removed methods with uint128 parameter `Utils.ToGuid` and `Utils.ToBytes`