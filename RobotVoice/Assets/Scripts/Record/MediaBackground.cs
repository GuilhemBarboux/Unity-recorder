using UnityEngine;

namespace Record
{
    [CreateAssetMenu(fileName = "mediaBackground", menuName = "Record/Media Background", order = 0)]
    public class MediaBackground : ScriptableObject
    {
        [SerializeField] public Color backgroundColor;
        [SerializeField] public Sprite backgroundPreview;
        [SerializeField] public Material backgroundMaterial;
    }
}