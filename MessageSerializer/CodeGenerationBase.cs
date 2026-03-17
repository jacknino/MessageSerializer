using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Reflection;
using Microsoft.CSharp;

namespace MessageSerializer
{
    public class CodeGenerationBase
    {
        protected static int _fileNumber = 0;
        protected readonly string _baseClassName;

        protected CodeGenerationBase(string baseClassName)
        {
            _baseClassName = baseClassName;
        }

        public static bool WriteCodeAndDebugInfoToDisk { get; set; }

        protected CompilerResults CompileCode(Type type, CodeCompileUnit codeCompileUnit)
        {
            // Note: The _fileNumber was added because of the unit tests
            // There were problems because the unit tests would create the class based on the attributes
            // and then create the class based on creating a file
            // This all worked fine except when debugging things related to that the .pdb file for
            // the original way the class was created would be in use when trying to create the class
            // the second way.  By adding the _fileNumber this means that if things are recreated
            // the files will be different
            string codeOutputFilename = _baseClassName + type.Name + "Code" + _fileNumber++ + ".cs";

            //using (CSharpCodeProvider provider = new CSharpCodeProvider())
            using (CodeDomProvider provider = CodeDomProviderFactory.Create())
            {
                CompilerParameters compilerParameters = new CompilerParameters();
                OptionallyWriteCodeAndDebugInfoToDisk(codeCompileUnit, codeOutputFilename, provider, compilerParameters);



                // Think can do a couple things here:
                // This page has a simple thing that uses Roslyn with a CodeDomProvider and an ICodeCompiler
                // https://github.com/jaredpar/roslyn-codedom/blob/master/src/Roslyn.CodeDom/RoslynCodeDomProvider.cs
                // CSharpCodeProvider is also a CodeDomProvider that under decompilation seems to use
                // CSharpCodeGenerator which is an ICodeCompiler and an ICodeGenerator.
                // The ICodeGenerator part seems to still work for creating the code and outputting to a file
                // so think can still use that to generate the code we need as it can take a stream
                // Then we can use the example ICodeCompiler or something similar from above to take the
                // value from this stream and compile it.  We probably want this stream to be the same for both
                // creating the output file and for doing the compilation.
                // Also just noticed the CreateCompiler() and CreateGenerator() from the example and the generator
                // uses the CSharpCodeProvider.CreateGenerator()
                // I'm guessing there is a way to figure out which framework we are currently running in to decide
                // which provider we want to use
                CompilerResults results = provider.CompileAssemblyFromDom(compilerParameters, codeCompileUnit);
                ThrowExceptionOnCompileError(results);

                return results;
            }
        }

        //protected void OptionallyWriteCodeAndDebugInfoToDisk(CodeCompileUnit codeCompileUnit, string codeOutputFilename, CSharpCodeProvider provider, CompilerParameters compilerParameters)
        protected void OptionallyWriteCodeAndDebugInfoToDisk(CodeCompileUnit codeCompileUnit, string codeOutputFilename, CodeDomProvider provider, CompilerParameters compilerParameters)
        {
            if (WriteCodeAndDebugInfoToDisk)
            {
                OptionallyWriteCodeToDisk(codeOutputFilename, codeCompileUnit, provider);

                if (!string.IsNullOrEmpty(codeOutputFilename))
                {
                    compilerParameters.IncludeDebugInformation = true;
                    int dotPosition = codeOutputFilename.LastIndexOf('.');
                    string debugFileName = dotPosition > 0 ? codeOutputFilename.Substring(0, dotPosition) : codeOutputFilename;
                    compilerParameters.CompilerOptions = "/pdb:" + string.Format("\"{0}\"", Path.Combine(MyDirectory, debugFileName + ".pdb"));
                }
            }
        }

        protected void OptionallyWriteCodeToDisk(string codeOutputFilename, CodeCompileUnit codeCompileUnit, CodeDomProvider provider)
        {
            if (codeOutputFilename != "")
            {
                using (TextWriter writer = File.CreateText(Path.Combine(MyDirectory, codeOutputFilename)))
                {
                    ICodeGenerator codeGenerator = provider.CreateGenerator(writer);
                    CodeGeneratorOptions options = GetCodeGeneratorOptions();

                    codeGenerator.GenerateCodeFromCompileUnit(codeCompileUnit, writer, options);

                    writer.Close();
                }
            }
        }

        protected CodeGeneratorOptions GetCodeGeneratorOptions()
        {
            CodeGeneratorOptions codeGeneratorOptions = new CodeGeneratorOptions();
            codeGeneratorOptions.BracingStyle = "C";
            codeGeneratorOptions.BlankLinesBetweenMembers = false;

            return codeGeneratorOptions;
        }

        protected void ThrowExceptionOnCompileError(CompilerResults results)
        {
            if (results.Errors.HasErrors)
            {
                string description = "";

                foreach (CompilerError error in results.Errors)
                {
                    description += error.ErrorText;
                }

                throw new Exception(description);
            }
        }

        protected string MyDirectory
        {
            get
            {
                Assembly objAssembly = Assembly.GetAssembly(GetType());
                Uri objUri = new Uri(objAssembly.CodeBase);
                return Path.GetDirectoryName(objUri.LocalPath);
            }
        }
    }
}
