using System;
using dnlib.DotNet;

namespace Protector
{
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
}
