using System;
namespace VisualDelegates
{
	/// <summary>
	/// Removed the ability to add and remove calls from this delegate in editor
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
	public sealed class RunTimeRestricted : Attribute { }
}
