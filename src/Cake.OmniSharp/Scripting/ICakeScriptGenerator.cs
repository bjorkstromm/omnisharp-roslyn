using Cake.Core.IO;

namespace Cake.OmniSharp.Scripting
{
    public interface ICakeScriptGenerator
    {
        CakeScript Generate(FilePath scriptPath);
    }
}