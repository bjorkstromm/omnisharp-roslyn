using System.Collections.Generic;
using Cake.Core.IO;
using Cake.Core.Packaging;

namespace Cake.OmniSharp
{
    public class CakePackageInstaller : IPackageInstaller
    {
        public bool CanInstall(PackageReference package, PackageType type)
        {
            return false;
        }

        public IReadOnlyCollection<IFile> Install(PackageReference package, PackageType type, DirectoryPath path)
        {
            return null;
        }
    }
}
