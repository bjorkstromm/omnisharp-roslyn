using System.Collections.Generic;

namespace Cake.OmniSharp
{
    public class CakeContextModelCollection
    {
        public CakeContextModelCollection(IEnumerable<CakeContextModel> projects)
        {
            Projects = projects;
        }

        public IEnumerable<CakeContextModel> Projects { get; }
    }
}