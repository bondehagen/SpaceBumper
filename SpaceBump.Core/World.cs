using System.Collections.Generic;

namespace SpaceBumper
{
    public class World
    {
        private IList<Bumpership> bumperships;
        private readonly Map map;

        public World(Map map)
        {
            this.map = map;
        }

        public Map Map
        {
            get { return map; }
        }

        public IList<Bumpership> Bumperships
        {
            get { return bumperships; }
        }

        public bool Update(int iterations)
        {
            foreach (Bumpership bumpership in bumperships)
                bumpership.Update(iterations);
            
            return true;
        }

        public void AddShips(IList<Bumpership> bumperships)
        {
            this.bumperships = bumperships;
        }
    }
}