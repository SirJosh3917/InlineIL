using Mono.Cecil;
using Mono.Cecil.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace InlineIL
{

	public static class Modify
	{
		public static void ReplaceEmits(Assembly assembly, ModuleDefinition module)
		{
			var methods = GetMethodsWithInlineAttribute(module);

			var materializations = GetMaterializationsOfMethodsWithParameterPass(assembly);

			foreach (var method in methods)
			{
				var modificationsToBeMade = new List<Modification>();

				foreach (var instruction in method.Body.Instructions)
				{
					HandleInstruction(module, materializations, instruction, ref modificationsToBeMade);
				}

				if (modificationsToBeMade.Count > 0)
				{
					WriteModifications(method, modificationsToBeMade);
				}
			}
		}

		private static IEnumerable<MethodDefinition> GetMethodsWithInlineAttribute(ModuleDefinition module)
		{
			return module.GetTypes()
						.SelectMany(type => type.Methods) // get all methods
						.Where // where theres InlineAttribute
						(
							method => method.CustomAttributes.Any
							(
								attribute => attribute.AttributeType.FullName == typeof(InlineAttribute).FullName
							)
						);
		}

		private static Dictionary<string, object> GetMaterializationsOfMethodsWithParameterPass(Assembly assembly)
		{
			return assembly.GetTypes()
							.SelectMany(type => type.GetMethods(BindingFlags.NonPublic | BindingFlags.Static)) // every private static method
							.Where // where there's ParameterPassAttribute
							(
								method => method.GetCustomAttributes(typeof(ParameterPassAttribute), false).Length > 0
							)

							.ToDictionary // materialize the id and return value of it
							(
								method => method // get id
									.GetCustomAttributes(false)
									.OfType<ParameterPassAttribute>()
									.First()
									.Id,

								method => method // and value
									.Invoke(null, new object[] { })
							);
		}

		private static void HandleInstruction(ModuleDefinition module, Dictionary<string, object> materializations, Instruction instruction, ref List<Modification> modificationsToBeMade)
		{
			// candidate for an emission replace ( IL.Emit(...) )
			if (instruction.OpCode.Code == Code.Call &&
				instruction.Previous != null && // always at least 1 parameter to be passed in is required
				instruction.Operand is Mono.Cecil.MethodReference methodReference &&
				methodReference.DeclaringType.FullName == typeof(IL).FullName) // some method within IL
			{
				if ((instruction.Previous?.IsLoadsOpcode() ?? false)) // emit(opcode)
				{
					HandleOpcode(module, instruction, modificationsToBeMade);
				}
				else if ((instruction.Previous?.IsLoadStringOpcode() ?? false) &&
					(instruction.Previous.Previous?.IsLoadsOpcode() ?? false))
				{
					HandleOpcodeString(module, materializations, instruction, modificationsToBeMade);
				}
				else
				{
					modificationsToBeMade = new List<Modification>
					{
						new Modification
						{
							DeleteOpCodes = new Instruction[] { instruction },
							Module = module,
							ReplaceWith = new Instruction[] { Instruction.Create(OpCodes.Throw) }
						}
					};
				}
			}
		}

		private static void WriteModifications(MethodDefinition method, List<Modification> modificationsToBeMade)
		{
				var il = method.Body.GetILProcessor();

				foreach (var modification in modificationsToBeMade)
				{
					// insert new IL after the last instruction to be deleted
					var last = modification.DeleteOpCodes.Last();

					foreach (var i in modification.ReplaceWith)
					{
						il.InsertAfter(last, i);
					}

					// and remove every instruction
					foreach (var i in modification.DeleteOpCodes)
					{
						il.Remove(i);
					}
				}
		}

		private static void HandleOpcode(ModuleDefinition module, Instruction instruction, List<Modification> modificationsToBeMade)
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

		private static void HandleOpcodeString(ModuleDefinition module, Dictionary<string, object> materializations, Instruction instruction, List<Modification> modificationsToBeMade)
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
				HandleLoadString(modificationsToBeMade, mod, opcode, strValue);
			}
			else // some kind of call
			{
				HandlePassParam(materializations, modificationsToBeMade, mod, opcode, strValue);
			}
		}

		private static void HandleLoadString(List<Modification> modificationsToBeMade, Modification mod, OpCode opcode, string strValue)
			=> modificationsToBeMade.Add(InstructionModifications.Emit(mod, opcode, strValue));

		private static void HandlePassParam(Dictionary<string, object> materializations, List<Modification> modificationsToBeMade, Modification mod, OpCode opcode, string strValue)
		{
			var material = materializations[strValue];

			modificationsToBeMade.Add
			(
				(Modification)typeof(InstructionModifications)
				.GetMethod(nameof(InstructionModifications.Emit), new Type[]
				{
								typeof(Modification),
								typeof(OpCode),
								material.GetType()
				})
				.Invoke(null, new object[] { mod, opcode, material })
			);
		}
	}
}
