# Resources

Resources are `.resx` files with string values that we use for error messages and such.
They live in `src\Engine\resources` and `src\Rules\resources` folders.

At the moment `dotnet cli` doesn't support automatically generating C# bindings (strongly typed resource files).
Script analyzer resources are created as part of the regular build via a Target dependency in `Engine/Engine.csproj`.

If you see compilation errors related to resources, you can explicitly create the resource files as follows:

```
PS> Set-Location ResGen
PS> dotnet run
```

This will create compilable C# binding files in respective `gen` directory, which will then be used during build.

## Editing `.resx` files

**Do not edit** `.resx` files from Visual Studio. 
It will try to create `.cs` files for you and you will get whole bunch of hard-to-understand errors.

To edit a resource file, use any **plain text editor**. 
A resource file is a simple XML file, and it's easy to edit.
