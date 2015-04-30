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

        public override async Task Execute()
        {
            var dragValue = 0f;
            var initialHeight = Entity.Transform.Position.Y;

            while (Game.IsRunning)
            {
                // Wait next rendering frame
                await Script.NextFrame();

                // rotate character
                Entity.Transform.Rotation *= Quaternion.RotationAxis(Vector3.UnitY, (float)(0.005 * Math.PI));

                var characterAnimationPeriod = 2 * Math.PI * (Game.UpdateTime.Total.TotalMilliseconds % 10000) / 10000;
                Entity.Transform.Position.Y = initialHeight + 0.1f * (float)Math.Sin(3 * characterAnimationPeriod);

                // rotate camera
                dragValue = 0.95f * dragValue;
                if (Input.PointerEvents.Count > 0)
                    dragValue = Input.PointerEvents.Sum(x => x.DeltaPosition.X);

                var cameraRotation = Quaternion.RotationY((float)(2 * Math.PI * dragValue));
                Stand.Transform.Rotation *= cameraRotation;
                Entity.Transform.Rotation *= cameraRotation;
            }
        }
    }
}