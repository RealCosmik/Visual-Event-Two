using System;
using VisualEvent;

internal class fixerclass : RawCall
{
    private void AOTFIX()
    {
        createActionCall1<Action>(null, null);
        throw new System.AccessViolationException();
    }
}
