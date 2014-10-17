using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;

namespace GeometricPrimitives
{
    /// <summary>
    /// This sample shows how to create primitive objects: Plane, Cube, Sphere, GeoSphere, Cylinder, Torus, and Teapot.
    /// </summary>
    public class GeometricPrimitivesGame : Game
    {
        private enum RotatingState
        {
            Right,
            Left,
            None,
        }

        private const float InterpolateStep = 0.05f;
        private const float PerimeterScalar = 7f;
        private static readonly Vector3 PerimeterDirectionVector = new Vector3(0, 0, -1);
        private static readonly Vector3 PerimeterVector = PerimeterDirectionVector * PerimeterScalar;

        private List<GeometricPrimitive> primitives;
        private SimpleEffect simpleEffect; 
        private Matrix view;
        private Matrix projection;

        private RotatingState rotatingState = RotatingState.None;
        private float rotateFactor;
        private int lookAtIndex;

        public GeometricPrimitivesGame()
        {
            // Target 9.1 profile by default
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1 };
        }

        /// <summary>
        /// Initialize the engine, create primitive objects, and determine view and projection matrices
        /// </summary>
        /// <returns></returns>
        protected override async Task LoadContent()
        {
            await base.LoadContent();

            CreateRenderingPipeline();

            IsMouseVisible = true;

            Input.ActivatedGestures.Add(new GestureConfigDrag());

            // Creates all primitives
            primitives = new List<GeometricPrimitive>
                             {
                                 GeometricPrimitive.Plane.New(GraphicsDevice),
                                 GeometricPrimitive.Cube.New(GraphicsDevice, 0.8f),
                                 GeometricPrimitive.Sphere.New(GraphicsDevice),
                                 GeometricPrimitive.GeoSphere.New(GraphicsDevice),
                                 GeometricPrimitive.Cylinder.New(GraphicsDevice, 0.9f),
                                 GeometricPrimitive.Torus.New(GraphicsDevice),
                                 GeometricPrimitive.Teapot.New(GraphicsDevice)
                             };

            // Load the texture, and create SimpleEffect
            simpleEffect = new SimpleEffect(GraphicsDevice) {Texture = Asset.Load<Texture2D>("texture")};

            // Create the view and projection matrices
            view = Matrix.LookAtRH(PerimeterDirectionVector * 10f + new Vector3(0, 2.0f, 0.0f), new Vector3(0, -4, 0), Vector3.UnitY);
            projection = Matrix.PerspectiveFovRH((float)Math.PI / 4.0f, (float)GraphicsDevice.BackBuffer.Width / GraphicsDevice.BackBuffer.Height, 0.1f, 100.0f);

            Script.Add(UpdateObject);
        }

        /// <summary>
        /// Create rendering pipeline for Background and custom renderer which is used to render primitive objects
        /// </summary>
        private void CreateRenderingPipeline()
        {
            // Setup the default rendering pipeline
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services));
            RenderSystem.Pipeline.Renderers.Add(new BackgroundRenderer(Services, "ParadoxBackground"));
            RenderSystem.Pipeline.Renderers.Add(new DelegateRenderer(Services) { Render = Render});
        }

        /// <summary>
        /// Check swipe input for a user by detecting direction of the drag gesture, and update rotation for snapping the view to the target
        /// </summary>
        /// <returns></returns>
        private async Task UpdateObject()
        {
            var swipeStep = 1f / (primitives.Count);
            var interpolateFactor = 0f;
            
            var swipingFromPosition = 0f;
            var swipingToPosition = 0f;

            while (IsRunning)
            {
                // Wait for the nextFrame.
                await Script.NextFrame();

                // Rotating transformation is not finished yet, continue rotating and skip input processing
                if (rotatingState != RotatingState.None)
                {
                    interpolateFactor += InterpolateStep;
                    rotateFactor = MathUtil.Lerp(swipingFromPosition, swipingToPosition, interpolateFactor);

                    if(interpolateFactor < 1.0f)
                        continue;

                    lookAtIndex += (rotatingState == RotatingState.Left) ? 1 : -1;
                    interpolateFactor = 0.0f;
                    rotateFactor = swipingToPosition;
                    rotatingState = RotatingState.None;
                }

                // Poll and process swipe input from drag gesture
                foreach (var dragEvent in Input.GestureEvents.Where(gestureEvent => gestureEvent.Type == GestureType.Drag).Select(gestureEvent => ((GestureEventDrag)gestureEvent)))
                {
                    // Determine which direction of the swipe and find the current position and target position for interpolation
                    rotatingState = (dragEvent.CurrentPosition.X - dragEvent.StartPosition.X > 0) ? RotatingState.Left : RotatingState.Right;

                    if (lookAtIndex <= 0 && rotatingState == RotatingState.Right || lookAtIndex >= primitives.Count - 1 && rotatingState == RotatingState.Left)
                    {
                        rotatingState = RotatingState.None;
                        break;
                    }

                    swipingFromPosition = rotateFactor;
                    swipingToPosition = (rotatingState == RotatingState.Right) ? rotateFactor + swipeStep : rotateFactor - swipeStep;
                }
            }
        }

        /// <summary>
        /// Render primitive objects with SimpleEffect by supplying calculated transformation for animations
        /// </summary>
        /// <param name="renderContext"></param>
        private void Render(RenderContext renderContext)
        {
            // Render each primitive
            for (var i = 0; i < primitives.Count; i++)
            {
                var primitive = primitives[i];

                // Calculate a world transformation
                var time = (float)DrawTime.Total.TotalSeconds + i;
                var position = PerimeterVector + new Vector3(2.0f, 0, 0) * (rotateFactor * primitives.Count  + i);
                var world = Matrix.RotationY(time * 2.0f) * Matrix.RotationX(0.2f) * Matrix.Translation(position);

                // Disable Cull only for the plane primitive, otherwise use standard culling
                GraphicsDevice.SetRasterizerState(i == 0 ? GraphicsDevice.RasterizerStates.CullNone : GraphicsDevice.RasterizerStates.CullBack);

                // Draw the primitive using BasicEffect
                simpleEffect.Transform = Matrix.Multiply(world, Matrix.Multiply(view, projection));
                simpleEffect.Apply();
                primitive.Draw();
            }
        }
    }
}
