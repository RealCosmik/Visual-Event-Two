using System;
using VisualEvent;

internal class fixerclass : RawCall
{
    private void AOTFIX()
    {
        createFieldCall<intpub>(null, null);
        createActionCall1<System.Int32>(null, null);
        createAction1<System.Int32>(null);
        throw new System.AccessViolationException();
    }
}
