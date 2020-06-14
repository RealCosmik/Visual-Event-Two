using System;
using VisualDelegates;

internal class fixerclass : RawCall
{
    private void AOTFIX()
    {
        createPropertyCall<System.Int32>(null, null);
        createPropertyCall<UnityEngine.Vector3>(null, null);
        createPropertyCall<System.String>(null, null);
        createPropertyCall<System.Boolean>(null, null);
        createActionCall3<System.Single, System.Single, System.Single>(null, null, null, null);
        createActionCall3<System.String, LogType, UnityEngine.Object>(null, null, null, null);
        throw new System.AccessViolationException();
    }
}
