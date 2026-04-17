using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;

namespace MessageSerializer
{
    public class RoslynCSharpCodeProvider : CodeDomProvider
    {
        protected ICodeCompiler _codeCompiler;
        protected ICodeGenerator _codeGenerator;

        public RoslynCSharpCodeProvider()
        {
            // We can't just create a CSharpCodeGenerator because it is marked as internal
            // Since this is marked Obsolete and looking at the CSharpCodeProvider we could probably instead do:
            // public override void GenerateCodeFromMember(CodeTypeMember member, TextWriter writer, CodeGeneratorOptions options)
            // {
            //     generator.GenerateCodeFromMember(member, writer, options);
            // }
            // Except have the CSharpCodeProvider as a member and call the GeneratorCodeFromMember on that
#pragma warning disable CS0618 // Type or member is obsolete
            _codeGenerator = new CSharpCodeProvider().CreateGenerator();
#pragma warning restore CS0618 // Type or member is obsolete
            _codeCompiler = new RoslynCodeCompiler(_codeGenerator);
        }

        [Obsolete("Callers should not use the ICodeCompiler interface and should instead use the methods directly on the CodeDomProvider class.")]
        public override ICodeCompiler CreateCompiler() => _codeCompiler;

        [Obsolete]
        public override ICodeGenerator CreateGenerator() => _codeGenerator;
    }
}
