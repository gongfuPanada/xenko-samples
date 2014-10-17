using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Input;

namespace SpaceEscape
{
    /// <summary>
    /// Agent is the main character that is controllable by a user.
    /// It could change lane to left and right, and slide.
    /// </summary>
    public class Agent : ScriptContext, IScript
    {
        public enum AgentState
        {
            Run,
            ChangeLaneLeft,
            ChangeLaneRight,
            Slide,
            Die,
        }

        public enum InputState
        {
            None,
            Left,
            Right,
            Down,
        }

        public enum AgentAnimationKeys
        {
            Active,
            DodgeLeft,
            DodgeRight,
            Slide,
            Crash,
        }

        public enum BoundingBoxKeys
        {
            Normal,
            Slide,
        }

        private const int LeftLane = 0;
        private const int MiddleLane = 1;
        private const int RightLane = 2;
        private const float FloatingHeight = 150f;
        private const float ShadowHeight = 30f;
        private const float LaneLength = 500f;

        // An entity of a root of transformation
        public Entity RootEntity { get; private set; }
        // ModelEntity is a child of the RootEntity, and holds the actual Model
        public Entity ModelEntity { get; private set; }
        public Vector3 Position;

        public bool IsRunning { get; set; }
        public int CurLane { get; private set; }
        public BoundingBox ActiveBoundingBox { get; set; }
        public AgentState State
        {
            get { return state; }
            private set
            {
                state = value;
                OnEnter(state);
            }
        }

        public bool InputEnabled { get; set; }

        private AgentState state; // Current state of the agent
        private PlayingAnimation playingAnimation; // Current active animation
        private float startChangeLanePosX; // Position of X before changing lane
        private float targetChangeLanePosX; // Position of X after changning lane
        private Entity shadowEntity; // Cache for disable and enable
        private readonly Dictionary<BoundingBoxKeys, BoundingBox> boundingBoxes = new Dictionary<BoundingBoxKeys, BoundingBox>();
        private bool enableShadow;
        private readonly float screenRatio;

        private bool EnableShadow
        {
            set
            {
                if (enableShadow == value)
                    return;

                enableShadow = value;

                if (enableShadow)
                    RootEntity.Transformation.Children.Add(shadowEntity.Transformation);
                else
                {
                    RootEntity.Transformation.Children.Remove(shadowEntity.Transformation);
                    Entities.Remove(shadowEntity);
                }
            }
        }

        public Agent(IServiceRegistry registry) 
            : base(registry)
        {
            screenRatio = (float)GraphicsDevice.BackBuffer.Height/GraphicsDevice.BackBuffer.Width;
        }

        public bool IsDead { get { return State == AgentState.Die; } }

        /// <summary>
        /// Load model resource, and initialise state of the object. 
        /// </summary>
        /// <returns></returns>
        public void LoadContent()
        {
            // Load character and shadow entities with AssetManager
            shadowEntity = Asset.Load<Entity>("shadow00");
            shadowEntity.Transformation.Translation.Y += ShadowHeight;
            ModelEntity = Asset.Load<Entity>("character_00");
            ModelEntity.Transformation.Translation.Y = FloatingHeight;

            // Assign model and shadow as a children of RootEntity.
            RootEntity = new Entity();
            RootEntity.Transformation.Children.Add(ModelEntity.Transformation);

            // Config Gesture for controlling the agent
            Input.ActivatedGestures.Add(
                new GestureConfigDrag(GestureShape.Free)
                {
                    MinimumDragDistance = 0.02f,
                    RequiredNumberOfFingers = 1
                });

            // Setup Normal pose BoundingBox with that of obtained by ModelComponent.
            var model = ModelEntity.Get<ModelComponent>().Model;
            boundingBoxes[BoundingBoxKeys.Normal] = model.BoundingBox;

            // Create a slide pose BoundingBox by substracting it with a threshold for making the box, smaller in Y axis.
            var modelMinBB = model.BoundingBox.Minimum;
            var modelMaxBB = model.BoundingBox.Maximum;
            boundingBoxes[BoundingBoxKeys.Slide] = new BoundingBox(modelMinBB, new Vector3(modelMaxBB.X, modelMaxBB.Y - 70, modelMaxBB.Z));
        }

        /// <summary>
        /// Script Function which awaits each frame and update the Agent.
        /// </summary>
        /// <returns></returns>
        public async Task Execute()
        {
            Reset();
            // Add Entity to EntitySystem to Enable drawing in the next frame.
            Entities.Add(RootEntity);

            IsRunning = true;
            while (IsRunning)
            {
                await Script.NextFrame();
                Update();
            }
        }

        /// <summary>
        /// Reset internal state of the agent.
        /// </summary>
        public void Reset()
        {
            InputEnabled = false;
            State = AgentState.Run;
            CurLane = MiddleLane;
            EnableShadow = true;
            RootEntity.Transformation.Translation.Y = 0;
            RootEntity.Transformation.Translation.X = GetXPosition(CurLane);
        }

        /// <summary>
        /// Invoke from its user to indicate that the agent has died.
        /// </summary>
        public void OnDied(float floorHeight)
        {
            State = AgentState.Die;
            RootEntity.Transformation.Translation.Y = floorHeight;
            EnableShadow = false;
        }

        /// <summary>
        /// Process input, and update the agent according to its states.
        /// </summary>
        /// <returns></returns>
        private void Update()
        {
            // Get input state from gesture, if none check from the keyboard.
            var inputState = GetInputFromGesture();

            if (inputState == InputState.None)
                inputState = GetInputFromKeyboard();

            // Process obtained input in this frame
            ProcessInput(inputState);

            // Update the agent
            UpdateState();
        }

        /// <summary>
        /// Retrieve input from the user by his/her keyboard, and transform to one of the agent's input state.
        /// </summary>
        /// <returns></returns>
        private InputState GetInputFromKeyboard()
        {
            if (Input.IsKeyPressed(Keys.Left))
            {
                return InputState.Left;
            }
            if (Input.IsKeyPressed(Keys.Right))
            {
                return InputState.Right;
            }
            if (Input.IsKeyPressed(Keys.Down))
            {
                return InputState.Down;
            }
            return InputState.None;
        }

        /// <summary>
        /// Retrieve input from the user by Drag gesture, and determine the input state by 
        /// calculating the direction of drag by  ProcessInputFromDragGesture().
        /// </summary>
        /// <returns></returns>
        private InputState GetInputFromGesture()
        {
            // Gesture recognition
            foreach (var gestureEvent in Input.GestureEvents)
            {
                // Select only Drag gesture with Began state.
                if (gestureEvent.Type == GestureType.Drag && gestureEvent.State == GestureState.Began)
                    // From Draw gesture, determine the InputState from direction of the swipe.
                    return ProcessInputFromDragGesture((GestureEventDrag)gestureEvent);
            }

            return InputState.None;
        }

        /// <summary>
        /// Process gestureEvent to determine the input state.
        /// </summary>
        /// <param name="gestureEvent"></param>
        /// <returns></returns>
        private InputState ProcessInputFromDragGesture(GestureEventDrag gestureEvent)
        {
            // Get drag vector and multiply by the screenRatio of the screen, also flip y (-screenRatio).
            var dragVector = (gestureEvent.CurrentPosition - gestureEvent.StartPosition) * new Vector2(1f, -screenRatio);
            var dragDirection = Vector2.Normalize(dragVector);

            Vector2 comparedAxis;
            float xDeg;
            float yDeg;

            // Head of dragDirection is in Quadrant 1.
            if (dragDirection.X >= 0 && dragDirection.Y >= 0)
            {
                comparedAxis = Vector2.UnitX;
                xDeg = FindAngleBetweenVector(ref dragDirection, ref comparedAxis);
                comparedAxis = Vector2.UnitY;
                yDeg = FindAngleBetweenVector(ref dragDirection, ref comparedAxis);

                return xDeg <= yDeg ? InputState.Right : InputState.None;
            }

            // Head of dragDirection is in Quadrant 2. 
            if (dragDirection.X <= 0 && dragDirection.Y >= 0)
            {
                comparedAxis = -Vector2.UnitX;
                xDeg = FindAngleBetweenVector(ref dragDirection, ref comparedAxis);
                comparedAxis = Vector2.UnitY;
                yDeg = FindAngleBetweenVector(ref dragDirection, ref comparedAxis);

                return xDeg <= yDeg ? InputState.Left : InputState.None;
            }

            // Head of dragDirection is in Quadrant 3, check if the input is left or down.
            if (dragDirection.X <= 0 && dragDirection.Y <= 0)
            {
                comparedAxis = -Vector2.UnitX;
                xDeg = FindAngleBetweenVector(ref dragDirection, ref comparedAxis);
                comparedAxis = -Vector2.UnitY;
                yDeg = FindAngleBetweenVector(ref dragDirection, ref comparedAxis);

                return xDeg <= yDeg ? InputState.Left : InputState.Down;
            }

            // Head of dragDirection is in Quadrant 4, check if the input is right or down.
            comparedAxis = Vector2.UnitX;
            xDeg = FindAngleBetweenVector(ref dragDirection, ref comparedAxis);
            comparedAxis = -Vector2.UnitY;
            yDeg = FindAngleBetweenVector(ref dragDirection, ref comparedAxis);

            return xDeg <= yDeg ? InputState.Right : InputState.Down;
        }

        private static float FindAngleBetweenVector(ref Vector2 v1, ref Vector2 v2)
        {
            float dotProd;
            Vector2.Dot(ref v1, ref v2, out dotProd);
            return (float)Math.Acos(dotProd);
        }

        /// <summary>
        /// Process user's input, according to the current state.
        /// It might change state of the agent.
        /// </summary>
        /// <param name="currentInputState"></param>
        private void ProcessInput(InputState currentInputState)
        {
            if (!InputEnabled)
                return;

            switch (currentInputState)
            {
                case InputState.Left:
                    if (CurLane != LeftLane && (State == AgentState.Run|| State == AgentState.Slide))
                        State = AgentState.ChangeLaneLeft;
                    break;
                case InputState.Right:
                    if (CurLane != RightLane && (State == AgentState.Run || State == AgentState.Slide))
                        State = AgentState.ChangeLaneRight;
                    break;
                case InputState.Down:
                    if (State == AgentState.Run)
                        State = AgentState.Slide;
                    break;
            }
        }

        /// <summary>
        /// Invoke upon enter each state. It sets initial behaviour of the Agent for that state.
        /// </summary>
        /// <param name="agentState"></param>
        private void OnEnter(AgentState agentState)
        {
            ActiveBoundingBox = (agentState == AgentState.Slide) 
                ? boundingBoxes[BoundingBoxKeys.Slide] :
                  boundingBoxes[BoundingBoxKeys.Normal];

            switch (agentState)
            {
                case AgentState.Run:
                    PlayAnimation(AgentAnimationKeys.Active);
                    break;
                case AgentState.ChangeLaneLeft:
                    OnEnterChangeLane(true);
                    break;
                case AgentState.ChangeLaneRight:
                    OnEnterChangeLane(false);
                    break;
                case AgentState.Slide:
                    PlayAnimation(AgentAnimationKeys.Slide);
                    break;
                case AgentState.Die:
                    PlayAnimation(AgentAnimationKeys.Crash);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("agentState");
            }
        }

        /// <summary>
        /// Upon Enter ChangeLane state, cache the start X position, and determine X position that will arrive for interpolation.
        /// And play animation accordingly.
        /// </summary>
        /// <param name="isChangeLaneLeft"></param>
        private void OnEnterChangeLane(bool isChangeLaneLeft)
        {
            if (isChangeLaneLeft)
            {
                startChangeLanePosX = GetXPosition(CurLane--);
                PlayAnimation(AgentAnimationKeys.DodgeLeft);
            }
            else
            {
                startChangeLanePosX = GetXPosition(CurLane++);
                PlayAnimation(AgentAnimationKeys.DodgeRight);
            }

            targetChangeLanePosX = GetXPosition(CurLane);
        }

        /// <summary>
        /// UpdateState updates the agent according to its state.
        /// </summary>
        private void UpdateState()
        {
            switch (State)
            {
                case AgentState.ChangeLaneLeft:
                case AgentState.ChangeLaneRight:
                    UpdateChangeLane();
                    break;
                case AgentState.Slide:
                    if (playingAnimation.CurrentTime.TotalSeconds >= playingAnimation.Clip.Duration.TotalSeconds)
                        State = AgentState.Run;
                    break;
            }
        }
        /// <summary>
        /// In ChangeLane state, the agent's X position is determined by Linear interpolation of the current animation process.
        /// </summary>
        private void UpdateChangeLane()
        {
            var t = (float)(playingAnimation.CurrentTime.TotalSeconds / playingAnimation.Clip.Duration.TotalSeconds);

            // Interpolate new X position in World coordinate.
            var newPosX = MathUtil.Lerp(startChangeLanePosX, targetChangeLanePosX, t);
            RootEntity.Transformation.Translation.X = newPosX;

            // Animation ends, changing state.
            if (t >= 1.0f)
                State = AgentState.Run;
        }

        /// <summary>
        /// Helper function for playing animation given AgentAnimationKey.
        /// </summary>
        /// <param name="key"></param>
        private void PlayAnimation(AgentAnimationKeys key)
        {
            var animComp = ModelEntity.Get<AnimationComponent>();
           
            animComp.Play(key.ToString());
            playingAnimation = animComp.PlayingAnimations[0];
        }

        /// <summary>
        /// Returns world position in X axis for the giving lane index.
        /// </summary>
        /// <param name="lane"></param>
        /// <returns></returns>
        private static float GetXPosition(int lane)
        {
            return (1 - lane) * LaneLength;
        }
    }
}
