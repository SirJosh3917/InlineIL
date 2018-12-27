using System;

//
// we use TargetAssembly just like normal in our project.
// nothing weird appears to happen to the caller of it
//
// but the caller is actually using the *modified* TargetAssembly
// so the IL.Emit(...)s will turn into IL instructions
//
// in the build process, we call "ModifyTarget"
// that will produce a TargetAssembly-Modified.dll
// which we be copied over before we run UseModifiedTarget.
//

namespace UseModifiedTarget
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Running TargetAssembly's PrintStuff");
			Console.WriteLine();

			TargetAssembly.ExampleClass.PrintStuff();

			Console.ReadLine();
		}
	}
}
