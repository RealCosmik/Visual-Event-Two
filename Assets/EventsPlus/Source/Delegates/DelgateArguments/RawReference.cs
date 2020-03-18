using UnityEngine;
namespace VisualEvent
{
    [System.Serializable]
    public class RawReference :RawDelegate
    {
        public string ParentArgumentType;
        [SerializeField] bool m_isvaluetype;
        [SerializeField] bool isparentargstring;
        public bool isvaluetype => m_isvaluetype;
        public bool isparentargumentstring => isparentargstring;
    }
}
