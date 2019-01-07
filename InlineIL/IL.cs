using Mono.Cecil.Cil;

namespace InlineIL
{
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

		public static void EmitThrow(object toThrow)
		{
		}
	}
}
