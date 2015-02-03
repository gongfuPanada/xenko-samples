using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Physics;
using SiliconStudio.Paradox.UI;
using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Panels;

namespace Constraints
{
    public class ConstraintsGame : Game
    {
        private IPhysicsSystem physicsSystem;

        public ConstraintsGame()
        {
            // Target 9.1 profile by default
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1 };
            GraphicsDeviceManager.PreferredBackBufferHeight = 720;
            GraphicsDeviceManager.PreferredBackBufferWidth = 1280;
        }

        private Constraint currentConstraint;
        private readonly List<Action> constraintsList = new List<Action>();
        private int constraintIndex;

        private RigidBody cubeRigidBody;
        private RigidBody sphereRigidBody;

        private TextBlock constraintNameBlock;

        void CreatePoint2PointConstraint()
        {
            cubeRigidBody.LinearFactor = Vector3.Zero;
            cubeRigidBody.AngularFactor = Vector3.Zero;
            sphereRigidBody.LinearFactor = new Vector3(1, 1, 1);
            sphereRigidBody.AngularFactor = new Vector3(1, 1, 1);

            currentConstraint = physicsSystem.PhysicsEngine.CreateConstraint(ConstraintTypes.Point2Point, cubeRigidBody, sphereRigidBody, Matrix.Identity, Matrix.Translation(new Vector3(20, 0, 0)));
            physicsSystem.PhysicsEngine.AddConstraint(currentConstraint);
            constraintNameBlock.Text = "Point to Point";

            //there are no limits so the sphere will orbit once we apply this
            sphereRigidBody.ApplyImpulse(new Vector3(0, 0, 250));
        }

        void CreateHingeConstraint()
        {
            cubeRigidBody.LinearFactor = Vector3.Zero;
            cubeRigidBody.AngularFactor = Vector3.Zero;
            sphereRigidBody.LinearFactor = new Vector3(1, 1, 1);
            sphereRigidBody.AngularFactor = new Vector3(1, 1, 1);

            currentConstraint = physicsSystem.PhysicsEngine.CreateConstraint(ConstraintTypes.Hinge, cubeRigidBody, sphereRigidBody, Matrix.Identity, Matrix.Translation(new Vector3(20, 0, 0)));
            physicsSystem.PhysicsEngine.AddConstraint(currentConstraint);
            constraintNameBlock.Text = "Hinge";

            //applying this impulse will show the hinge limits stopping it
            sphereRigidBody.ApplyImpulse(new Vector3(0, 0, 250));
        }

        void CreateGearConstraint()
        {
            cubeRigidBody.LinearFactor = Vector3.Zero;
            cubeRigidBody.AngularFactor = new Vector3(1,1,1);
            sphereRigidBody.LinearFactor = Vector3.Zero;
            sphereRigidBody.AngularFactor = new Vector3(1, 1, 1);

            currentConstraint = physicsSystem.PhysicsEngine.CreateConstraint(ConstraintTypes.Gear, sphereRigidBody, cubeRigidBody, Matrix.Translation(new Vector3(1, 0, 0)), Matrix.Translation(new Vector3(1, 0, 0)));
            physicsSystem.PhysicsEngine.AddConstraint(currentConstraint);
            constraintNameBlock.Text = "Gear";

            //this force will start a motion in the sphere which gets propagated into the cube
            sphereRigidBody.ApplyTorque(new Vector3(500,0,0));
        }

        void CreateSliderConstraint()
        {
            cubeRigidBody.LinearFactor = Vector3.Zero;
            cubeRigidBody.AngularFactor = Vector3.Zero;
            sphereRigidBody.LinearFactor = new Vector3(1, 1, 1);
            sphereRigidBody.AngularFactor = new Vector3(1, 1, 1);

            currentConstraint = physicsSystem.PhysicsEngine.CreateConstraint(ConstraintTypes.Slider, cubeRigidBody, sphereRigidBody, Matrix.Identity, Matrix.Identity, true);
            physicsSystem.PhysicsEngine.AddConstraint(currentConstraint);
            constraintNameBlock.Text = "Slider";

            var slider = (SliderConstraint)currentConstraint;
            slider.LowerLinearLimit = -20;
            slider.UpperLinearLimit = 0;
            //avoid strange movements
            slider.LowerAngularLimit = (float)-Math.PI/3.0f;
            slider.UpperAngularLimit = (float)Math.PI/3.0f;
            //make the return after hitting the limit fast!
            //slider.SoftnessLimLinear = 20;

            //set an event that on collision will kick the impulse again
            cubeRigidBody.OnFirstContactBegin += (sender, args) =>
            {
                if (currentConstraint as SliderConstraint == null) return;
                if (args.Contact.ColliderA != sphereRigidBody && args.Contact.ColliderB != sphereRigidBody) return;
                sphereRigidBody.LinearVelocity = Vector3.Zero; //clear any existing velocity
                sphereRigidBody.ApplyImpulse(new Vector3(-50, 0, 0)); //fire impulse
            };

            //applying this impulse will let the sphere reach the lower linear limit and afterwards will be dragged back towards the cube
            sphereRigidBody.ApplyImpulse(new Vector3(-50, 0, 0));
        }

        void CreateConeTwistConstraint()
        {
            cubeRigidBody.LinearFactor = Vector3.Zero;
            cubeRigidBody.AngularFactor = Vector3.Zero;
            sphereRigidBody.LinearFactor = new Vector3(1, 1, 1);
            sphereRigidBody.AngularFactor = new Vector3(1, 1, 1);

            currentConstraint = physicsSystem.PhysicsEngine.CreateConstraint(ConstraintTypes.ConeTwist, cubeRigidBody, sphereRigidBody, Matrix.Identity, Matrix.Translation(new Vector3(20, 0, 0)));
            physicsSystem.PhysicsEngine.AddConstraint(currentConstraint);
            constraintNameBlock.Text = "Cone Twist";

            var coneTwist = (ConeTwistConstraint)currentConstraint;
            coneTwist.SetLimit(0.5f, 0.5f, 0.5f);

            //applying this impulse will show the cone limits
            sphereRigidBody.ApplyImpulse(new Vector3(0, 0, 250));
        }

        void CreateGeneric6DoFConstraint()
        {
            cubeRigidBody.LinearFactor = Vector3.Zero;
            cubeRigidBody.AngularFactor = Vector3.Zero;
            sphereRigidBody.LinearFactor = new Vector3(1, 1, 1);
            sphereRigidBody.AngularFactor = new Vector3(1, 1, 1);

            currentConstraint = physicsSystem.PhysicsEngine.CreateConstraint(ConstraintTypes.Generic6DoF, cubeRigidBody, sphereRigidBody, Matrix.Identity, Matrix.Translation(new Vector3(20, 0, 0)));
            physicsSystem.PhysicsEngine.AddConstraint(currentConstraint);
            constraintNameBlock.Text = "Generic 6D of Freedom";

            sphereRigidBody.ApplyImpulse(new Vector3(0, 0, 250));
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            CreatePipeline();

            //physics is a plug-in now, needs explicit initialization
            physicsSystem = new Bullet2PhysicsSystem(this);
            physicsSystem.PhysicsEngine.Initialize();

            // Load and initialize entities
            var ground = Asset.Load<Entity>("ground");
            var cube = Asset.Load<Entity>("cube_1");
            var sphere = Asset.Load<Entity>("sphere_1");

            cube.Transformation.Translation = new Vector3(10, 5, 0);
            sphere.Transformation.Translation = new Vector3(-10, 5, 0);

            Entities.Add(ground);
            Entities.Add(cube);
            Entities.Add(sphere);

            cubeRigidBody = cube.GetOrCreate<PhysicsComponent>()[0].RigidBody;
            cubeRigidBody.CanSleep = false;
            sphereRigidBody = sphere.GetOrCreate<PhysicsComponent>()[0].RigidBody;
            sphereRigidBody.CanSleep = false;
            
            // Create the UI
            var font = Asset.Load<SpriteFont>("Font");
            constraintNameBlock = new TextBlock
            {
                Font = font,
                TextSize = 60,
                TextColor = Color.White,
            };
            constraintNameBlock.SetCanvasPinOrigin(new Vector3(0.5f, 0.5f, 0));
            constraintNameBlock.SetCanvasRelativePosition(new Vector3(0.5f, 0.93f, 0));

            UI.RootElement = new Canvas { Children = {constraintNameBlock, CreateButton("Next Constraint", font, 1), CreateButton("Last Constraint", font, -1) } };

            // Create and initialize constraint
            constraintsList.Add(CreatePoint2PointConstraint);
            constraintsList.Add(CreateHingeConstraint);
            constraintsList.Add(CreateGearConstraint);
            constraintsList.Add(CreateSliderConstraint);
            constraintsList.Add(CreateConeTwistConstraint);
            constraintsList.Add(CreateGeneric6DoFConstraint);

            constraintsList[constraintIndex]();
        }

        private Button CreateButton(string text, SpriteFont font, int offset)
        {
            var button = new Button
            {
                Name = text,
                Padding = Thickness.UniformRectangle(15),
                HorizontalAlignment = HorizontalAlignment.Left,
                Content = new TextBlock { Text = text, Font = font, TextSize = 35, TextColor = new Color(200, 200, 200, 255)},
            };
            button.Click += (sender, args) => ChangeConstraint(offset);
            button.SetCanvasPinOrigin(new Vector3(offset>0? 1: 0, 0.5f, 0));
            button.SetCanvasRelativePosition(new Vector3(offset>0? 0.97f: 0.03f, 0.93f, 0));

            return button;
        }

        private void ChangeConstraint(int offset)
        {
            //Remove and dispose the current constraint
            physicsSystem.PhysicsEngine.RemoveConstraint(currentConstraint);
            currentConstraint.Dispose();

            //Stop motion and reset the rigid bodies
            cubeRigidBody.PhysicsWorldTransform = Matrix.Translation(new Vector3(10, 5, 0))*
                                                  Matrix.RotationQuaternion(new Quaternion(0, 0, 0, 1));

            cubeRigidBody.AngularVelocity = Vector3.Zero;
            cubeRigidBody.LinearVelocity = Vector3.Zero;

            sphereRigidBody.PhysicsWorldTransform = Matrix.Translation(new Vector3(-10, 5, 0))*
                                                    Matrix.RotationQuaternion(new Quaternion(0, 0, 0, 1));

            sphereRigidBody.AngularVelocity = Vector3.Zero;
            sphereRigidBody.LinearVelocity = Vector3.Zero;

            // calculate constraint index
            constraintIndex = (constraintIndex + offset + constraintsList.Count) % constraintsList.Count;

            constraintsList[constraintIndex]();
        }

        private void CreatePipeline()
        {
            // Setup the default rendering pipeline
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services));
            RenderSystem.Pipeline.Renderers.Add(new BackgroundRenderer(Services, "ParadoxBackground"));
            RenderSystem.Pipeline.Renderers.Add(new ModelRenderer(Services, "ConstraintsEffectMain"));
            RenderSystem.Pipeline.Renderers.Add(new UIRenderer(Services));

            //set view
            RenderSystem.Pipeline.Parameters.Set(TransformationKeys.View, Matrix.LookAtRH(new Vector3(0, 0, 50), new Vector3(0, 0, 0), Vector3.UnitY));
            RenderSystem.Pipeline.Parameters.Set(TransformationKeys.Projection, Matrix.PerspectiveFovRH((float)Math.PI / 4.0f, (float)GraphicsDevice.BackBuffer.Width / GraphicsDevice.BackBuffer.Height, 0.1f, 10000.0f));
        }
    }
}
