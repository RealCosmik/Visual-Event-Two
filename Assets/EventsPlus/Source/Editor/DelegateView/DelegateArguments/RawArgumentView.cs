using UnityEditor;
using System.Collections.Generic;
namespace VisualEvent
{
    public class RawArgumentView
    {
        public MonoScript CurrentScript;
        public RawReferenceView argumentReference;
        public RawArgumentView(System.Type argumentType)
        {
            argumentReference = new RawReferenceView(argumentType);
        }
    }
}
