using System.Threading.Tasks;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Physics;

namespace Constraints
{
    public class ConstraintsGame : Game
    {
        protected override void Initialize()
        {
            base.Initialize();

            //physics needs explicit initialization
            GameSystems.Add(new Bullet2PhysicsSystem(Services));
        }
    }
}
