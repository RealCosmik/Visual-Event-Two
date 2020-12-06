using System;
namespace VisualDelegates
{
    /// <summary>
    /// Hide fields from showing up in <see cref="VisualDelegate"/> Drawer
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Field,AllowMultiple =false)]
    public class HideExposedField : System.Attribute { }
}
