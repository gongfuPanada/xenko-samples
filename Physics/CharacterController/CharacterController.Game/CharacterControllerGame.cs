using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Physics;

namespace CharacterController
{
    public class CharacterControllerGame : Game
    {
        protected override void Initialize()
        {
            base.Initialize();

            //physics needs explicit initialization
            GameSystems.Add(new Bullet2PhysicsSystem(Services));
        }
    }
}
