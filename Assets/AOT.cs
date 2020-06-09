using System;
using VisualDelegates;

internal class fixerclass : RawCall
{
    private void AOTFIX()
    {
        createPropertyCall<System.Int32>(null, null);
        createPropertyCall<UnityEngine.Vector3>(null, null);
        throw new System.AccessViolationException();
    }
}
