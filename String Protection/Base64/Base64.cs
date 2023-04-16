using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Microsoft.SqlServer.Server;
using Satfuscator.Protections.Injections;
using static Satfuscator.Protections.StringEcrypt.StringEcnryption;

namespace Protector.Protections.StringEcrypt
{
    internal class Base64
    {

        public static void Execute(ModuleDef module)
        {

            int count_xxx = 0;
            ModuleDefMD typeModule = ModuleDefMD.Load(typeof(DecryptCrypto).Module);
            TypeDef typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(DecryptCrypto).MetadataToken));
            IEnumerable<IDnlibDef> members = InjectHelper.Inject(typeDef, module.GlobalType, module);
            MethodDef init = (MethodDef)members.Single(method => method.Name == "Decrypt");
            FieldDef finit = (FieldDef)members.Single(field => field.Name == "Key");
            FieldDef finit2 = (FieldDef)members.Single(field => field.Name == "IV");
            FieldDef finit4 = (FieldDef)members.Single(field => field.Name == "plaintext");
            FieldDef finit5 = (FieldDef)members.Single(field => field.Name == "cipherText");


            init.Name = Renamer.GenerateName();
            finit.Name = Renamer.GenerateName(); ;
            finit2.Name = Renamer.GenerateName();
            finit4.Name = Renamer.GenerateName();
            finit5.Name = Renamer.GenerateName();

            foreach (TypeDef type in module.Types)
            {
                if (type.IsGlobalModuleType)
                {
                    foreach (MethodDef method in type.Methods)
                    {
                        for (int i = 0; i < method.Body.Instructions.Count; i++)
                            if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr)
                            {
                                string operand = method.Body.Instructions[i].Operand.ToString();
                                if (operand == @"qwr{@^h`h&_`50/.a9!'dcmh3!uw<&=?")
                                {
                                    method.Body.Instructions[i].Operand = akey;
                                }
                                if (operand == @"9/\~V).A.lY&=t2b")
                                {
                                    method.Body.Instructions[i].Operand = aiv;
                                }
                            }
                    }
                }
            }

            /*for (var i = 0; i < 142; i++)//3285
            {
                TypeDef typeDef2 = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(FloodEx).MetadataToken));
                IEnumerable<IDnlibDef> members2 = InjectHelper.Inject(typeDef2, module.GlobalType, module);
                MethodDef init2 = (MethodDef)members2.Single(method => method.Name == "Decrypt");
                FieldDef finit42 = (FieldDef)members2.Single(field => field.Name == "plaintext");
                FieldDef finit52 = (FieldDef)members2.Single(field => field.Name == "cipherText");
                init2.Name = Undetected.RandomStringStrong(40);
                finit42.Name = Undetected.RandomStringStrong(41);
                finit52.Name = Undetected.RandomStringStrong(41);
            }*/

            foreach (MethodDef method in module.GlobalType.Methods)
                if (method.Name.Equals(".ctor"))
                {
                    module.GlobalType.Remove(method);
                    break;
                }

            foreach (TypeDef type in module.Types)
            {
                if (type.IsGlobalModuleType) continue;
                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody) continue;
                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                        if (method.Body.Instructions[i].OpCode == OpCodes.Ldstr)
                        {
                            string operand = method.Body.Instructions[i].Operand.ToString();
                            method.Body.Instructions[i].Operand = Encrypt(operand);
                            method.Body.Instructions.Insert(i + 1, OpCodes.Call.ToInstruction(init));
                            count_xxx += 1;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine("StringStrongerEnc: " + count_xxx);
                        }
                }
            }
        }

        private static string akey = Renamer.GenerateName();
        private static string aiv = Renamer.GenerateName();

        private static byte[] Key = Encoding.ASCII.GetBytes(akey);//32
        private static byte[] IV = Encoding.ASCII.GetBytes(aiv);//16


        public static string Encrypt(string plainText)
        {

            byte[] encrypted;

            using (AesCryptoServiceProvider aesAlg = new AesCryptoServiceProvider())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Mode = CipherMode.CBC;
                aesAlg.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(encrypted);
        }


    }

    public static class DecryptCrypto
    {
        private static byte[] Key = Encoding.ASCII.GetBytes(@"qwr{@^h`h&_`50/.a9!'dcmh3!uw<&=?");
        private static byte[] IV = Encoding.ASCII.GetBytes(@"9/\~V).A.lY&=t2b");
        //static AesCryptoServiceProvider aesAlg2 = new AesCryptoServiceProvider();
        static string plaintext = null;
        private static byte[] cipherText;
        //private static ICryptoTransform decryptor;
        //private static MemoryStream msDecrypt;
        //private static CryptoStream csDecrypt;
        //private static StreamReader srDecrypt;

        public static string Decrypt(string Text)
        {

            plaintext = null;
            cipherText = Convert.FromBase64String(Text.Replace(' ', '+'));

            using (AesCryptoServiceProvider aesAlg2 = new AesCryptoServiceProvider())
            {
                aesAlg2.Key = Key;
                aesAlg2.IV = IV;
                aesAlg2.Mode = CipherMode.CBC;
                aesAlg2.Padding = PaddingMode.PKCS7;


                ICryptoTransform decryptor = aesAlg2.CreateDecryptor(aesAlg2.Key, aesAlg2.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;
        }
    }
}
