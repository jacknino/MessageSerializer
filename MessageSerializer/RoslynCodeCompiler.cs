using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace MessageSerializer
{
    public class RoslynCodeCompiler : ICodeCompiler
    {
        protected ICodeGenerator _codeGenerator;

        public RoslynCodeCompiler(ICodeGenerator codeGenerator) 
        { 
            _codeGenerator = codeGenerator;
        }

        public CompilerResults CompileAssemblyFromDom(CompilerParameters compilerParameters, CodeCompileUnit codeCompileUnit)
        {
            return CompileAssemblyFromDomBatch(compilerParameters, new[] { codeCompileUnit });
        }

        //public CompilerResults CompileAssemblyFromDomBatch(CompilerParameters options, CodeCompileUnit[] compilationUnits)
        //{
        //    CompilerResults compilerResults = null;
        //    CodeGeneratorOptions codeGeneratorOptions = new CodeGeneratorOptions();

        //    foreach (CodeCompileUnit codeCompileUnit in compilationUnits)
        //    {
        //        // TODO: What we should be doing is creating an array of sources and then calling CompileAssemblyFromSources
        //        using (TextWriter writer = new StringWriter())
        //        {
        //            _codeGenerator.GenerateCodeFromCompileUnit(codeCompileUnit, writer, codeGeneratorOptions);
        //            compilerResults = CompileAssemblyFromSource(options, writer.ToString());
        //            writer.Close();
        //        }
        //    }

        //    return compilerResults;
        //}

        public CompilerResults CompileAssemblyFromDomBatch(CompilerParameters compilerParameters, CodeCompileUnit[] codeCompileUnits)
        {
            CodeGeneratorOptions codeGeneratorOptions = new CodeGeneratorOptions();

            List<string> sourceStrings = new List<string>();
            foreach (CodeCompileUnit codeCompileUnit in codeCompileUnits)
            {
                compilerParameters.ReferencedAssemblies.AddRange(codeCompileUnit.ReferencedAssemblies.Cast<string>().ToArray());
                using (TextWriter writer = new StringWriter())
                {
                    _codeGenerator.GenerateCodeFromCompileUnit(codeCompileUnit, writer, codeGeneratorOptions);
                    sourceStrings.Add(writer.ToString());
                    writer.Close();
                }
            }

            CompilerResults compilerResults = CompileAssemblyFromSourceBatch(compilerParameters, sourceStrings.ToArray());
            return compilerResults;
        }

        public CompilerResults CompileAssemblyFromFile(CompilerParameters compilerParameters, string fileName)
        {
            throw new System.NotImplementedException();
        }

        public CompilerResults CompileAssemblyFromFileBatch(CompilerParameters compilerParameters, string[] fileNames)
        {
            throw new System.NotImplementedException();
        }

        public CompilerResults CompileAssemblyFromSource(CompilerParameters compilerParameters, string source)
        {
            return CompileAssemblyFromSourceBatch(compilerParameters, new[] { source });
        }

        public CompilerResults CompileAssemblyFromSourceBatch(CompilerParameters compilerParameters, string[] sources)
        {
            List<MetadataReference> references = new List<MetadataReference>();
            foreach(string referencedAssembly in compilerParameters.ReferencedAssemblies)
            {
                references.Add(MetadataReference.CreateFromFile(referencedAssembly));
            }

            var compilation = CSharpCompilation
                .Create(
                    Path.GetFileName(compilerParameters.OutputAssembly),
                    syntaxTrees: sources.Select(x => CSharpSyntaxTree.ParseText(x)),
                    references);
            //.WithFrameworkReferences(TargetFramework);

            var compilerResults = new CompilerResults(new TempFileCollection());
            AppendDiagnostics(compilation.GetDiagnostics());

            //using (FileStream stream = new FileStream(options.OutputAssembly, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            using (MemoryStream stream = new MemoryStream())
            {
                var emitResult = compilation.Emit(stream);
                //stream.Close();

                if (emitResult.Success)
                {
                    compilerResults.NativeCompilerReturnValue = 0;
                    if (compilerParameters.GenerateInMemory)
                    {
                        //var bytes = File.ReadAllBytes(options.OutputAssembly);
                        byte[] bytes = stream.ToArray();
                        compilerResults.CompiledAssembly = Assembly.Load(bytes);
                    }
                }
                else
                {
                    compilerResults.NativeCompilerReturnValue = 0;
                    AppendDiagnostics(emitResult.Diagnostics);
                }

                stream.Close();
                return compilerResults;
            }

            void AppendDiagnostics(IEnumerable<Diagnostic> diagnostics)
            {
                foreach (var diagnostic in diagnostics)
                {
                    var error = new CompilerError(
                        diagnostic.Location.SourceTree?.FilePath,
                        line: diagnostic.Location.GetLineSpan().StartLinePosition.Line,
                        column: diagnostic.Location.GetLineSpan().StartLinePosition.Character,
                        errorNumber: diagnostic.Id,
                        errorText: diagnostic.GetMessage());
                    compilerResults.Errors.Add(error);
                }
            }
        }
    }
}

