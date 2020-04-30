using System;
namespace VisualEvent
{
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
	public sealed class RunTimeRestricted : Attribute
	{
	}
}
