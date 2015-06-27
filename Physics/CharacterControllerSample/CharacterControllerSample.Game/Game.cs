using SiliconStudio.Paradox.Physics;

namespace CharacterControllerSample
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
