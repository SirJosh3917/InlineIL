using Mono.Cecil.Cil;
using System;

namespace InlineIL
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class InlineAttribute : Attribute
	{
	}

	public class ParameterPassAttribute : Attribute
	{
		public ParameterPassAttribute(string id)
		{
			Id = id;
		}

		public string Id { get; }
	}

	// just class to detect opcodes & such here
	public static class IL
	{
		public static void Emit(OpCode op)
		{
		}

		public static void Emit(OpCode op, string strValue)
		{
		}

		public static void EmitParameterPass(OpCode op, string id)
		{
		}
	}
}
