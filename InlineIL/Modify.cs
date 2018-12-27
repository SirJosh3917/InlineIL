using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Collections.Generic;
using System.Linq;

namespace InlineIL
{
	public struct Modification
	{
		public Instruction emits;
		public Instruction replace;
	}

	public static class Modify
	{
		public static void ReplaceEmits(ModuleDefinition module)
		{
			// get every method with [Inline] on it
			var methods = module.GetTypes()
				.SelectMany(method => method.Methods)
				.Where(method => method.CustomAttributes
										.Any(attribute =>
				attribute.AttributeType.FullName == $"{nameof(InlineIL)}.{nameof(InlineAttribute)}"));

			foreach(var method in methods)
			{
				var modificationsToBeMade = new List<Modification>();

				foreach (var instruction in method.Body.Instructions)
				{
					if (instruction.Next != null && // next isn't null
						instruction.Next.OpCode.Code == Code.Call && // it's a call
						instruction.OpCode.Code == Code.Ldsfld && // loading a field
						instruction.Operand is FieldReference field && // is field reference
						field.FieldType.FullName == typeof(OpCode).FullName)
					{
						var opcode = (OpCode)typeof(OpCodes) // get the OpCodes.Whatever
							.GetField(field.Name)
							.GetValue(null);

						modificationsToBeMade.Add(new Modification
						{
							replace = instruction,
							emits = Instruction.Create(opcode)
						});
					}
				}

				if(modificationsToBeMade.Count > 0)
				{
					foreach (var modification in modificationsToBeMade)
					{
						var il = method.Body.GetILProcessor();

						// TODO: don't make so many assumptions
						il.Remove(modification.replace.Next);

						il.InsertAfter(modification.replace, modification.emits);

						il.Remove(modification.replace);
					}
				}
			}
		}
	}
}
