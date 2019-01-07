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
		private const string Console_WriteLine = "1";

		[ParameterPass(Console_WriteLine)] private static MethodInfo _WriteLine()
			=> typeof(Console).GetMethod(nameof(Console.WriteLine), BindingFlags.Public | BindingFlags.Static, Type.DefaultBinder, new Type[] { typeof(string) }, null);

		[Inline]
		public static void PrintStuff()
		{
			Console.WriteLine("Hello World!");

			IL.Emit(OpCodes.Ldstr, "But this will print to the console!");
			IL.EmitParameterPass(OpCodes.Call, Console_WriteLine);
			IL.Emit(OpCodes.Ret);

			Console.WriteLine("This shouldn't happen!");
		}

		[Inline]
		public static void ThrowInteger()
		{
			IL.EmitThrow(1);
		}
	}
}
