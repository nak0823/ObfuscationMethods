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

## List of Available Protection Methods within this Repository

### **Controlflow** [🌀]
- Control Flow Obfuscation is a protection technique that alters the structure and order of instructions in a method's body to make it harder to understand and analyze the code flow. This can make it difficult for reverse engineers to understand the program's logic and make it harder to modify the code.

### **Anti Dump** [🗑️]
- Anti Dump is a protection method that aims to prevent attackers from dumping the assembly from the memory or disk, making it harder to analyze and reverse engineer the code. It can achieve this by decrypting parts of the code on the fly during runtime or by making it difficult to extract the original code from the binary file.

### **Method Hider** [💉]
- Method Hider is a code protection technique that obfuscates method names and their contents to make it harder for an attacker to reverse engineer and understand the code. It does this by renaming method names to random strings and inserting fake methods with similar names to confuse decompilers.

### **Anti De4dot** [🚫]
- This is a tool that can be used to deobfuscate .NET assemblies that have been obfuscated with de4dot. Anti-de4dot protections are intended to prevent this tool from successfully deobfuscating/renaming the code.

### **Anti Dnspy** [🕵️‍♀️]
- DnSpy is a popular .NET assembly editor and debugger. Anti-DnSpy protections are intended to prevent the use of DnSpy on the protected assembly, often by making the code difficult to read or by preventing the debugger from attaching to the process.

### **Anti Ildasm** [🔒]
-  Ildasm is a .NET disassembler that can be used to view the IL code of a .NET assembly. Anti-Ildasm protections are intended to prevent the use of Ildasm on the protected assembly, often by making the code difficult to read or by preventing the IL code from being generated in the first place.

### **Fake Attributes** [🎭]
- Adds fake attributes to confuse decompilers, DiE (Detect it Easy) and obfuscate code.

### **Import Protection** [📦]
- Modifies the import table to make it difficult for decompilers to resolve external dependencies.

### **Int Protection** [🔢]
- Replaces integers with expressions to make it harder for reverse engineers to read the code.

### **Invalid Opcodes** [🔢]
- Adds invalid opcodes to the IL code to confuse decompilers and make it harder to analyze the code.

### **Invalid Metadata** [🤖]
- Modifies the metadata of the assembly to make it harder for decompilers to read and analyze the code.

### **Junk Protection** [📝]
- Adds junk code to the assembly to increase its size and make it harder for reverse engineers to analyze the code.

### **Local 2 Field (L2F)** [🔍]
- Obfuscates local variables by converting them to fields.

### **Melting** [🔥]
- Disguises and hides the original program flow by merging it with other unrelated code.

### **Mutation** [🧬]
- Modifies the IL code to be functionally equivalent but harder to read and understand.

### **Proxy** [🕵️]
- Hides the original program by adding a proxy class and redirecting method calls to it.

### **Rename** [🔄]
- Hides the original program by adding a proxy class and redirecting method calls to it.

### **String: Cipher** [🔒]
- Encrypts strings to make it harder to read and modify. It works by encrypting the strings using an algorithm and storing the encrypted values in the assembly. When the program is executed, the encrypted strings are decrypted at runtime using the same algorithm.

### **String: Base64** [🔐]
- Encrypts strings to make it harder to read and modify.

### **Suf** [🌪️]
- Obfuscates the code by inserting confusing instructions and code paths.

### **Watermark** [📝]
- Adds a unique identifier or signature to the code to track it back to the original author or distributor.

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
