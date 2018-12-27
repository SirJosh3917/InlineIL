using InlineIL;
using Mono.Cecil.Cil;
using System;

//
// this is our TargetAssembly
// InlineIL will replace the IL.Emit instructions with their actual IL counterparts
// and you can reference this project normally.
//

namespace TargetAssembly
{
	public static class ExampleClass
	{
		[Inline]
		public static void PrintStuff()
		{
			Console.WriteLine("Hello World!");
			Console.WriteLine("The next line shouldn't execute.");

			IL.Emit(OpCodes.Ret);

			Console.WriteLine("This shouldn't happen!");
		}
	}
}
