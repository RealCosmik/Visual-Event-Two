using System;
using VisualEvent;

internal class fixerclass : RawCall
{
    private void AOTFIX()
    {
        createFuncCall2<Func<System.Int32>, System.Int32, System.Collections.IEnumerator>(null, null, null);
        throw new System.AccessViolationException();
    }
}
