using UnityEngine;

namespace Record
{
    [CreateAssetMenu(fileName = "ExportMediaDimension", menuName = "Export Media Dimension", order = 0)]
    public class MediaExport : ScriptableObject
    {
        [SerializeField] public string description;
        [SerializeField] public Vector2Int dimension;
    }
}