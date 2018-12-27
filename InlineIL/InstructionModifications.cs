using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace InlineIL
{
	public static class InstructionModifications
	{
		public static Modification Emit(Modification modification, OpCode opcode)
		{
			modification.ReplaceWith = new Instruction[]
			{
				Instruction.Create(opcode),
			};

			return modification;
		}

		public static Modification Emit(Modification modification, OpCode opcode, string str)
		{
			modification.ReplaceWith = new Instruction[]
			{
				Instruction.Create(opcode, str)
			};

			return modification;
		}

		public static Modification Emit(Modification modification, OpCode opcode, MethodInfo method)
		{
			modification.ReplaceWith = new Instruction[]
			{
				Instruction.Create(opcode, modification.Module.ImportReference(method))
			};

			return modification;
		}
	}
}
