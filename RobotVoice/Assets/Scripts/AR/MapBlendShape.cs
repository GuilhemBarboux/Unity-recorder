using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARKit;

namespace AR
{
    public class MapBlendShape : ARBlendShape
    {
        [SerializeField]
        public string prefixMap = "";
        [FormerlySerializedAs("Indexes")] [SerializeField]
        private List<string> indexes = new List<string>();
        
        private readonly List<string> shapeIndexes = new List<string>();
        
        public readonly Dictionary<string, float> shapeWeights = new Dictionary<string, float>();

        private new void Awake()
        {
            base.Awake();
            indexes.ForEach(index =>
            {
                var i = $"{prefixMap}.{index}";
                shapeIndexes.Add(i);
                shapeWeights.Add(i, 0);
            });
        }

        protected override bool IsValid()
        {
            return shapeIndexes != null;
        }

        protected override void SetShapeWeight(int index, float weight)
        {
            var shapeIndex = shapeIndexes.ElementAtOrDefault(index) ?? "";
            if (!shapeWeights.ContainsKey(shapeIndex)) return;
            shapeWeights[shapeIndex] = weight;
        }

        protected override int GetShapeIndex(string id)
        {
            return shapeIndexes.IndexOf(id);
        }
    }
}