using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;

namespace InlineIL
{
	public struct Modification
	{
		public Instruction[] ReplaceWith;
		public Instruction[] DeleteOpCodes;
	}

	public static class Modify
	{
		private static bool IsLoadsOpcode(this Instruction instruction)
			=> instruction.OpCode.Code == Code.Ldsfld &&
			instruction.Operand is FieldReference field &&
			field.FieldType.FullName == typeof(OpCode).FullName;

		private static OpCode GetOpcode(this FieldReference opcodeField)
			=> (OpCode)typeof(OpCodes) // get the OpCodes.Whatever
			.GetField(opcodeField.Name)
			.GetValue(null);

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
					// candidate for an emission replace
					if (instruction.OpCode.Code == Code.Call &&
						instruction.Previous != null)
					{
						if (instruction.Previous.IsLoadsOpcode())
						{
							var fieldReference = (FieldReference)instruction.Previous.Operand;
							var opcode = fieldReference.GetOpcode();

							modificationsToBeMade.Add(new Modification
							{
								DeleteOpCodes = new Instruction[]
								{
									instruction.Previous,
									instruction,
								},
								ReplaceWith = new Instruction[]
								{
									Instruction.Create(opcode)
								}
							});
						}
					}
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
