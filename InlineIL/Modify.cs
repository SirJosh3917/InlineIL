using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;

namespace InlineIL
{
	public struct Modification
	{
		public ModuleDefinition Module;
		public Instruction[] ReplaceWith;
		public Instruction[] DeleteOpCodes;
	}

	public static class Modify
	{
		private static bool IsLoadsOpcode(this Instruction instruction)
			=> instruction.OpCode.Code == Code.Ldsfld &&
			instruction.Operand is FieldReference field &&
			field.FieldType.FullName == typeof(OpCode).FullName;

		private static bool IsLoadStringOpcode(this Instruction instruction)
			=> instruction.OpCode.Code == Code.Ldstr;

		private static OpCode GetOpcode(this FieldReference opcodeField)
			=> (OpCode)typeof(OpCodes) // get the OpCodes.Whatever
			.GetField(opcodeField.Name)
			.GetValue(null);

		private static void HandleInstruction(ModuleDefinition module, Instruction instruction, ref List<Modification> modificationsToBeMade)
		{
			// candidate for an emission replace
			if (instruction.OpCode.Code == Code.Call &&
				instruction.Previous != null)
			{
				if ((instruction.Previous?.IsLoadsOpcode() ?? false)) // emit(opcode)
				{
					var opcode = ((FieldReference)instruction.Previous.Operand).GetOpcode();
					var mod = new Modification
					{
						Module = module,
						DeleteOpCodes = new Instruction[]
						{
							instruction.Previous,
							instruction,
						}
					};

					modificationsToBeMade.Add(InstructionModifications.Emit(mod, opcode));
				}
				else
				if ((instruction.Previous?.IsLoadStringOpcode() ?? false) &&
					(instruction.Previous.Previous?.IsLoadsOpcode() ?? false))
				{
					// ldstr or call

					var mod = new Modification
					{
						Module = module,
						DeleteOpCodes = new Instruction[]
						{
							instruction.Previous.Previous,
							instruction.Previous,
							instruction,
						}
					};

					var opcode = ((FieldReference)instruction.Previous.Previous.Operand).GetOpcode();
					var strValue = (string)instruction.Previous.Operand;

					if (opcode == OpCodes.Ldstr) // ldstr!
					{
						modificationsToBeMade.Add(InstructionModifications.EmitLdStr(mod, opcode, strValue));
					}
					else // some kind of call
					{
						modificationsToBeMade.Add(InstructionModifications.EmitCall(mod, opcode, strValue));
					}
				}
			}
		}

		public static void ReplaceEmits(ModuleDefinition module)
		{
			// get every method with [Inline] on it
			var methods = module.GetTypes()
				.SelectMany(method => method.Methods)
				.Where(method => method.CustomAttributes
										.Any(attribute =>
				attribute.AttributeType.FullName == $"{typeof(InlineAttribute).FullName}"));

			foreach(var method in methods)
			{
				var modificationsToBeMade = new List<Modification>();

				foreach (var instruction in method.Body.Instructions)
				{
					HandleInstruction(module, instruction, ref modificationsToBeMade);
				}

				if(modificationsToBeMade.Count > 0)
				{
					foreach (var modification in modificationsToBeMade)
					{
						var il = method.Body.GetILProcessor();

						var last = modification.DeleteOpCodes.Last();

						foreach(var i in modification.ReplaceWith)
						{
							il.InsertAfter(last, i);
						}

						foreach(var i in modification.DeleteOpCodes)
						{
							il.Remove(i);
						}
					}
				}
			}
		}
	}
}
