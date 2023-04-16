using dnlib.DotNet.Emit;
using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Numerics;

namespace Protector.Protections.Injections
{
    internal class InjectHelper
    {
        private static TypeDefUser Clone(TypeDef origin)
        {
            TypeDefUser typeDefUser = new TypeDefUser(origin.Namespace, origin.Name);
            typeDefUser.Attributes = origin.Attributes;
            bool flag = origin.ClassLayout != null;
            if (flag)
            {
                typeDefUser.ClassLayout = new ClassLayoutUser(origin.ClassLayout.PackingSize, origin.ClassSize);
            }
            foreach (GenericParam genericParam in origin.GenericParameters)
            {
                typeDefUser.GenericParameters.Add(new GenericParamUser(genericParam.Number, genericParam.Flags, "-"));
            }
            return typeDefUser;
        }

        private static MethodDefUser Clone(MethodDef origin)
        {
            MethodDefUser methodDefUser = new MethodDefUser(origin.Name, null, origin.ImplAttributes, origin.Attributes);
            foreach (GenericParam genericParam in origin.GenericParameters)
            {
                methodDefUser.GenericParameters.Add(new GenericParamUser(genericParam.Number, genericParam.Flags, "-"));
            }
            return methodDefUser;
        }

        private static FieldDefUser Clone(FieldDef origin)
        {
            return new FieldDefUser(origin.Name, null, origin.Attributes);
        }

        private static TypeDef PopulateContext(TypeDef typeDef, InjectHelper.InjectContext ctx)
        {
            IDnlibDef dnlibDef;
            bool flag = !ctx.map.TryGetValue(typeDef, out dnlibDef);
            TypeDef typeDef2;
            if (flag)
            {
                typeDef2 = InjectHelper.Clone(typeDef);
                ctx.map[typeDef] = typeDef2;
            }
            else
            {
                typeDef2 = (TypeDef)dnlibDef;
            }
            foreach (TypeDef typeDef3 in typeDef.NestedTypes)
            {
                typeDef2.NestedTypes.Add(InjectHelper.PopulateContext(typeDef3, ctx));
            }
            foreach (MethodDef methodDef in typeDef.Methods)
            {
                typeDef2.Methods.Add((MethodDef)(ctx.map[methodDef] = InjectHelper.Clone(methodDef)));
            }
            foreach (FieldDef fieldDef in typeDef.Fields)
            {
                typeDef2.Fields.Add((FieldDef)(ctx.map[fieldDef] = InjectHelper.Clone(fieldDef)));
            }
            return typeDef2;
        }

        private static void CopyTypeDef(TypeDef typeDef, InjectHelper.InjectContext ctx)
        {
            TypeDef typeDef2 = (TypeDef)ctx.map[typeDef];
            typeDef2.BaseType = ctx.Importer.Import(typeDef.BaseType);
            foreach (InterfaceImpl interfaceImpl in typeDef.Interfaces)
            {
                typeDef2.Interfaces.Add(new InterfaceImplUser(ctx.Importer.Import(interfaceImpl.Interface)));
            }
        }

        private static void CopyMethodDef(MethodDef methodDef, InjectHelper.InjectContext ctx)
        {
            MethodDef methodDef2 = (MethodDef)ctx.map[methodDef];
            methodDef2.Signature = ctx.Importer.Import(methodDef.Signature);
            methodDef2.Parameters.UpdateParameterTypes();
            bool flag = methodDef.ImplMap != null;
            if (flag)
            {
                methodDef2.ImplMap = new ImplMapUser(new ModuleRefUser(ctx.TargetModule, methodDef.ImplMap.Module.Name), methodDef.ImplMap.Name, methodDef.ImplMap.Attributes);
            }
            foreach (CustomAttribute customAttribute in methodDef.CustomAttributes)
            {
                methodDef2.CustomAttributes.Add(new CustomAttribute((ICustomAttributeType)ctx.Importer.Import(customAttribute.Constructor)));
            }
            bool hasBody = methodDef.HasBody;
            if (hasBody)
            {
                methodDef2.Body = new CilBody(methodDef.Body.InitLocals, new List<Instruction>(), new List<ExceptionHandler>(), new List<Local>());
                methodDef2.Body.MaxStack = methodDef.Body.MaxStack;
                Dictionary<object, object> bodyMap = new Dictionary<object, object>();
                foreach (Local local in methodDef.Body.Variables)
                {
                    Local local2 = new Local(ctx.Importer.Import(local.Type));
                    methodDef2.Body.Variables.Add(local2);
                    local2.Name = local.Name;
                    bodyMap[local] = local2;
                }
                foreach (Instruction instruction in methodDef.Body.Instructions)
                {
                    Instruction instruction2 = new Instruction(instruction.OpCode, instruction.Operand);
                    instruction2.SequencePoint = instruction.SequencePoint;
                    bool flag2 = instruction2.Operand is IType;
                    if (flag2)
                    {
                        instruction2.Operand = ctx.Importer.Import((IType)instruction2.Operand);
                    }
                    else
                    {
                        bool flag3 = instruction2.Operand is IMethod;
                        if (flag3)
                        {
                            instruction2.Operand = ctx.Importer.Import((IMethod)instruction2.Operand);
                        }
                        else
                        {
                            bool flag4 = instruction2.Operand is IField;
                            if (flag4)
                            {
                                instruction2.Operand = ctx.Importer.Import((IField)instruction2.Operand);
                            }
                        }
                    }
                    methodDef2.Body.Instructions.Add(instruction2);
                    bodyMap[instruction] = instruction2;
                }

                Func<Instruction, Instruction> instructionSelector = null;
                foreach (Instruction instruction3 in methodDef2.Body.Instructions)
                {
                    bool flag5 = instruction3.Operand != null && bodyMap.ContainsKey(instruction3.Operand);
                    if (flag5)
                    {
                        instruction3.Operand = bodyMap[instruction3.Operand];
                    }
                    else
                    {
                        bool flag6 = instruction3.Operand is Instruction[];
                        if (flag6)
                        {
                            Instruction instruction4 = instruction3;
                            IEnumerable<Instruction> source = (Instruction[])instruction3.Operand;
                            if (instructionSelector == null)
                            {
                                instructionSelector = (Instruction target) => (Instruction)bodyMap[target];
                            }
                            instruction4.Operand = source.Select(instructionSelector).ToArray<Instruction>();
                        }
                    }
                }


                foreach (ExceptionHandler exceptionHandler in methodDef.Body.ExceptionHandlers)
                {
                    methodDef2.Body.ExceptionHandlers.Add(new ExceptionHandler(exceptionHandler.HandlerType)
                    {
                        CatchType = ((exceptionHandler.CatchType == null) ? null : ctx.Importer.Import(exceptionHandler.CatchType)),
                        TryStart = (Instruction)bodyMap[exceptionHandler.TryStart],
                        TryEnd = (Instruction)bodyMap[exceptionHandler.TryEnd],
                        HandlerStart = (Instruction)bodyMap[exceptionHandler.HandlerStart],
                        HandlerEnd = (Instruction)bodyMap[exceptionHandler.HandlerEnd],
                        FilterStart = ((exceptionHandler.FilterStart == null) ? null : ((Instruction)bodyMap[exceptionHandler.FilterStart]))
                    });
                }
                methodDef2.Body.SimplifyMacros(methodDef2.Parameters);
            }
        }

        private static void CopyFieldDef(FieldDef fieldDef, InjectHelper.InjectContext ctx)
        {
            FieldDef fieldDef2 = (FieldDef)ctx.map[fieldDef];
            fieldDef2.Signature = ctx.Importer.Import(fieldDef.Signature);
        }
        private static void Copy(TypeDef typeDef, InjectHelper.InjectContext ctx, bool copySelf)
        {
            if (copySelf)
            {
                InjectHelper.CopyTypeDef(typeDef, ctx);
            }
            foreach (TypeDef typeDef2 in typeDef.NestedTypes)
            {
                InjectHelper.Copy(typeDef2, ctx, true);
            }
            foreach (MethodDef methodDef in typeDef.Methods)
            {
                InjectHelper.CopyMethodDef(methodDef, ctx);
            }
            foreach (FieldDef fieldDef in typeDef.Fields)
            {
                InjectHelper.CopyFieldDef(fieldDef, ctx);
            }
        }


        public static TypeDef Inject(TypeDef typeDef, ModuleDef target)
        {
            InjectHelper.InjectContext injectContext = new InjectHelper.InjectContext(typeDef.Module, target);
            InjectHelper.PopulateContext(typeDef, injectContext);
            InjectHelper.Copy(typeDef, injectContext, true);
            return (TypeDef)injectContext.map[typeDef];
        }

        public static MethodDef Inject(MethodDef methodDef, ModuleDef target)
        {
            InjectHelper.InjectContext injectContext = new InjectHelper.InjectContext(methodDef.Module, target);
            injectContext.map[methodDef] = InjectHelper.Clone(methodDef);
            InjectHelper.CopyMethodDef(methodDef, injectContext);
            return (MethodDef)injectContext.map[methodDef];
        }

        public static IEnumerable<IDnlibDef> Inject(TypeDef typeDef, TypeDef newType, ModuleDef target)
        {
            InjectHelper.InjectContext injectContext = new InjectHelper.InjectContext(typeDef.Module, target);
            injectContext.map[typeDef] = newType;
            InjectHelper.PopulateContext(typeDef, injectContext);
            InjectHelper.Copy(typeDef, injectContext, false);
            return injectContext.map.Values.Except(new TypeDef[]
            {
                newType
            });
        }

        private class InjectContext : ImportMapper
        {
            public InjectContext(ModuleDef module, ModuleDef target)
            {
                this.OriginModule = module;
                this.TargetModule = target;
                this.importer = new Importer(target, ImporterOptions.TryToUseTypeDefs, default(GenericParamContext), this);
            }

            public Importer Importer
            {
                get
                {
                    return this.importer;
                }
            }

            public override ITypeDefOrRef Map(ITypeDefOrRef typeDefOrRef)
            {
                TypeDef typeDef = typeDefOrRef as TypeDef;
                bool flag = typeDef != null;
                if (flag)
                {
                    bool flag2 = this.map.ContainsKey(typeDef);
                    if (flag2)
                    {
                        return (TypeDef)this.map[typeDef];
                    }
                }
                return null;
            }

            public override IMethod Map(MethodDef methodDef)
            {
                bool flag = this.map.ContainsKey(methodDef);
                IMethod result;
                if (flag)
                {
                    result = (MethodDef)this.map[methodDef];
                }
                else
                {
                    result = null;
                }
                return result;
            }

            public override IField Map(FieldDef fieldDef)
            {
                bool flag = this.map.ContainsKey(fieldDef);
                IField result;
                if (flag)
                {
                    result = (FieldDef)this.map[fieldDef];
                }
                else
                {
                    result = null;
                }
                return result;
            }

           
            public readonly Dictionary<IDnlibDef, IDnlibDef> map = new Dictionary<IDnlibDef, IDnlibDef>();

            public readonly ModuleDef OriginModule;

            public readonly ModuleDef TargetModule;

            private readonly Importer importer;
        }
    }
}
