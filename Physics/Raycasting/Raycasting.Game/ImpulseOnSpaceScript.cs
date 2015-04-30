using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.Engine;

namespace Raycasting
{
    /// <summary>
    /// Apply an impulse on the entity when pressing key 'Space'
    /// </summary>
    public class ImpulseOnSpaceScript : AsyncScript
    {
        public override async Task Execute()
        {
            var rigidBody = Entity.Get<PhysicsComponent>()[0].RigidBody;

            while (Game.IsRunning)
            {
                if (Input.IsKeyDown(Keys.Space))
                {
                    rigidBody.Activate();
                    rigidBody.ApplyImpulse(new Vector3(0, 1, 0));
                }

                await Script.NextFrame();
            }
        }
    }
}