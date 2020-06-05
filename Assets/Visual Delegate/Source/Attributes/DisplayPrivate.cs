using System;
namespace VisualDelegates
{
    /// <summary>
    /// Exposes any non-public field to delegate
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field)]
    public class DisplayPrivate : Attribute
    {

    }
}
