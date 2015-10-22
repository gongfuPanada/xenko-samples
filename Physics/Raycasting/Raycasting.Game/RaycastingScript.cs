using System.Linq;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Input;
using SiliconStudio.Xenko.Physics;

namespace Raycasting
{
    public class RaycastingScript : SyncScript
    {
        private Simulation simulation;
        private CameraComponent camera;

        public override void Start()
        {
            camera = Entity.Get<CameraComponent>();
            simulation = Entity.Get<PhysicsComponent>().Simulation;
        }

        public override void Update()
        {
            foreach (var pointerEvent in Input.PointerEvents.Where(x => x.State == PointerState.Down))
            {
                Raycast(pointerEvent.Position);
            }
        }

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
    }
}