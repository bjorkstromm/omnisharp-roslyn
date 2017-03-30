using System.Collections.Generic;

namespace OmniSharp.Cake
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