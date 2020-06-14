using System;
using VisualDelegates;

internal class fixerclass : RawCall
{
    private void AOTFIX()
    {
        createPropertyCall<System.String>(null, null);
        createPropertyCall<UnityEngine.Vector3>(null, null);
        createPropertyCall<System.Int32>(null, null);
        ToStringDelegate<System.Int32>(null);
        createActionCall3<System.Single, System.Single, System.Single>(null, null, null, null);
        throw new System.AccessViolationException();
    }
}
