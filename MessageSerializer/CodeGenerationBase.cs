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

            using (CSharpCodeProvider provider = new CSharpCodeProvider())
            {
                CompilerParameters compilerParameters = new CompilerParameters();
                OptionallyWriteCodeAndDebugInfoToDisk(codeCompileUnit, codeOutputFilename, provider, compilerParameters);

                CompilerResults results = provider.CompileAssemblyFromDom(compilerParameters, codeCompileUnit);
                ThrowExceptionOnCompileError(results);

                return results;
            }
        }

        protected void OptionallyWriteCodeAndDebugInfoToDisk(CodeCompileUnit codeCompileUnit, string codeOutputFilename, CSharpCodeProvider provider, CompilerParameters compilerParameters)
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

        protected void OptionallyWriteCodeToDisk(string codeOutputFilename, CodeCompileUnit codeCompileUnit, CSharpCodeProvider provider)
        {
            if (codeOutputFilename != "")
            {
                TextWriter writer = File.CreateText(Path.Combine(MyDirectory, codeOutputFilename));

                ICodeGenerator codeGenerator = provider.CreateGenerator(writer);
                CodeGeneratorOptions options = GetCodeGeneratorOptions();

                codeGenerator.GenerateCodeFromCompileUnit(codeCompileUnit, writer, options);

                writer.Close();
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
