using System.Collections.Generic;
using SiliconStudio.Paradox.Engine;

namespace SpaceEscape.Background
{
    public class BackgroundInfo : Script
    {
        public int MaxNbObstacles { get; set; }
        public List<Hole> Holes { get; private set; } = new List<Hole>();
    }
}
