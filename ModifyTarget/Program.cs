using InlineIL;
using Mono.Cecil;
using System;
using System.IO;
using System.Reflection;

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
			var fullFileName = Path.GetFullPath(fileName);
			var dllCopy = $"{fullFileName}-2.dll";

			if (File.Exists(dllCopy))
			{
				File.Delete(dllCopy);
			}

			File.Copy(fullFileName, dllCopy);

			var module = ModuleDefinition.ReadModule(fullFileName);
			Modify.ReplaceEmits(Assembly.LoadFrom(dllCopy), module);
			module.Write(modifiedFileName);
		}
	}
}
