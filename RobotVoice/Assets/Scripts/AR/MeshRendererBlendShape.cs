using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARKit;

namespace AR
{
    public class MeshRendererBlendShape : ARBlendShape
    {
        [FormerlySerializedAs("Mesh")] [SerializeField]
        private SkinnedMeshRenderer meshRenderer;
        
        protected override bool IsValid()
        {
            return meshRenderer != null && meshRenderer.enabled && meshRenderer.sharedMesh != null;
        }

        protected override void SetShapeWeight(int index, float weight)
        {
            meshRenderer.SetBlendShapeWeight(index, weight);
        }

        protected override int GetShapeIndex(string id)
        {
            return meshRenderer.sharedMesh.GetBlendShapeIndex(id);
        }
    }
}