using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;

namespace Protecter.Protections
{
    internal class FakeAttributes
    {
        public static void Execute(ModuleDef module)
        {

			List<string> list = new List<string>
            {
                "ZYXDNGuarder", // DnGuard
                "HVMRuntm.dll", // DnGuard   	
			};

            foreach (string s in list)
            {
                TypeDef typeDef = new TypeDefUser("Attributes", s, module.Import(typeof(Attribute)));
                typeDef.Attributes = TypeAttributes.NotPublic;
                module.Types.Add(typeDef);
            }
            TypeDef typeDef2 = module.Types[new Random().Next(0, module.Types.Count)];
        }
    }
}