using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Effects.Modules;
using System;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.Physics;
using SiliconStudio.Paradox.UI;
using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Panels;

namespace Raycasting
{
    public class RaycastingGame : Game
    {
        public RaycastingGame()
        {
            // Target 9.1 profile by default
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1 };
            GraphicsDeviceManager.PreferredBackBufferHeight = 720;
            GraphicsDeviceManager.PreferredBackBufferWidth = 1280;
        }

        private Entity mainCube;

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            CreatePipeline();

            // Initialize physics
            Physics.PhysicsEngine.Initialize(PhysicsEngineFlags.None);

            // Let mouse show
            IsMouseVisible = true;

            // UI
            var textBlock = new TextBlock { Text = "Shoot the cubes!", Font = Asset.Load<SpriteFont>("Font"), TextColor = Color.White, TextSize = 60 };
            textBlock.SetCanvasPinOrigin(new Vector3(0.5f, 0.5f, 0));
            textBlock.SetCanvasRelativePosition(new Vector3(0.5f, 0.9f, 0f));
            UI.RootElement = new Canvas { Children = {textBlock} };

            // load ground
            var ground = Asset.Load<Entity>("ground");
            Entities.Add(ground);

            // create some cubes
            mainCube = Asset.Load<Entity>("cube");

            for (var i = 0; i < 8; i++)
            {
                var cube = mainCube.Clone();
                cube.Transformation.Translation = new Vector3(-30 + (6 * i), 0, -26 + (7 * i));
                Entities.Add(cube);
                // set cubes to never sleep!
                cube.GetOrCreate<PhysicsComponent>()[0].RigidBody.CanSleep = false;
            }

            // Add a custom script
            Script.Add(GameScript1);
        }

        private void CreatePipeline()
        {
            // Setup the default rendering pipeline
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services));
            RenderSystem.Pipeline.Renderers.Add(new BackgroundRenderer(Services, "ParadoxBackground"));
            RenderSystem.Pipeline.Renderers.Add(new ModelRenderer(Services, "RaycastingEffectMain"));
            RenderSystem.Pipeline.Renderers.Add(new UIRenderer(Services));

            // set view
            RenderSystem.Pipeline.Parameters.Set(TransformationKeys.View, Matrix.LookAtRH(new Vector3(0, 0, 50), new Vector3(0, -8, 0), Vector3.UnitY));
            RenderSystem.Pipeline.Parameters.Set(TransformationKeys.Projection, Matrix.PerspectiveFovRH(0.7f, (float)GraphicsDevice.BackBuffer.Width / GraphicsDevice.BackBuffer.Height, 0.1f, 10000.0f));
        }

        void Raycast(Vector2 screenPos)
        {
            screenPos.X *= GraphicsDevice.BackBuffer.Width;
            screenPos.Y *= GraphicsDevice.BackBuffer.Height;

            var unprojectedNear =
                GraphicsDevice.Viewport.Unproject(
                    new Vector3(screenPos, 0.0f),
                    RenderSystem.Pipeline.Parameters.Get(TransformationKeys.Projection),
                    RenderSystem.Pipeline.Parameters.Get(TransformationKeys.View), Matrix.Identity);

            var unprojectedFar =
                GraphicsDevice.Viewport.Unproject(
                    new Vector3(screenPos, 1.0f),
                    RenderSystem.Pipeline.Parameters.Get(TransformationKeys.Projection),
                    RenderSystem.Pipeline.Parameters.Get(TransformationKeys.View), Matrix.Identity);

            var result = Physics.PhysicsEngine.Raycast(unprojectedNear, unprojectedFar);
            if (!result.Succeeded || result.Collider == null) return;

            var rigidBody = result.Collider as RigidBody;
            if (rigidBody != null) rigidBody.ApplyImpulse(new Vector3(0, 15, 0));
        }

        private async Task GameScript1()
        {
            while (IsRunning)
            {
                // Wait next rendering frame
                await Script.NextFrame();

                // on touches on the screen
                foreach (var pointerEvent in Input.PointerEvents.Where(x=>x.State == PointerState.Down))
                    Raycast(pointerEvent.Position);
            }
        }
    }
}
