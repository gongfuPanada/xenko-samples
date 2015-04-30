using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Physics;

namespace AccelerometerGravity
{
    public class AccelerometerGravityGame : Game
    {
        protected override void Initialize()
        {
            base.Initialize();

            //physics needs explicit initialization
            GameSystems.Add(new Bullet2PhysicsSystem(Services));
        }
    }
}