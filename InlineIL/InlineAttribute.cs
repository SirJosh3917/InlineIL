using Mono.Cecil.Cil;
using System;

namespace InlineIL
{
	[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class InlineAttribute : Attribute
	{
	}

	// just class to detect opcodes & such here
	public static class IL
	{
		public static void Emit(OpCode op)
		{
		}
	}
}
