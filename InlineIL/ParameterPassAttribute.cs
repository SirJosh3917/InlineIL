using System;

namespace InlineIL
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
	public sealed class ParameterPassAttribute : Attribute
	{
		public ParameterPassAttribute(string id)
		{
			Id = id;
		}

		public string Id { get; }
	}
}
