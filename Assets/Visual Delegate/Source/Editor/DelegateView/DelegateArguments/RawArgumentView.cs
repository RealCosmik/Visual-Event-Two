using UnityEditor;
using System.Collections.Generic;
using System;
namespace VisualEvent
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
