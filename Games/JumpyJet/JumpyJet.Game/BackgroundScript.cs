using System.Collections.Generic;
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
    public class BackgroundScript : SyncScript
    {
        // Entities' depth
        private const int Pal0Depth = 0;
        private const int Pal1Depth = 1;
        private const int Pal2Depth = 2;
        private const int Pal3Depth = 3;

        private SpriteBatch spriteBatch;

        private readonly List<BackgroundSection> backgroundParallax = new List<BackgroundSection>();
        private SpriteSheet pal0SpriteSheet;
        private SpriteSheet pal1SpriteSheet;
        private SpriteSheet pal2SpriteSheet;
        private SpriteSheet pal3SpriteSheet;

        private SceneDelegateRenderer delegateRenderer;

        public override void Start()
        {
            var virtualResolution = new Vector3(GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height, 20f);

            // Create Parallax Background
            pal0SpriteSheet = Asset.Load<SpriteSheet>("pal0_sprite");
            pal1SpriteSheet = Asset.Load<SpriteSheet>("pal1_sprite");
            pal2SpriteSheet = Asset.Load<SpriteSheet>("pal2_sprite");
            backgroundParallax.Add(new BackgroundSection(pal0SpriteSheet.Sprites[0], virtualResolution, GameScript.GameSpeed / 4f, Pal0Depth));
            backgroundParallax.Add(new BackgroundSection(pal1SpriteSheet.Sprites[0], virtualResolution, GameScript.GameSpeed / 3f, Pal1Depth));
            backgroundParallax.Add(new BackgroundSection(pal2SpriteSheet.Sprites[0], virtualResolution, GameScript.GameSpeed / 1.5f, Pal2Depth));

            // For pal3Sprite: Ground, move it downward so that its bottom edge is at the bottom screen.
            var screenHeight = virtualResolution.Y;
            pal3SpriteSheet = Asset.Load<SpriteSheet>("pal3_sprite");
            var pal3Height = pal3SpriteSheet.Sprites[0].Region.Height;
            backgroundParallax.Add(new BackgroundSection(pal3SpriteSheet.Sprites[0], virtualResolution, GameScript.GameSpeed, Pal3Depth, Vector2.UnitY * (screenHeight - pal3Height) / 2));
            
            // allocate the sprite batch in charge of drawing the backgrounds.
            spriteBatch = new SpriteBatch(GraphicsDevice) { VirtualResolution = virtualResolution };

            // register the renderer in the pipeline
            var scene = SceneSystem.SceneInstance.Scene;
            var compositor = ((SceneGraphicsCompositorLayers)scene.Settings.GraphicsCompositor);
            compositor.Master.Renderers.Insert(1, delegateRenderer = new SceneDelegateRenderer(DrawParallax));
        }

        public override void Update()
        {
            var elapsedTime = (float)Game.UpdateTime.Elapsed.TotalSeconds;

            // Update Parallax backgrounds
            foreach (var parallax in backgroundParallax)
                parallax.Update(elapsedTime);
        }

        public override void Cancel()
        {
            // remove the delegate renderer from the pipeline
            var scene = SceneSystem.SceneInstance.Scene;
            var compositor = ((SceneGraphicsCompositorLayers)scene.Settings.GraphicsCompositor);
            compositor.Master.Renderers.Remove(delegateRenderer);

            // free graphic objects
            spriteBatch.Dispose();
            Asset.Unload(pal0SpriteSheet);
            Asset.Unload(pal1SpriteSheet);
            Asset.Unload(pal2SpriteSheet);
            Asset.Unload(pal3SpriteSheet);
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