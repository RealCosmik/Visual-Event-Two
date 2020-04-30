using System;
using VisualEvent;

internal class fixerclass : RawCall
{
    private void AOTFIX()
    {
        createPropertyCall<UnityEngine.Vector3>(null, null);
        throw new System.AccessViolationException();
    }
}
