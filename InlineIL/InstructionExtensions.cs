using Mono.Cecil;
using Mono.Cecil.Cil;

namespace InlineIL
{
	public static class InstructionExtensions
	{
		public static bool IsLoadsOpcode(this Instruction instruction)
			=> instruction.OpCode.Code == Code.Ldsfld &&
			instruction.Operand is FieldReference field &&
			field.FieldType.FullName == typeof(OpCode).FullName;

		public static bool IsLoadStringOpcode(this Instruction instruction)
			=> instruction.OpCode.Code == Code.Ldstr;

		public static OpCode GetOpcode(this FieldReference opcodeField)
			=> (OpCode)typeof(OpCodes) // get the OpCodes.Whatever
			.GetField(opcodeField.Name)
			.GetValue(null);
	}
}
