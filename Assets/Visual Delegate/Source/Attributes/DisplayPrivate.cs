using System;
namespace VisualDelegates
{
    /// <summary>
    /// Exposes any non-public field to delegate
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field,AllowMultiple =false)]
    public class DisplayPrivate : Attribute
    {

    }
}
