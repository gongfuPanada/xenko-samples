using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Composers;
using SiliconStudio.Paradox.Graphics;

namespace JumpyJet
{
    /// <summary>
    /// The script in charge of creating and updating the background.
    /// </summary>
    public class BackgroundScript : AsyncScript
    {
        // Entities' depth
        private const int Pal0Depth = 0;
        private const int Pal1Depth = 1;
        private const int Pal2Depth = 2;
        private const int Pal3Depth = 3;

        private SpriteBatch spriteBatch;

        private readonly List<BackgroundSection> backgroundParallax = new List<BackgroundSection>();

        public override void Start()
        {
            base.Start();

            var virtualResolution = new Vector3(GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height, 20f);

            // Create Parallax Background
            var pal0SpriteGroup = Asset.Load<SpriteGroup>("pal0_sprite");
            var pal1SpriteGroup = Asset.Load<SpriteGroup>("pal1_sprite");
            var pal2SpriteGroup = Asset.Load<SpriteGroup>("pal2_sprite");
            backgroundParallax.Add(new BackgroundSection(pal0SpriteGroup.Images[0], virtualResolution, GameScript.GameSpeed / 4f, Pal0Depth));
            backgroundParallax.Add(new BackgroundSection(pal1SpriteGroup.Images[0], virtualResolution, GameScript.GameSpeed / 3f, Pal1Depth));
            backgroundParallax.Add(new BackgroundSection(pal2SpriteGroup.Images[0], virtualResolution, GameScript.GameSpeed / 1.5f, Pal2Depth));

            // For pal3Sprite: Ground, move it downward so that its bottom edge is at the bottom screen.
            var screenHeight = virtualResolution.Y;
            var pal3SpriteGroup = Asset.Load<SpriteGroup>("pal3_sprite");
            var pal3Height = pal3SpriteGroup.Images[0].Region.Height;
            backgroundParallax.Add(new BackgroundSection(pal3SpriteGroup.Images[0], virtualResolution, GameScript.GameSpeed, Pal3Depth, Vector2.UnitY * (screenHeight - pal3Height) / 2));
            
            // allocate the sprite batch in charge of drawing the backgrounds.
            spriteBatch = new SpriteBatch(GraphicsDevice) { VirtualResolution = virtualResolution };

            // register the renderer in the pipeline
            var scene = SceneSystem.SceneInstance.Scene;
            var compositor = ((SceneGraphicsCompositorLayers)scene.Settings.GraphicsCompositor);
            compositor.Master.Renderers.Insert(1, new SceneDelegateRenderer(DrawParallax));
        }

        public override async Task Execute()
        {
            while (Game.IsRunning)
            {
                var elapsedTime = (float) Game.UpdateTime.Elapsed.TotalSeconds;

                // Update Parallax backgrounds
                foreach (var parallax in backgroundParallax)
                    parallax.Update(elapsedTime);

                await Script.NextFrame();
            }
        }


        public void DrawParallax(RenderContext context, RenderFrame frame)
        {
            spriteBatch.Begin();

            foreach (var pallaraxBackground in backgroundParallax)
                pallaraxBackground.DrawSprite(spriteBatch);

            spriteBatch.End();
        }

        public void StartScrolling()
        {
            EnableAllParallaxesUpdate(true);
        }

        public void StopScrolling()
        {
            EnableAllParallaxesUpdate(false);
        }

        private void EnableAllParallaxesUpdate(bool isEnable)
        {
            foreach (var pallarax in backgroundParallax)
            {
                pallarax.IsUpdating = isEnable;
            }
        }
    }
}