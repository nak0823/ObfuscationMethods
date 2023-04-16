using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Protector.Protections
{
    internal class Credits
    {
        public static void Execute(Context ctx)
        {
            var cctor = ctx.typeDef.FindOrCreateStaticConstructor();

            var field = new FieldDefUser("Arizona", new FieldSig(ctx.moduleDef.CorLibTypes.String), FieldAttributes.Static | FieldAttributes.Private);
            ctx.moduleDef.GlobalType.Fields.Add(field);

            var ldstr = new Instruction(OpCodes.Ldstr, "Protector by nak0823 ~ Today is a Beautiful Day");
            var stsfld = new Instruction(OpCodes.Stsfld, field);

            cctor.Body.Instructions.Insert(0, ldstr);
            cctor.Body.Instructions.Insert(1, stsfld);
        }
    }
}