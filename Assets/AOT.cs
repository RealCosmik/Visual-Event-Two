using System;
using VisualDelegates;

internal class fixerclass : RawCall
{
    private void AOTFIX()
    {
        createFieldCall<UnityEngine.Object>(null, null);
        createFieldCall<UnityEngine.Object>(null, null);
        throw new System.AccessViolationException();
    }
}
