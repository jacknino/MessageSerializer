using System.CodeDom.Compiler;

namespace MessageSerializer
{
    public class CodeDomProviderFactory
    {
        public static CodeDomProvider Create()
        {
            return new RoslynCSharpCodeProvider();
        }
    }
}
