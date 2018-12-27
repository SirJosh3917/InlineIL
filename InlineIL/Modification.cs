using Mono.Cecil;
using Mono.Cecil.Cil;

namespace InlineIL
{
	public struct Modification
	{
		public ModuleDefinition Module;
		public Instruction[] ReplaceWith;
		public Instruction[] DeleteOpCodes;
	}
}
