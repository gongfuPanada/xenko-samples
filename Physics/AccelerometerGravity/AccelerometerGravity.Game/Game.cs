using SiliconStudio.Paradox.Physics;

namespace AccelerometerGravity
{
    public class Game : SiliconStudio.Paradox.Engine.Game
    {
        protected override void Initialize()
        {
            base.Initialize();

            //physics needs explicit initialization
            GameSystems.Add(new Bullet2PhysicsSystem(Services));
        }
    }
}