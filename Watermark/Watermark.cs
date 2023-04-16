using dnlib.DotNet;

public static void Execute(Context ctx)
        {
            // The code modifies the metadata of the input assembly by changing the assembly name, module name, and assembly attributes, including the version number, title, description, company, product, and trademark.
            // Specifically, it changes the assembly description, company name, and product name attributes to reflect the use of your assembly. This is achieved using the dnlib library to access and modify the metadata of the assembly.
            // The purpose of this modification is to add a watermark to the assembly to indicate that it has been processed by an obfuscator.
            // These changes will be reflected in the generated assembly file.


            ctx.assemblyDef.Name = ""; // Assembly name
            ctx.moduleDef.Name = ""; // Moduledef name

            var asmAttribs = ctx.moduleDef.Assembly.CustomAttributes.Find("System.Reflection.AssemblyVersionAttribute");
            asmAttribs.ConstructorArguments[0] = new CAArgument(ctx.moduleDef.CorLibTypes.String, new UTF8String("")); // Version

            asmAttribs = ctx.moduleDef.Assembly.CustomAttributes.Find("System.Reflection.AssemblyTitleAttribute");
            asmAttribs.ConstructorArguments[0] = new CAArgument(ctx.moduleDef.CorLibTypes.String, new UTF8String("")); // Assembly title

            asmAttribs = ctx.moduleDef.Assembly.CustomAttributes.Find("System.Reflection.AssemblyDescriptionAttribute");
            asmAttribs.ConstructorArguments[0] = new CAArgument(ctx.moduleDef.CorLibTypes.String, new UTF8String("")); // Assembly description

            asmAttribs = ctx.moduleDef.Assembly.CustomAttributes.Find("System.Reflection.AssemblyConfigurationAttribute");
            asmAttribs.ConstructorArguments[0] = new CAArgument(ctx.moduleDef.CorLibTypes.String, new UTF8String("")); // Build configuration

            asmAttribs = ctx.moduleDef.Assembly.CustomAttributes.Find("System.Reflection.AssemblyCompanyAttribute");
            asmAttribs.ConstructorArguments[0] = new CAArgument(ctx.moduleDef.CorLibTypes.String, new UTF8String("")); // Custom company name

            asmAttribs = ctx.moduleDef.Assembly.CustomAttributes.Find("System.Reflection.AssemblyProductAttribute");
            asmAttribs.ConstructorArguments[0] = new CAArgument(ctx.moduleDef.CorLibTypes.String, new UTF8String("")); // Product name

            asmAttribs = ctx.moduleDef.Assembly.CustomAttributes.Find("System.Reflection.AssemblyTrademarkAttribute");
            asmAttribs.ConstructorArguments[0] = new CAArgument(ctx.moduleDef.CorLibTypes.String, new UTF8String("")); // Trademark attribute
        }