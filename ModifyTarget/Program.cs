using InlineIL;
using Mono.Cecil;
using System;
using System.IO;

//
// This will be run when we build UseModifiedTarget
// it modifies TargetAssembly
//

namespace ModifyTarget
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Replacing instructions in TargetAssembly");

			var fileName = args[0];
			var modifiedFileName = args[1];

			var module = ModuleDefinition.ReadModule(Path.GetFullPath(fileName));
			Modify.ReplaceEmits(module);
			module.Write(modifiedFileName);
		}
	}
}
