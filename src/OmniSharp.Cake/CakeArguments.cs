using Cake.Core;

namespace OmniSharp.Cake
{
    public class CakeArguments : ICakeArguments
    {
        public bool HasArgument(string name)
        {
            return false;
        }

        public string GetArgument(string name)
        {
            return string.Empty;
        }
    }
}
