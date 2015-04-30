using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Physics;

namespace Raycasting
{
    public class RaycastingGame : Game
    {
        protected override void Initialize()
        {
            base.Initialize();

            //physics needs explicit initialization
            GameSystems.Add(new Bullet2PhysicsSystem(Services));
        }
    }
}