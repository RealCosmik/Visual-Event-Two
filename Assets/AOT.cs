using System;
using VisualEvent;

internal class fixerclass : RawCall
{
    private void AOTFIX()
    {
        createPropertyCall<System.Int32>(null, null);
        throw new System.AccessViolationException();
    }
}
