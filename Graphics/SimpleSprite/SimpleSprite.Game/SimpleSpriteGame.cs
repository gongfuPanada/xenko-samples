using System;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;

namespace SimpleSprite
{
    public class SimpleSpriteGame : Game
    {
        private const int SphereSpace = 4;
        private const int SphereWidth = 150;
        private const int SphereHeight = 150;
        private const int SphereCountPerRow = 6;
        private const int SphereTotalCount = 32;

        private SpriteBatch spriteBatch;
        private Texture sphere;

        private Vector2 resolution;
        private Vector2 ballHalfSize;
        private Vector2 ballPosition;
        private Vector2 ballSpeed;

        public SimpleSpriteGame()
        {
            // Target 9.1 profile by default
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1 };
        }

        protected override async Task LoadContent()
        {
            // create the ball sprite.
            spriteBatch = new SpriteBatch(GraphicsDevice) { VirtualResolution = VirtualResolution };
            sphere = await Asset.LoadAsync<Texture>("sphere");

            // Initialize ball's state related variables.
            resolution = new Vector2(VirtualResolution.X, VirtualResolution.Y);
            ballHalfSize = new Vector2(SphereWidth / 2, SphereHeight / 2);
            ballPosition = resolution / 2;
            ballSpeed = new Vector2(600, -400);

            // create the rendering pipeline
            CreatePipeline();

            // Add a custom script
            Script.Add(UpdateSpheres);
        }

        private void RenderSpheres(RenderContext renderContext)
        {
            spriteBatch.Begin();

            // draw the ball
            var time = (float)DrawTime.Total.TotalSeconds;
            var rotation = time * (float)Math.PI * 0.5f;
            var sourceRectangle = GetSphereAnimation(1.25f * time);
            spriteBatch.Draw(sphere, ballPosition, sourceRectangle, Color.White, rotation, ballHalfSize, 1.0f);

            spriteBatch.End();            
        }

        private void CreatePipeline()
        {
            // Setup the default rendering pipeline
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services));
            RenderSystem.Pipeline.Renderers.Add(new BackgroundRenderer(Services, "ParadoxBackground")); 
            RenderSystem.Pipeline.Renderers.Add(new DelegateRenderer(Services) { Render = RenderSpheres });
        }

        private async Task UpdateSpheres()
        {
            while (IsRunning)
            {
                await Script.NextFrame();

                ballPosition += ballSpeed * (float)UpdateTime.Elapsed.TotalSeconds;

                if (ballPosition.X < ballHalfSize.X)
                {
                    ballPosition.X = ballHalfSize.X + (ballHalfSize.X - ballPosition.X);
                    ballSpeed.X = -ballSpeed.X;
                }
                if (ballPosition.X > resolution.X - ballHalfSize.X)
                {
                    ballPosition.X = 2 * (resolution.X - ballHalfSize.X) - ballPosition.X;
                    ballSpeed.X = -ballSpeed.X;
                }
                if (ballPosition.Y < ballHalfSize.Y)
                {
                    ballPosition.Y = ballHalfSize.Y + (ballHalfSize.Y - ballPosition.Y);
                    ballSpeed.Y = -ballSpeed.Y;
                }
                if (ballPosition.Y > resolution.Y - ballHalfSize.Y)
                {
                    ballPosition.Y = 2 * (resolution.Y - ballHalfSize.Y) - ballPosition.Y;
                    ballSpeed.Y = -ballSpeed.Y;
                }
            }
        }

        /// <summary>
        /// Calculates the rectangle region from the original Sphere bitmap.
        /// </summary>
        /// <param name="time">The current time</param>
        /// <returns>The region from the sphere texture to display</returns>
        private static Rectangle GetSphereAnimation(float time)
        {
            var sphereIndex = MathUtil.Clamp((int)((time % 1.0f) * SphereTotalCount), 0, SphereTotalCount);

            var sphereX = sphereIndex % SphereCountPerRow;
            var sphereY = sphereIndex / SphereCountPerRow;
            return new Rectangle(sphereX * (SphereWidth+SphereSpace), sphereY * (SphereHeight+SphereSpace), SphereWidth, SphereHeight);
        }
    }
}
