using System;

namespace InlineIL
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public sealed class InlineAttribute : Attribute
	{
	}
}
