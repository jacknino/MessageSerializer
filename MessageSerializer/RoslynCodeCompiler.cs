using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
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

        IEnumerable<MetadataReference> GetRoslynRefs(IEnumerable<string> referencedAssemblies)
        {
            foreach (var referencedAssembly in referencedAssemblies)
            {
                string path = referencedAssembly;
                if (!Path.IsPathRooted(path))
                {
                    // try already-loaded assembly by simple name
                    Assembly assembly = AppDomain.CurrentDomain.GetAssemblies()
                        .FirstOrDefault(a => a.GetName().Name.Equals(Path.GetFileNameWithoutExtension(referencedAssembly), StringComparison.OrdinalIgnoreCase));
                    if (assembly != null)
                    {
                        path = assembly.Location;
                    }
                    else
                    {
                        // try load by name (Framework) or fallback to runtime dir
                        try 
                        { 
                            path = Assembly.Load(new AssemblyName(Path.GetFileNameWithoutExtension(referencedAssembly))).Location; 
                        } 
                        catch 
                        { 
                        }

                        if (!File.Exists(path))
                            path = Path.Combine(System.Runtime.InteropServices.RuntimeEnvironment.GetRuntimeDirectory(), referencedAssembly);
                    }
                }

                if (File.Exists(path)) 
                    yield return MetadataReference.CreateFromFile(path);
                else 
                    throw new FileNotFoundException($"Reference not found: {referencedAssembly}");
            }
        }

        public CompilerResults CompileAssemblyFromSourceBatch(CompilerParameters compilerParameters, string[] sources)
        {
            //List<MetadataReference> metadataReferences = new List<MetadataReference>();
            //foreach (string referencedAssembly in compilerParameters.ReferencedAssemblies)
            //{
            //    metadataReferences.Add(MetadataReference.CreateFromFile(referencedAssembly));
            //}

            //List<MetadataReference> metadataReferences = GetRoslynRefs(compilerParameters.ReferencedAssemblies.Cast<string>()).ToList();
            string[] trustedPlatformAssemblies = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")).Split(Path.PathSeparator);
            var metadataReferences = trustedPlatformAssemblies.Select(p => MetadataReference.CreateFromFile(p)).ToList();
            var compilation = CSharpCompilation
                .Create(
                    Path.GetFileName(compilerParameters.OutputAssembly),
                    sources.Select(x => CSharpSyntaxTree.ParseText(x)),
                    metadataReferences,
                    new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            //.WithFrameworkReferences(TargetFramework);

            var compilerResults = new CompilerResults(new TempFileCollection());
            AppendDiagnostics(compilation.GetDiagnostics());

            compilerParameters.GenerateInMemory = true;

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

