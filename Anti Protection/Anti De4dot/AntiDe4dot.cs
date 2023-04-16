using dnlib.DotNet;

namespace Protector.Protections
{
    internal class AntiDe4dot
    {
        public static void Execute(Context context)
        {
            // Adds invalid interface implementations to crash De4dot

            foreach (ModuleDef module in context.assemblyDef.Modules)
            {
                InterfaceImplUser invalidImpl = new InterfaceImplUser(module.GlobalType);

                for (int i = 100; i < 150; i++)
                {
                    TypeDefUser invalidType = new TypeDefUser("", Renamer.GenerateName(), module.CorLibTypes.GetTypeRef("System", "Attribute"));
                    InterfaceImplUser typeImpl = new InterfaceImplUser(invalidType);
                    module.Types.Add(invalidType);
                    invalidType.Interfaces.Add(typeImpl);
                    invalidType.Interfaces.Add(invalidImpl);
                }
            }
        }
    }
}