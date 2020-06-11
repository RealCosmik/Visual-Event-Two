using System;
using VisualDelegates;

internal class fixerclass : RawCall
{
    private void AOTFIX()
    {
        createFunc1<System.Int32, System.Collections.IEnumerator>(null);
        throw new System.AccessViolationException();
    }
}
