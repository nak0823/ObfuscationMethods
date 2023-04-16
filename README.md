# Obfuscation Methods

C# obfuscation is the process of making source code difficult to understand or reverse-engineer by modifying its structure, function names, variable names, and other metadata. The purpose of obfuscation is to protect intellectual property and prevent malicious actors from analyzing and exploiting vulnerabilities in the code. Obfuscation can also help to reduce the size of the compiled code by removing unnecessary metadata, making the application more efficient.

Here is an example of obfuscation in C#. The code snippet below demonstrates how the method and variable names can be obfuscated:

**Before:**
```cs
public class ExampleClass
{
    private string _exampleVariable = "Hello, World!";

    public void ExampleMethod(int exampleParameter)
    {
        int result = exampleParameter * 2;
        Console.WriteLine(_exampleVariable + " " + result);
    }
}
```

**After:**
```cs
public class A
{
    private string a = "Hello, World!";

    public void B(int a)
    {
        int b = a * 2;
        Console.WriteLine(this.a + " " + b);
    }
}

```

## How to apply these obfuscation techniques to your own program

To apply these obfuscation methods to your own program, first load in your assembly using the `dnlib` library in C# as shown below:

```cs
using dnlib.DotNet;

AssemblyDef assembly = AssemblyDef.Load(InputFile);
Context context = new Context(assembly);
context.moduleDefMD = ModuleDefMD.Load(InputFile);
```

Next, set up a Context object as shown in the code below:

```cs
public class Context
{
    public AssemblyDef assemblyDef;
    public ModuleDef moduleDef;
    public ModuleDefMD moduleDefMD;
    public TypeDef typeDef;
    public Importer importer;
    public MethodDef cctor;

    public Context(AssemblyDef asm)
    {
        assemblyDef = asm;
        moduleDef = asm.ManifestModule;
        typeDef = moduleDef.GlobalType;
        importer = new Importer(moduleDef);
        cctor = typeDef.FindOrCreateStaticConstructor();
    }
}
```

Finally, use the ModuleWriterOptions class to specify the options for writing the obfuscated assembly to an output file as shown below:
```cs
using dnlib.DotNet.Writer;

var Options = new ModuleWriterOptions(assembly.ManifestModule);
Options.Logger = DummyLogger.NoThrowInstance;
assembly.Write(OutputFile, Options);
```
