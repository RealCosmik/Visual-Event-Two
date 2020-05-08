using UnityEditor;
using System;
namespace VisualDelegates.Editor
{
    public class RawArgumentView
    {
        public MonoScript CurrentScript;
        public RawReferenceView argumentReference;
        public bool hasCustomType;
        public Type CurrentCustomType;
        public RawArgumentView(System.Type argumentType)
        {
            argumentReference = new RawReferenceView(argumentType);
        }
    }
}
