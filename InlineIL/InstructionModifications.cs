using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
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

		public static Modification EmitLdStr(Modification modification, OpCode opcode, string str)
		{
			modification.ReplaceWith = new Instruction[]
			{
				Instruction.Create(opcode, str)
			};

			return modification;
		}

		public static Modification EmitCall(Modification modification, OpCode opcode, string str)
		{
			modification.ReplaceWith = new Instruction[]
			{
				Instruction.Create(opcode, modification.Module.ImportReference
				(
					// TODO: interpret the s tring into a method
					typeof(Console)
					.GetMethod(nameof(Console.WriteLine), System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static, Type.DefaultBinder, new Type[]
					{
						typeof(string)
					}, null)
				))
			};

			return modification;
		}
	}
}
