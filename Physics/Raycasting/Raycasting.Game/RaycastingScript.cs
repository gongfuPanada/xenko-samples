using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.Physics;

namespace Raycasting
{
    public class RaycastingScript : AsyncScript
    {
        private Simulation simulation;
        private CameraComponent camera;

        private void Raycast(Vector2 screenPos)
        {
            screenPos.X *= GraphicsDevice.BackBuffer.Width;
            screenPos.Y *= GraphicsDevice.BackBuffer.Height;

            var unprojectedNear =
                GraphicsDevice.Viewport.Unproject(
                    new Vector3(screenPos, 0.0f),
                    camera.ProjectionMatrix,
                    camera.ViewMatrix,
                    Matrix.Identity);

            var unprojectedFar =
                GraphicsDevice.Viewport.Unproject(
                    new Vector3(screenPos, 1.0f),
                    camera.ProjectionMatrix,
                    camera.ViewMatrix,
                    Matrix.Identity);

            var result = simulation.Raycast(unprojectedNear, unprojectedFar);
            if (!result.Succeeded || result.Collider == null) return;

            var rigidBody = result.Collider as RigidBody;
            if (rigidBody == null) return;

            rigidBody.Activate();
            rigidBody.ApplyImpulse(new Vector3(0, 5, 0));
        }

        public override async Task Execute()
        {
            simulation = Entity.Get<PhysicsComponent>().Simulation;
            simulation.Gravity *= 1.0f; //1 unit = 1 m so we need to multiply default gravity.

            camera = Entity.Get<CameraComponent>();

            while (Game.IsRunning)
            {
                foreach (var pointerEvent in Input.PointerEvents.Where(x => x.State == PointerState.Down))
                {
                    Raycast(pointerEvent.Position);
                }

                await Script.NextFrame();
            }
        }
    }
}