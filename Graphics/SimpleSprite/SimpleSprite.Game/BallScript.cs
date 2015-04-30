using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Rendering.Composers;
using SiliconStudio.Paradox.Graphics;

namespace SimpleSprite
{
    public class BallScript : AsyncScript
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


        public override async Task Execute()
        {
            // create the ball sprite.
            var virtualResolution = new Vector3(GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height, 1);
            spriteBatch = new SpriteBatch(GraphicsDevice) { VirtualResolution = virtualResolution };
            sphere = await Asset.LoadAsync<Texture>("sphere");

            // Initialize ball's state related variables.
            resolution = new Vector2(virtualResolution.X, virtualResolution.Y);
            ballHalfSize = new Vector2(SphereWidth / 2, SphereHeight / 2);
            ballPosition = resolution / 2;
            ballSpeed = new Vector2(600, -400);

            // Add Graphics Layer
            var scene = SceneSystem.SceneInstance.Scene;
            var compositor = ((SceneGraphicsCompositorLayers)scene.Settings.GraphicsCompositor);
            compositor.Master.Renderers.Add(new SceneDelegateRenderer(RenderSpheres));

            while (Game.IsRunning)
            {
                await Script.NextFrame();
            
                ballPosition += ballSpeed * (float)Game.UpdateTime.Elapsed.TotalSeconds;

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

        private void RenderSpheres(RenderContext renderContext, RenderFrame frame)
        {
            spriteBatch.Begin();

            // draw the ball
            var time = (float)Game.DrawTime.Total.TotalSeconds;
            var rotation = time * (float)Math.PI * 0.5f;
            var sourceRectangle = GetSphereAnimation(1.25f * time);
            spriteBatch.Draw(sphere, ballPosition, sourceRectangle, Color.White, rotation, ballHalfSize);

            spriteBatch.End();
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
            return new Rectangle(sphereX * (SphereWidth + SphereSpace), sphereY * (SphereHeight + SphereSpace), SphereWidth, SphereHeight);
        }
    }
}
