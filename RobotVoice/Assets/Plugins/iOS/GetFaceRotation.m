#import <ARKit/ARKit.h>

typedef struct
{
    int version;
    void* anchor;
}  NativeFace;

simd_float4 GetFaceRotation(NativeFace* nativeFace)
{
    ARFaceAnchor* faceAnchor = (__bridge ARFaceAnchor*)nativeFace->anchor;

    // Flip handedness
    const simd_float3x3 rotation = simd_matrix(
         faceAnchor.transform.columns[0].xyz,
         faceAnchor.transform.columns[1].xyz,
        -faceAnchor.transform.columns[2].xyz);

    // Convert to quaternion
    const simd_float4 v = simd_quaternion(rotation).vector;

    // Convert back to left-handed for Unity
    return simd_make_float4(v.xy, -v.zw);
}
