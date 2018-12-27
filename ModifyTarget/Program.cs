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

			if (args.Length >= 2)
			{
				Replace(args[0], args[1]);
			}
			else
			{
				// set the ModifyTarget project as startup project to debug InlienIL
				Replace("TargetAssembly.dll", "TargetAssembly-Modified.dll");
			}
		}

		static void Replace(string fileName, string modifiedFileName)
		{
			var module = ModuleDefinition.ReadModule(Path.GetFullPath(fileName));
			Modify.ReplaceEmits(module);
			module.Write(modifiedFileName);
		}
	}
}
