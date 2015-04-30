using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;

// ReSharper disable All

namespace SpriteEntity
{
    public class BeamScript : AsyncScript
    {
        /// <summary>
        /// Direction of the beam along the X axis
        /// </summary>
        public float DirectionX { get; set; }

        /// <summary>
        /// Tells whether this bullet is alive.
        /// </summary>
        public bool IsAlive { get; set; }

        private const float beamSpeed = 14f;
        private const float maxWidthX = 8f + 2f;

        // TODO centralize
        private const float minWidthX = -8f - 2f;

        private SpriteGroup beamNormalSprite;

        public BeamScript()
        {
            DirectionX = 1f;
            IsAlive = true;
        }

        public override async Task Execute()
        {
            while (Game.IsRunning)
            {
                await Script.NextFrame();

                if (!IsAlive) continue;

                // Move
                Entity.Transform.Position.X += DirectionX * beamSpeed * (float)Game.UpdateTime.Elapsed.TotalSeconds;

                // Entity went out the screen, mark it as dead
                if ((Entity.Transform.Position.X <= minWidthX) || (Entity.Transform.Position.X >= maxWidthX))
                {
                    IsAlive = false;
                }
            }
        }

        public RectangleF GetBoundingBox()
        {
            if (beamNormalSprite == null) beamNormalSprite = Asset.Load<SpriteGroup>("bullet");
            var result = beamNormalSprite.Images.First().Region;
            result.Width *= LogicScript.ScreenScale;
            result.Height *= LogicScript.ScreenScale;
            return result;
        }

    }
}
