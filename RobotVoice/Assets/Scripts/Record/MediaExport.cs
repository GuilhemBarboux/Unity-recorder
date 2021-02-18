using UnityEngine;

namespace Record
{
    [CreateAssetMenu(fileName = "ExportMedia", menuName = "Record/Media dimension", order = 0)]
    public class MediaExport : ScriptableObject
    {
        [SerializeField] public string description;
        [SerializeField] public Vector2Int dimension;
    }
}