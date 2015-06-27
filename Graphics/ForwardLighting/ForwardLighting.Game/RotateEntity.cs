using System;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;

namespace ForwardLighting
{
    /// <summary>
    /// Script in charge of rotating the entity
    /// </summary>
    public class RotateEntity : AsyncScript
    {
        /// <summary>
        /// A reference to the stand
        /// </summary>
        public Entity Stand;

        /// <summary>
        /// A reference to the camera
        /// </summary>
        public Entity Camera;

        public override async Task Execute()
        {
            var dragValue = 0f;
            var initialHeight = Entity.Transform.Position.Y;
            var initialRotation = Entity.Transform.Rotation;

            while (Game.IsRunning)
            {
                // Wait next rendering frame
                await Script.NextFrame();

                // rotate character
                var characterAnimationPeriod = 2 * Math.PI * (Game.UpdateTime.Total.TotalMilliseconds % 10000) / 10000;

                Entity.Transform.Rotation = initialRotation * Quaternion.RotationAxis(Vector3.UnitY, (float)characterAnimationPeriod);
                Entity.Transform.Position.Y = initialHeight + 0.1f * (float)Math.Sin(3 * characterAnimationPeriod);

                // rotate camera
                dragValue = 0.95f * dragValue;
                if (Input.PointerEvents.Count > 0)
                    dragValue = Input.PointerEvents.Sum(x => x.DeltaPosition.X);

                var cameraRotation = Quaternion.RotationY((float)(2 * Math.PI * -dragValue));
                Camera.Transform.Position = Vector3.Transform(Camera.Transform.Position, cameraRotation);
                Camera.Transform.Rotation *= cameraRotation;
            }
        }
    }
}