using dnlib.DotNet.Emit;
using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protection.Protections
{
    internal class Local2Field
    {
        private static Dictionary<Local, FieldDef> _convertedLocals = new Dictionary<Local, FieldDef>();

        public static void Execute(Context ctx)
        {
            foreach (var type in ctx.moduleDef.Types.Where(x => x != ctx.moduleDef.GlobalType))
            {
                foreach (var meth in type.Methods.Where(x => x.HasBody && x.Body.HasInstructions && !x.IsConstructor))
                {
                    _convertedLocals = new Dictionary<Local, FieldDef>();
                    Process(ctx.moduleDef, meth);
                }
            }
        }

        private static void Process(ModuleDef module, MethodDef meth)
        {
            meth.Body.SimplifyMacros(meth.Parameters);
            var instructions = meth.Body.Instructions;
            foreach (var t in instructions)
            {
                if (!(t.Operand is Local local)) continue;
                FieldDef def;
                if (!_convertedLocals.ContainsKey(local))
                {
                    def = new FieldDefUser(Renamer.GenerateName(), new FieldSig(local.Type), FieldAttributes.Public | FieldAttributes.Static);
                    module.GlobalType.Fields.Add(def);
                    _convertedLocals.Add(local, def);
                }
                else
                {
                    def = _convertedLocals[local];
                }

                var eq = t.OpCode?.Code;
                switch (eq)
                {
                    case Code.Ldloc:
                        t.OpCode = OpCodes.Ldsfld;
                        break;
                    case Code.Ldloca:
                        t.OpCode = OpCodes.Ldsflda;
                        break;
                    case Code.Stloc:
                        t.OpCode = OpCodes.Stsfld;
                        break;
                    default:
                        t.OpCode = null;
                        break;
                }
                t.Operand = def;

            }
            _convertedLocals.ToList().ForEach(x => meth.Body.Variables.Remove(x.Key));
            _convertedLocals = new Dictionary<Local, FieldDef>();
        }

    }
}
