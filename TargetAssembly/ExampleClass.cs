using InlineIL;
using Mono.Cecil.Cil;
using System;
using System.Reflection;

//
// this is our TargetAssembly
// InlineIL will replace the IL.Emit instructions with their actual IL counterparts
// and you can reference this project normally.
//

namespace TargetAssembly
{
	public static class ExampleClass
	{
		[ParameterPass("1")] private static MethodInfo _WriteLine()
			=> typeof(Console).GetMethod(nameof(Console.WriteLine), BindingFlags.Public | BindingFlags.Static, Type.DefaultBinder, new Type[] { typeof(string) }, null);

		[Inline]
		public static void PrintStuff()
		{
			Console.WriteLine("Hello World!");
			Console.WriteLine("The line after the return won't execute.");

			IL.Emit(OpCodes.Ldstr, "But this will print to the console!");
			IL.EmitParameterPass(OpCodes.Call, "1");

			IL.Emit(OpCodes.Ret);

			Console.WriteLine("This shouldn't happen!");
		}
	}
}
