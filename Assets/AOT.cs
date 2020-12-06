using System;
using VisualDelegates;

internal class fixerclass : RawCall
{
    private void AOTFIX()
    {
        createActionCall3<System.String, UnityEngine.LogType, UnityEngine.Object>(null, null, null, null);
        throw new System.AccessViolationException();
    }
}
