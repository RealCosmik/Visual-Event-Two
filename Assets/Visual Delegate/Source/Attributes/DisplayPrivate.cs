using System;
namespace VisualDelegates
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
    public class DisplayPrivate : Attribute
    {

    }
}
