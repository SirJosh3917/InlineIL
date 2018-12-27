# InlineIL
Write IL emissions directly in your code, without any need for Reflection.Emit

## Project Outline
This is a prototype/proof-of-concept library to write IL instructions inline with your C# code.

It uses Mono.Cecil to inspect wherever you use `IL.Emit`, and replace those calls with the actual opcode.

In order to do this on build, the ModifyAssembly project is called whenever UseModifiedTarget is built, and the modified DLL is copied over.