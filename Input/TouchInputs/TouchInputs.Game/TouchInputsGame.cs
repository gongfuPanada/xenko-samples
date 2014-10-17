using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox;
using SiliconStudio.Paradox.Effects;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;

namespace TouchInputs
{
    public class TouchInputsGame : Game
    {
        private const float TextSpaceY = 3;
        private const float TextSubSectionOffsetX = 15;
        private const string KeyboardSessionString = "Keyboard :";

        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont11;

        private Texture roundTexture;
        private Vector2 roundTextureSize;

        private readonly Color fontColor = Color.WhiteSmoke;

        private float textHeight;
        private readonly Vector2 textLeftTopCorner = new Vector2(5, 5);

        private Vector2 screenSize;

        // keyboard
        private string keyEvents;
        private string keyDown;

        // mouse
        private Vector2 mousePosition;
        private string mouseButtonPressed;
        private string mouseButtonDown;
        private string mouseButtonReleased;

        private readonly Color mouseColor = Color.DarkGray;

        // pointers
        private readonly Queue<Tuple<Vector2, TimeSpan>> pointerPressed = new Queue<Tuple<Vector2, TimeSpan>>();
        private readonly Queue<Tuple<Vector2, TimeSpan>> pointerMoved = new Queue<Tuple<Vector2, TimeSpan>>();
        private readonly Queue<Tuple<Vector2, TimeSpan>> pointerReleased = new Queue<Tuple<Vector2, TimeSpan>>();

        private readonly TimeSpan displayPointerDuration = TimeSpan.FromSeconds(1.5f);

        // Gestures
        private string dragEvent;
        private string flickEvent;
        private string longPressEvent;
        private string compositeEvent;
        private string tapEvent;

        private Tuple<GestureEvent, TimeSpan> lastFlickEvent = new Tuple<GestureEvent, TimeSpan>(null, TimeSpan.Zero);
        private Tuple<GestureEvent, TimeSpan> lastLongPressEvent = new Tuple<GestureEvent, TimeSpan>(null, TimeSpan.Zero);
        private Tuple<GestureEvent, TimeSpan> lastTapEvent = new Tuple<GestureEvent, TimeSpan>(null, TimeSpan.Zero);

        // GamePads
        private string gamePadText;

        private readonly TimeSpan displayGestureDuration = TimeSpan.FromSeconds(1f);
        
        public TouchInputsGame()
        {
            // Target 9.1 profile by default
            GraphicsDeviceManager.PreferredGraphicsProfile = new[] { GraphicsProfile.Level_9_1 };
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            CreateRenderingPipeline();

            // Load the fonts
            spriteFont11 = Asset.Load<SpriteFont>("Font");

            // load the round texture 
            roundTexture = Asset.Load<Texture2D>("round");

            // create the SpriteBatch used to render them
            spriteBatch = new SpriteBatch(GraphicsDevice) {VirtualResolution = VirtualResolution};

            // initialize parameters
            textHeight = spriteFont11.MeasureString(KeyboardSessionString).Y;
            screenSize = new Vector2(VirtualResolution.X, VirtualResolution.Y);
            roundTextureSize = new Vector2(roundTexture.Width, roundTexture.Height);

            // activate the gesture recognitions
            Input.ActivatedGestures.Add(new GestureConfigDrag());
            Input.ActivatedGestures.Add(new GestureConfigFlick());
            Input.ActivatedGestures.Add(new GestureConfigLongPress());
            Input.ActivatedGestures.Add(new GestureConfigComposite());
            Input.ActivatedGestures.Add(new GestureConfigTap());

            // add a task to the task scheduler that will be executed asynchronously 
            Script.Add(UpdateInputStates);
        }

        private void CreateRenderingPipeline()
        {
            // Setup the default rendering pipeline
            RenderSystem.Pipeline.Renderers.Add(new RenderTargetSetter(Services));
            RenderSystem.Pipeline.Renderers.Add(new BackgroundRenderer(Services, "ParadoxBackground"));
            RenderSystem.Pipeline.Renderers.Add(new DelegateRenderer(Services) { Render = Render});
        }

        private void Render(RenderContext renderContext)
        {
            spriteBatch.Begin();

            // render the keyboard key states
            spriteBatch.DrawString(spriteFont11, KeyboardSessionString, textLeftTopCorner, fontColor);
            spriteBatch.DrawString(spriteFont11, "Key pressed/released: " + keyEvents, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 1 * (textHeight + TextSpaceY)), fontColor);
            spriteBatch.DrawString(spriteFont11, "Key down: " + keyDown, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 2 * (textHeight + TextSpaceY)), fontColor);

            // render the mouse key states
            spriteBatch.DrawString(spriteFont11, "Mouse :", textLeftTopCorner + new Vector2(0, 4 * (textHeight + TextSpaceY)), fontColor);
            spriteBatch.DrawString(spriteFont11, "Mouse position: " + mousePosition, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 5 * (textHeight + TextSpaceY)), fontColor);
            spriteBatch.DrawString(spriteFont11, "Mouse button pressed: " + mouseButtonPressed, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 6 * (textHeight + TextSpaceY)), fontColor);
            spriteBatch.DrawString(spriteFont11, "Mouse button down: " + mouseButtonDown, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 7 * (textHeight + TextSpaceY)), fontColor);
            spriteBatch.DrawString(spriteFont11, "Mouse button released: " + mouseButtonReleased, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 8 * (textHeight + TextSpaceY)), fontColor);

            var mouseScreenPosition = new Vector2(mousePosition.X * screenSize.X, mousePosition.Y * screenSize.Y);
            spriteBatch.Draw(roundTexture, mouseScreenPosition, mouseColor, 0, roundTextureSize / 2, 0.1f);

            // render the pointer states
            foreach (var tuple in pointerPressed)
                DrawPointers(tuple, 1.5f, Color.Blue);
            foreach (var tuple in pointerMoved)
                DrawPointers(tuple, 1f, Color.Green);
            foreach (var tuple in pointerReleased)
                DrawPointers(tuple, 2f, Color.Red);

            // render the gesture states
            spriteBatch.DrawString(spriteFont11, "Gestures :", textLeftTopCorner + new Vector2(0, 9 * (textHeight + TextSpaceY)), fontColor);
            spriteBatch.DrawString(spriteFont11, "Drag: " + dragEvent, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 10 * (textHeight + TextSpaceY)), fontColor);
            spriteBatch.DrawString(spriteFont11, "Flick: " + flickEvent, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 11 * (textHeight + TextSpaceY)), fontColor);
            spriteBatch.DrawString(spriteFont11, "LongPress: " + longPressEvent, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 12 * (textHeight + TextSpaceY)), fontColor);
            spriteBatch.DrawString(spriteFont11, "Composite: " + compositeEvent, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 13 * (textHeight + TextSpaceY)), fontColor);
            spriteBatch.DrawString(spriteFont11, "Tap: " + tapEvent, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 14 * (textHeight + TextSpaceY)), fontColor);

            spriteBatch.DrawString(spriteFont11, "GamePads: " + gamePadText, textLeftTopCorner + new Vector2(TextSubSectionOffsetX, 16 * (textHeight + TextSpaceY)), fontColor);
            spriteBatch.End();
        }

        private void DrawPointers(Tuple<Vector2, TimeSpan> tuple, float baseScale, Color baseColor)
        {
            var position = tuple.Item1;
            var duration = DrawTime.Total - tuple.Item2;

            var scale = (float)(0.2f * (1f - duration.TotalSeconds / displayPointerDuration.TotalSeconds));
            var pointerScreenPosition = new Vector2(position.X * screenSize.X, position.Y * screenSize.Y);

            spriteBatch.Draw(roundTexture, pointerScreenPosition, baseColor, 0, roundTextureSize / 2, scale * baseScale);
        }

        private async Task UpdateInputStates()
        {
            while (true)
            {
                await Script.NextFrame();

                var currentTime = DrawTime.Total;

                keyDown = "";
                keyEvents = "";
                mouseButtonPressed = "";
                mouseButtonDown = "";
                mouseButtonReleased = "";
                dragEvent = "";
                flickEvent = "";
                longPressEvent = "";
                compositeEvent = "";
                tapEvent = "";
                gamePadText = "";

                // Keyboard
                if (Input.HasKeyboard)
                {
                    foreach (var keyEvent in Input.KeyEvents)
                        keyEvents += keyEvent + ", ";

                    foreach (var key in Input.KeyDown)
                        keyDown += key + ", ";
                }

                // Mouse
                if (Input.HasMouse)
                {
                    mousePosition = Input.MousePosition;
                    for (int i = 0; i <= (int)MouseButton.Extended2; i++)
                    {
                        var button = (MouseButton)i;
                        if (Input.IsMouseButtonPressed(button))
                            mouseButtonPressed += button + ", ";
                        if (Input.IsMouseButtonDown(button))
                            mouseButtonDown += button + ", ";
                        if (Input.IsMouseButtonReleased(button))
                            mouseButtonReleased += button + ", ";
                    }
                }

                // Pointers
                if (Input.HasPointer)
                {
                    foreach (var pointerEvent in Input.PointerEvents)
                    {
                        switch (pointerEvent.State)
                        {
                            case PointerState.Down:
                                pointerPressed.Enqueue(Tuple.Create(pointerEvent.Position, currentTime));
                                break;
                            case PointerState.Move:
                                pointerMoved.Enqueue(Tuple.Create(pointerEvent.Position, currentTime));
                                break;
                            case PointerState.Up:
                                pointerReleased.Enqueue(Tuple.Create(pointerEvent.Position, currentTime));
                                break;
                            case PointerState.Out:
                            case PointerState.Cancel:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }

                    // remove too old pointer events
                    RemoveOldPointerEventInfo(pointerPressed);
                    RemoveOldPointerEventInfo(pointerMoved);
                    RemoveOldPointerEventInfo(pointerReleased);
                }

                // Gestures
                foreach (var gestureEvent in Input.GestureEvents)
                {
                    switch (gestureEvent.Type)
                    {
                        case GestureType.Drag:
                            var dragGestureEvent = (GestureEventDrag)gestureEvent;
                            dragEvent = "Translation = " + dragGestureEvent.TotalTranslation;
                            break;
                        case GestureType.Flick:
                            lastFlickEvent = Tuple.Create(gestureEvent, currentTime);
                            break;
                        case GestureType.LongPress:
                            lastLongPressEvent = Tuple.Create(gestureEvent, currentTime);
                            break;
                        case GestureType.Composite:
                            var compositeGestureEvent = (GestureEventComposite)gestureEvent;
                            compositeEvent = "Rotation = " + compositeGestureEvent.TotalRotation + " - Scale = " + compositeGestureEvent.TotalScale + " - Translation = " + compositeGestureEvent.TotalTranslation;
                            break;
                        case GestureType.Tap:
                            lastTapEvent = Tuple.Create(gestureEvent, currentTime);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }

                if (Input.HasGamePad)
                {
                    for (int i = 0; i < Input.GamePadCount; i++)
                    {
                        var gamePadState = Input.GetGamePad(i);
                        gamePadText += "\n[" + i + "] " + gamePadState;
                    }
                }

                if (currentTime - lastFlickEvent.Item2 < displayGestureDuration && lastFlickEvent.Item1 != null)
                {
                    var flickGestureEvent = (GestureEventFlick)lastFlickEvent.Item1;
                    flickEvent = " Start Position = " + flickGestureEvent.StartPosition + " - Speed = " + flickGestureEvent.AverageSpeed;
                }
                if (currentTime - lastLongPressEvent.Item2 < displayGestureDuration && lastLongPressEvent.Item1 != null)
                {
                    var longPressGestureEvent = (GestureEventLongPress)lastLongPressEvent.Item1;
                    longPressEvent = " Position = " + longPressGestureEvent.Position;
                }
                if (currentTime - lastTapEvent.Item2 < displayGestureDuration && lastTapEvent.Item1 != null)
                {
                    var tapGestureEvent = (GestureEventTap)lastTapEvent.Item1;
                    tapEvent = " Position = " + tapGestureEvent.TapPosition + " - number of taps = " + tapGestureEvent.NumberOfTaps;
                }
            }
        }

        /// <summary>
        /// Utility function to remove old pointer event from the queues
        /// </summary>
        /// <param name="tuples">the pointers event position and triggered time.</param>
        private void RemoveOldPointerEventInfo(Queue<Tuple<Vector2, TimeSpan>> tuples)
        {
            while (tuples.Count > 0 && UpdateTime.Total - tuples.Peek().Item2 > displayPointerDuration)
                tuples.Dequeue();
        }
    }
}
