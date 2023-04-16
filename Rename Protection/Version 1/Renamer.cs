using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Protector.Protections
{
    internal class Renamer
    {
        public static int SpamCount
        {
            get
            {
                return 4;
            }
        }
        public static string InvisibleName
        {
            get
            {
                return GenerateName();
            }
        }

        private static readonly string[] WordList = new string[] {
            "AccessModifier",
            "ArrayType",
            "Assembly",
            "Attribute",
            "Boolean",
            "Byte",
            "Callback",
            "Char",
            "Checked",
            "Class",
            "Collection",
            "Compilation",
            "Constructor",
            "Delegate",
            "Derived",
            "Disposable",
            "Double",
            "Dynamic",
            "Enum",
            "Equals",
            "Event",
            "Exception",
            "Execute",
            "False",
            "Field",
            "Float",
            "Generic",
            "Hashtable",
            "Identity",
            "Implement",
            "Indexer",
            "Inherit",
            "Initialize",
            "Instance",
            "Interface",
            "Invalid",
            "Iterator",
            "KeyValuePair",
            "LinkedList",
            "List",
            "Literal",
            "Lock",
            "Long",
            "Managed",
            "Member",
            "Metadata",
            "Method",
            "Module",
            "Multicast",
            "Namespace",
            "Nested",
            "Nullable",
            "Object",
            "Operator",
            "Overload",
            "Override",
            "Parameter",
            "Parse",
            "Partial",
            "Platform",
            "Pointer",
            "Predicate",
            "Private",
            "Property",
            "Protected",
            "Public",
            "Query",
            "Random",
            "Readonly",
            "Refactoring",
            "Reflection",
            "Register",
            "Release",
            "Remove",
            "SafeHandle",
            "Scalar",
            "Sealed",
            "Section",
            "Serial",
            "Serialize",
            "Short",
            "Signed",
            "Single",
            "SizeOf",
            "Stack",
            "Static",
            "String",
            "Struct",
            "Subclass",
            "Subroutine",
            "Switch",
            "Synchronized",
            "Syntax",
            "Thread",
            "Throw",
            "True",
            "Type",
            "Typecasting",
            "Unbox",
            "Unicode",
            "Unchecked",
            "Unit",
            "Unsafe",
            "Unwrap",
            "Ushort",
            "Using",
            "Value",
            "Variable",
            "Variant",
            "Virtual",
            "Volatile",
            "WebClient",
            "While",
            "Xor",
            "Yield",
            "Zone"
        };
        public static string GenerateName()
        {
            StringBuilder nameBuilder = new StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                int index = Renamer.rnd.Next(WordList.Length);
                string word = WordList[index];
                nameBuilder.Append(word);
            }
            return nameBuilder.ToString();
        }

        private static Random rnd = new Random();
        public static void Execute(Context ctx)
        {
            string jName = null;
            foreach(ModuleDef module in ctx.assemblyDef.Modules)
            {
                foreach (TypeDef typeDef in ctx.moduleDef.Types)
                {
                    if (typeDef.IsPublic)
                        jName = typeDef.Name;

                    if (CanRename(typeDef))
                    {
                        foreach(MethodDef methodDef in typeDef.Methods)
                        {
                            if (Renamer.CanRename(methodDef))
                            {
                                TypeRef typeRef = ctx.moduleDef.CorLibTypes.GetTypeRef("System.Runtime.CompilerServices", "CompilerGeneratedAttribute");
                                MemberRefUser ctor = new MemberRefUser(ctx.moduleDef, ".ctor", MethodSig.CreateInstance(ctx.moduleDef.Import(typeof(void)).ToTypeSig(true)), typeRef);
                                CustomAttribute item = new CustomAttribute(ctor);
                                methodDef.CustomAttributes.Add(item);
                                methodDef.Name = InvisibleName;
                            }

                            foreach (Parameter parameter in methodDef.Parameters)
                            {
                                parameter.Name = Renamer.InvisibleName;
                            }
                        }
                        
                    }

                    foreach (FieldDef fieldDef in typeDef.Fields)
                    {
                        bool flag3 = Renamer.CanRename(fieldDef);
                        if (flag3)
                        {
                            fieldDef.Name = Renamer.InvisibleName;
                        }
                    }
                    foreach (EventDef eventDef in typeDef.Events)
                    {
                        bool flag4 = Renamer.CanRename(eventDef);
                        if (flag4)
                        {
                            eventDef.Name = Renamer.InvisibleName;
                        }
                    }
                    bool isPublic2 = typeDef.IsPublic;
                    if (isPublic2)
                    {
                        foreach (Resource resource in ctx.moduleDef.Resources)
                        {
                            bool flag5 = resource.Name.Contains(jName);
                            if (flag5)
                            {
                                resource.Name = resource.Name.Replace(jName, typeDef.Name);
                            }
                        }
                    }


                }
            }
        }

        private static bool CanRename(TypeDef type)
        {
            if (type.IsGlobalModuleType)
                return false;

            try
            {
                if (type.Name.Contains("My"))
                    return false;
            }
            catch (Exception ex)
            {
                Program.Logger(Color.Yellow, ex.Message);
            }

            if (type.Interfaces.Count > 0)
                return false;

            if (type.IsSpecialName)
                return false;

            if (type.IsRuntimeSpecialName)
                return false;

            bool isSat = type.Name.Contains("Sat");
            return !isSat;


        }

        private static bool CanRename(EventDef ev)
        {
            bool isForwarder = ev.DeclaringType.IsForwarder;
            bool result;
            if (isForwarder)
            {
                result = false;
            }
            else
            {
                bool isRuntimeSpecialName = ev.IsRuntimeSpecialName;
                result = !isRuntimeSpecialName;
            }
            return result;
        }
        private static bool CanRename(FieldDef field)
        {
            bool flag = field.IsLiteral && field.DeclaringType.IsEnum;
            bool result;
            if (flag)
            {
                result = false;
            }
            else
            {
                bool isForwarder = field.DeclaringType.IsForwarder;
                if (isForwarder)
                {
                    result = false;
                }
                else
                {
                    bool isRuntimeSpecialName = field.IsRuntimeSpecialName;
                    if (isRuntimeSpecialName)
                    {
                        result = false;
                    }
                    else
                    {
                        bool flag2 = field.IsLiteral && field.DeclaringType.IsEnum;
                        if (flag2)
                        {
                            result = false;
                        }
                        else
                        {
                            bool flag3 = field.Name.Contains("Sugar");
                            result = !flag3;
                        }
                    }
                }
            }
            return result;
        }


        private static bool CanRename(MethodDef method)
        {
            bool isConstructor = method.IsConstructor;
            bool result;
            if (isConstructor)
            {
                result = false;
            }
            else
            {
                bool isForwarder = method.DeclaringType.IsForwarder;
                if (isForwarder)
                {
                    result = false;
                }
                else
                {
                    bool isFamily = method.IsFamily;
                    if (isFamily)
                    {
                        result = false;
                    }
                    else
                    {
                        bool flag = method.IsConstructor || method.IsStaticConstructor;
                        if (flag)
                        {
                            result = false;
                        }
                        else
                        {
                            bool isRuntimeSpecialName = method.IsRuntimeSpecialName;
                            if (isRuntimeSpecialName)
                            {
                                result = false;
                            }
                            else
                            {
                                bool isForwarder2 = method.DeclaringType.IsForwarder;
                                if (isForwarder2)
                                {
                                    result = false;
                                }
                                else
                                {
                                    bool isGlobalModuleType = method.DeclaringType.IsGlobalModuleType;
                                    if (isGlobalModuleType)
                                    {
                                        result = false;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}