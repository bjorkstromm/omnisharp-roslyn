using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cake.Core.IO;
using Cake.Core.Packaging;

namespace OmniSharp.Cake.Internal
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
