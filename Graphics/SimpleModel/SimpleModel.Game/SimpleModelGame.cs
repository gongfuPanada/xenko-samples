using System;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;

namespace SimpleModel
{
    public class SimpleModelGame : Game
    {
        private Entity entity;

        public SimpleModelGame()
        {
            // Target 9.1 profile by default
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1 };
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            // create the rendering pipeline of the game
            CreatePipeline();

            // load the entity to display
            entity = Asset.Load<Entity>("SimpleEntity");

            // create and set the camera
            var target = new Entity(new Vector3(0, 75, 0));
            var camera = new Entity(new Vector3(0, 150, 450))
            {
                new CameraComponent(target, 1, 5000) {VerticalFieldOfView = 1f, TargetUp = Vector3.UnitY}
            };
            RenderSystem.Pipeline.SetCamera(camera.Get<CameraComponent>());

            // add the entity and the camera to the scene
            Entities.Add(entity);
            Entities.Add(target);
            Entities.Add(camera);

            Script.Add(RotateModel);
        }

        private async Task RotateModel()
        {
            while (IsRunning)
            {
                await Script.NextFrame();

                var time = (float)UpdateTime.Total.TotalSeconds;

                entity.Transformation.Rotation = Quaternion.RotationY((0.4f * time % 2f)*(float) Math.PI);
            }
        }

        private void CreatePipeline()
        {
            // Setup the default rendering pipeline
            RenderSystem.Pipeline.Renderers.Add(new CameraSetter(Services));
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services));
            RenderSystem.Pipeline.Renderers.Add(new BackgroundRenderer(Services, "ParadoxBackground"));
            RenderSystem.Pipeline.Renderers.Add(new ModelRenderer(Services, "SimpleModelEffectMain"));
        }
    }
}
