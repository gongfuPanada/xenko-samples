using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Rendering;
using SiliconStudio.Paradox.Rendering.Composers;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.Input;

namespace SpriteFonts
{
    /// <summary>
    /// This sample shows how to easily manipulate font in several different ways for rendering using SpriteBatch.
    /// The features of font described in here includes: 
    /// 1. Static font
    /// 2. Dynamic font with different size
    /// 3. Font styles {Bold, Italic}
    /// 4. Alias modes {Aliased, Anti-aliased, Clear}
    /// 5. Different languages supported
    /// 6. Three alignment modes {Left, Center, Right}
    /// 7. Animated text
    /// </summary>
    public class FontScript : Script
    {
        // Time to display text groups where the first index corresponding to introduction text, and the rest corresponding to text groups
        private static readonly float[] TimeToDisplayTextGroups = { 3f /*Intro*/, 5f /*Static*/, 5f /*Dynamic*/, 4f /*Style*/, 5f /*Alias*/,
                                                                    5f /*Language*/, 5f /*Alignment*/, 10f /*Animated*/};

        private readonly List<Action> screenRenderers = new List<Action>();

        private const float FadeInDuration = 1f;
        private const float FadeOutDuration = 1f;

        private const float DynamicFontContentSize = 50;

        private const string RefenceText = @"
In the first centuries of typesetting,
quotations were distinguished merely by
indicating the speaker, and this can still
be seen in some editions of the Bible.
During the Renaissance, quotations
were distinguished by setting in a typeface
contrasting with the main body text
(often Italic type with roman,
or the other way round).
Block quotations were set this way
at full size and full measure";

        private Vector2 centerVirtualPosition;
        private Vector2 screenSize;
        private SpriteBatch spriteBatch;

        private SpriteFont staticFont;
        private SpriteFont dynamicFont;
        private SpriteFont boldFont;
        private SpriteFont italicFont;
        private SpriteFont aliasedFont;
        private SpriteFont antialiasedFont;
        private SpriteFont clearTypeFont;
        private SpriteFont japaneseFont;
        private SpriteFont timesNewRoman;
        private SpriteFont headerFont;

        private Vector2 animatedFontPosition;
        private float animatedFontScale;
        private float animatedFontRotation;
        private float animatedFontAlpha;

        private bool isPlaying = true;

        private int currentScreenIndex;

        private readonly Vector2 headerPosition = new Vector2(0.5f, 0.25f);
        private readonly Vector2 contentPosition = new Vector2(0.5f, 0.4f);

        private readonly Color paradoxColor = new Color(0x583069);
        private float currentTime;

        private Vector2 virtualResolution = new Vector2(1920, 1080);

        /// <summary>
        /// Draw all text groups with SpriteBatch
        /// </summary>
        public override void Start()
        {
            // Create the SpriteBatch used to render them
            spriteBatch = new SpriteBatch(GraphicsDevice) { VirtualResolution = new Vector3(virtualResolution, 1000) };

            centerVirtualPosition = new Vector2(virtualResolution.X * 0.5f, virtualResolution.Y * 0.5f);
            screenSize = new Vector2(GraphicsDevice.BackBuffer.Width, GraphicsDevice.BackBuffer.Height);

            // Load fonts
            staticFont = Asset.Load<SpriteFont>("StaticFont");
            dynamicFont = Asset.Load<SpriteFont>("DynamicFont");
            boldFont = Asset.Load<SpriteFont>("BoldFont");
            italicFont = Asset.Load<SpriteFont>("ItalicFont");

            aliasedFont = Asset.Load<SpriteFont>("AliasedFont");
            antialiasedFont = Asset.Load<SpriteFont>("AntialiasedFont");
            clearTypeFont = Asset.Load<SpriteFont>("ClearTypeFont");

            japaneseFont = Asset.Load<SpriteFont>("JapaneseFont");
            timesNewRoman = Asset.Load<SpriteFont>("TimesNewRoman");
            headerFont = Asset.Load<SpriteFont>("HeaderFont");

            screenRenderers.Add(DrawIntroductionCategory);
            screenRenderers.Add(DrawStaticCategory);
            screenRenderers.Add(DrawDynamicCategory);
            screenRenderers.Add(DrawStyleCategory);
            screenRenderers.Add(DrawAliasCategory);
            screenRenderers.Add(DrawLanguageCategory);
            screenRenderers.Add(DrawAlignmentCategory);
            screenRenderers.Add(DrawAnimationCategory);

            // Add the background task to update the font parameters
            Script.AddTask(UpdateAnimatedFontParameters);
            Script.AddTask(UpdateInput);
            Script.AddTask(UpdateCurrentScreenIndex);

            // Add Graphics Layer
            var scene = SceneSystem.SceneInstance.Scene;
            var compositor = ((SceneGraphicsCompositorLayers) scene.Settings.GraphicsCompositor);
            compositor.Master.Renderers.Add(new SceneDelegateRenderer(DrawFont));
        }

        #region Draw Methods

        private void DrawFont(RenderContext context, RenderFrame frame)
        {
            if(isPlaying)
                currentTime += (float)Game.UpdateTime.Elapsed.TotalSeconds;

            spriteBatch.Begin();
            screenRenderers[currentScreenIndex]();
            spriteBatch.End();
        }

        private void DrawHeader(string headerPart1, string headerPart2, string headerPart3)
        {
            const float headerSize = 70;

            var position = GetVirtualPosition(headerPosition);

            // Find the X position offset for the first part of text
            position -= spriteBatch.MeasureString(headerFont, headerPart1 + headerPart2 + headerPart3, headerSize, screenSize) * 0.5f;

            // Draw each part separately because we need to have a different color in the 2nd part
            spriteBatch.DrawString(headerFont, headerPart1, headerSize, position, Color.White * GetInterpolatedAlpha());

            position.X += spriteBatch.MeasureString(headerFont, headerPart1, headerSize, screenSize).X;

            spriteBatch.DrawString(headerFont, headerPart2, headerSize, position, paradoxColor * GetInterpolatedAlpha());

            position.X += spriteBatch.MeasureString(headerFont, headerPart2, headerSize, screenSize).X;

            spriteBatch.DrawString(headerFont, headerPart3, headerSize, position, Color.White * GetInterpolatedAlpha());
        }

        /// <summary>
        /// Draw "Introduction" text group.
        /// Render Paradox SpriteFont sample introduction page.
        /// </summary>
        private void DrawIntroductionCategory()
        {
            // Draw Create {cross-platform} {game} {in C#} in three pieces separately
            const float textSize = 80;
            const string textPart1 = "Create cross-platform";
            const string textPart2 = " games ";
            const string textPart3 = "in C#";

            var position = GetVirtualPosition(0.5f, 0.5f);

            // Find the X position offset for the first part of text
            position -= spriteBatch.MeasureString(dynamicFont, textPart1 + textPart2 + textPart3, textSize, screenSize) * 0.5f;

            // Draw each part separately because we need to have a different color in the 2nd part
            spriteBatch.DrawString(dynamicFont, textPart1, textSize, position, Color.White * GetInterpolatedAlpha());

            position.X += spriteBatch.MeasureString(dynamicFont, textPart1, textSize, screenSize).X;

            spriteBatch.DrawString(dynamicFont, textPart2, textSize, position, paradoxColor * GetInterpolatedAlpha());

            position.X += spriteBatch.MeasureString(dynamicFont, textPart2, textSize, screenSize).X;

            spriteBatch.DrawString(dynamicFont, textPart3, textSize, position, Color.White * GetInterpolatedAlpha());
        }

        /// <summary>
        /// Draw "Static" text group.
        /// Render text created in compiling-time which could not change in run-time.
        /// </summary>
        private void DrawStaticCategory()
        {
            DrawHeader("Compile-time rendered ", "static", " fonts");

            var position = GetVirtualPosition(contentPosition);

            var text = "Embeds only required characters into the database\n" +
                       "Does not require any rendering time at execution\n" +
                       "Cannot adjust their size to the virtual resolution\n" +
                       "Cannot modify their size at run-time";

            position.X -= spriteBatch.MeasureString(staticFont, text, screenSize).X / 2;

            spriteBatch.DrawString(staticFont, text, position, Color.White * GetInterpolatedAlpha());
        }

        /// <summary>
        /// Draw "Dynamic" text group.
        /// Display text created dynamically in different sizes.
        /// </summary>
        private void DrawDynamicCategory()
        {
            DrawHeader("Run-time rendered ", "dynamic", " fonts");

            var text = "Embeds all characters of the font into the database\n" +
                       "Is rendered at execution time and requires some time for rendering\n" +
                       "Can adjust their size to the virtual resolution\n";

            var position = GetVirtualPosition(contentPosition);
            var firstTextSize = spriteBatch.MeasureString(dynamicFont, text, DynamicFontContentSize, screenSize);

            position.X -= firstTextSize.X / 2;

            spriteBatch.DrawString(dynamicFont, text, DynamicFontContentSize, position, Color.White * GetInterpolatedAlpha());

            text = "Can modify their size at execution time";

            position.Y += firstTextSize.Y;
            spriteBatch.DrawString(dynamicFont, text, 80, position, Color.White * GetInterpolatedAlpha());
        }

        /// <summary>
        /// Draw "Style" text group.
        /// Illustrate possible styles of font that can be rendered in compile-time {Italic, Bold}
        /// </summary>
        private void DrawStyleCategory()
        {
            DrawHeader("Support common font ", "styles", "");

            var position = GetVirtualPosition(contentPosition);

            var text = "None - This is a sample sentence.";
            var firstTextSize = spriteBatch.MeasureString(dynamicFont, text, DynamicFontContentSize, screenSize);
            position.X -= firstTextSize.X / 2;
            spriteBatch.DrawString(dynamicFont, text, DynamicFontContentSize, position, Color.White * GetInterpolatedAlpha());

            text = "Italic - This is a sample sentence.";
            position.Y += firstTextSize.Y;
            spriteBatch.DrawString(italicFont, text, DynamicFontContentSize, position, Color.White * GetInterpolatedAlpha());

            text = "Bold - This is a sample sentence.";
            position.Y += spriteBatch.MeasureString(italicFont, text, DynamicFontContentSize, screenSize).Y;
            spriteBatch.DrawString(boldFont, text, DynamicFontContentSize, position, Color.White * GetInterpolatedAlpha());
        }

        /// <summary>
        /// Draw "Alias" text group.
        /// Display all three possible alias modes {Aliased, Anti-aliased, Clear type}.
        /// </summary>
        private void DrawAliasCategory()
        {
            DrawHeader("Support common ", "anti-aliasing", " modes");

            var position = GetVirtualPosition(contentPosition);

            var text = "Aliased - This is a sample sentence.";
            var firstTextSize = spriteBatch.MeasureString(aliasedFont, text, DynamicFontContentSize, screenSize);
            position.X -= firstTextSize.X / 2;
            spriteBatch.DrawString(aliasedFont, text, position, Color.White * GetInterpolatedAlpha());

            position.Y += firstTextSize.Y;
            text = "Anti-aliased - This is a sample sentence.";
            spriteBatch.DrawString(antialiasedFont, text, position, Color.White * GetInterpolatedAlpha());

            position.Y += spriteBatch.MeasureString(antialiasedFont, text, screenSize).Y;
            text = "Clear-type - This is a sample sentence.";
            spriteBatch.DrawString(clearTypeFont, text, position, Color.White * GetInterpolatedAlpha());
        }

        /// <summary>
        /// Draw "Language" text group.
        /// Show Japanese dynamic font, by rendering a japanese paragraph.
        /// Other pictogram alphabets are supported as well.
        /// </summary>
        private void DrawLanguageCategory()
        {
            DrawHeader("Support ", "pictogram-based", " fonts");

            var sizeIncreament = 15;
            var position = GetVirtualPosition(contentPosition);
            var text = "Japanese dynamic sprite font\nあいうえおかきくけこ   天竜の\nアイウエオカキクケコ   幅八町の\n一二三四五六七八九十   梅雨濁り";

            position.X -= spriteBatch.MeasureString(japaneseFont, text, DynamicFontContentSize + sizeIncreament, screenSize).X / 2;
            spriteBatch.DrawString(japaneseFont, text, DynamicFontContentSize + sizeIncreament, position, Color.White * GetInterpolatedAlpha());
        }

        /// <summary>
        /// Draw "Alignment" text group.
        /// Display three paragraphs showing possible alignments {Left, Center, Right}
        /// </summary>
        private void DrawAlignmentCategory()
        {
            DrawHeader("Support standard ", "text alignment", " modes");

            var position = GetVirtualPosition(contentPosition);

            // Draw content
            position.X = virtualResolution.X * 0.03f;
            var text = "LEFT-ALIGNED TEXT\n" + RefenceText;

            var textSize = 28;

            spriteBatch.DrawString(timesNewRoman, text, textSize, position, Color.White * GetInterpolatedAlpha());

            position.X = centerVirtualPosition.X - 0.5f * spriteBatch.MeasureString(timesNewRoman, text, textSize, screenSize).X;
            text = "CENTERED TEXT\n" + RefenceText;

            spriteBatch.DrawString(timesNewRoman, text, textSize, position, Color.White * GetInterpolatedAlpha(), TextAlignment.Center);

            position.X = virtualResolution.X - spriteBatch.MeasureString(timesNewRoman, text, textSize, screenSize).X - virtualResolution.X * 0.03f;
            text = "RIGHT-ALIGNED TEXT\n" + RefenceText;

            spriteBatch.DrawString(timesNewRoman, text, textSize, position, Color.White * GetInterpolatedAlpha(), TextAlignment.Right);
        }

        /// <summary>
        /// Draw "Animation" text group.
        /// Illustrate an animate text.
        /// </summary>
        private void DrawAnimationCategory()
        {
            DrawHeader("Easily ", "animate", " your texts!");

            // Draw content
            var text = "Paradox Engine";

            spriteBatch.DrawString(dynamicFont, text, DynamicFontContentSize, animatedFontPosition, animatedFontAlpha * Color.White * GetInterpolatedAlpha(), animatedFontRotation,
                0.5f * spriteBatch.MeasureString(dynamicFont, text, DynamicFontContentSize, screenSize), animatedFontScale * Vector2.One, SpriteEffects.None, 0f, TextAlignment.Left);
        }

        #endregion Draw methods

        /// <summary>
        /// Check if there is any input command.
        /// Input commands are for controlling: 1. Text group advancing, 2. Previous/Next text group selection.
        /// </summary>
        /// <returns></returns>
        private async Task UpdateInput()
        {
            while (Game.IsRunning)
            {
                // Wait for next frame
                await Script.NextFrame();

                // Toggle play/not play
                if (Input.IsKeyPressed(Keys.Space) || Input.PointerEvents.Any(pointerEvent => pointerEvent.State == PointerState.Down))
                {
                    isPlaying = !isPlaying;
                }
                else if (Input.IsKeyPressed(Keys.Left) || Input.IsKeyPressed(Keys.Right))
                {
                    currentTime = 0;
                    currentScreenIndex = (currentScreenIndex + (Input.IsKeyPressed(Keys.Left) ? -1 : +1) + screenRenderers.Count) % screenRenderers.Count;
                }
            }
        }

        private async Task UpdateCurrentScreenIndex()
        {
            while (Game.IsRunning)
            {
                await Script.NextFrame();

                var upperBound = TimeToDisplayTextGroups[currentScreenIndex];

                if (currentTime > upperBound)
                {
                    currentTime = 0;
                    currentScreenIndex = (currentScreenIndex + 1) % screenRenderers.Count;
                }
            }
        }

        /// <summary>
        /// Update the main font parameters according to sample state.
        /// </summary>
        /// <returns></returns>
        private async Task UpdateAnimatedFontParameters()
        {
            while (Game.IsRunning)
            {
                // Wait for next frame
                await Script.NextFrame();

                if (!isPlaying)
                    continue;

                animatedFontAlpha = GetVaryingValue(1.6f * currentTime);
                animatedFontRotation = 2f * currentTime * (float)Math.PI;
                animatedFontPosition = GetVirtualPosition(0.5f, 0.65f) + 160 * new Vector2(1.5f * (float)Math.Cos(1.5f * currentTime), (float)Math.Sin(1.5f * currentTime));
                animatedFontScale = 0.9f + 0.2f * GetVaryingValue(2.5f * currentTime);
            }
        }

        /// <summary>
        /// Return interpolated value for alpha channel of a text that controls opacity.
        /// Value, that is outside the bound, would not invisible.
        /// </summary>
        /// <returns></returns>
        private float GetInterpolatedAlpha()
        {
            var upperBound = TimeToDisplayTextGroups[currentScreenIndex];

            if (currentTime < FadeInDuration)
                return currentTime / FadeInDuration;

            if (currentTime < upperBound - FadeOutDuration)
                return 1f;

            return Math.Max(upperBound - currentTime, 0) / FadeOutDuration;
        }

        /// <summary>
        /// Return position in virtual resolution coordinate by given relative position [0, 1]
        /// </summary>
        /// <param name="relativePositionX"></param>
        /// <param name="relativePositionY"></param>
        /// <returns></returns>
        private Vector2 GetVirtualPosition(float relativePositionX, float relativePositionY)
        {
            return GetVirtualPosition(new Vector2(relativePositionX, relativePositionY));
        }

        /// <summary>
        /// Return position in virtual resolution coordinate by given relative position [0, 1]
        /// </summary>
        /// <returns></returns>
        private Vector2 GetVirtualPosition(Vector2 relativePosition)
        {
            return new Vector2(virtualResolution.X * relativePosition.X, virtualResolution.Y * relativePosition.Y);
        }

        /// <summary>
        /// Get a varying value between [0,1] depending on the time
        /// </summary>
        /// <param name="time">the current time</param>
        /// <returns>the varying value</returns>
        private static float GetVaryingValue(float time)
        {
            return (float)Math.Cos(time) * 0.5f + 0.5f;
        }
    }
}
