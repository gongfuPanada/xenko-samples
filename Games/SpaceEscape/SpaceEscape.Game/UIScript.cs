using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Engine;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.UI;
using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Panels;

namespace SpaceEscape
{
    /// <summary>
    /// UIScript manages UIElements using in the game.
    /// At one time UI.RootElement is one of each root which corresponding to each state of the game.
    /// 
    /// It provides a ButtonClickedEvent Action that could be subscribed by its user.
    /// This action provides the name of Button element that is clicked,
    ///  which is one of {StartButton, MenuBotton and RestartButton}
    /// </summary>
    public class UIScript : StartupScript
    {
        internal Button StartButton { get; private set; }
        internal Button MenuButton { get; private set; }
        internal Button RetryButton { get; private set; }

        private ModalElement mainMenuRoot;
        private Canvas gameRoot;
        private ModalElement gameOverRoot;

        private TextBlock distanceTextBlock;
        private SpriteFont spriteFont;
        private Sprite buttonImage;
        private SpriteSheet uiImages;

        /// <summary>
        /// Load resource and construct ui components
        /// </summary>
        public override void Start()
        {
            base.Start();
            
            // Load resources shared by different UI screens
            uiImages = Asset.Load<SpriteSheet>("UIImages");
            spriteFont = Asset.Load<SpriteFont>("Font");
            buttonImage = uiImages["button"];

            // Load and create specific UI screens.
            CreateMainMenuUI();
            CreateGameUI();
            CreateGameOverUI();
        }

        private void CreateMainMenuUI()
        {
            var paradoxLogo = new ImageElement { Source = uiImages["pdx_logo"] };

            paradoxLogo.SetCanvasPinOrigin(new Vector3(0.5f, 0.5f, 1f));
            paradoxLogo.SetCanvasRelativeSize(new Vector3(0.5f, 0.5f, 1f));
            paradoxLogo.SetCanvasRelativePosition(new Vector3(0.5f, 0.3f, 1f));

            StartButton = new Button
            {
                Content = new TextBlock { Font = spriteFont, Text = "Touch to Start", TextColor = Color.Black, 
                    HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center},
                NotPressedImage = buttonImage,
                PressedImage = buttonImage,
                MouseOverImage = buttonImage,
                Padding = new Thickness(80, 27, 25, 35),
                MinimumWidth = 250f,
            };

            StartButton.SetCanvasPinOrigin(new Vector3(0.5f, 0.5f, 1f));
            StartButton.SetCanvasRelativePosition(new Vector3(0.5f, 0.7f, 0f));

            var mainMenuCanvas = new Canvas();
            mainMenuCanvas.Children.Add(paradoxLogo);
            mainMenuCanvas.Children.Add(StartButton);

            mainMenuRoot = new ModalElement
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = mainMenuCanvas
            };
        }

        private void CreateGameUI()
        {
            distanceTextBlock = new TextBlock { Font = spriteFont, TextColor = Color.Gold, VerticalAlignment = VerticalAlignment.Center };
            distanceTextBlock.SetCanvasPinOrigin(new Vector3(0.5f, 0.5f, 1f));
            distanceTextBlock.SetCanvasRelativePosition(new Vector3(0.2f, 0.05f, 0f));

            var scoreBoard = new ContentDecorator
            {
                BackgroundImage = uiImages["distance_bg"],
                Content = distanceTextBlock,
                Padding = new Thickness(60, 31, 25, 35),
                MinimumWidth = 290f // Set the minimum width of score button so that it wont modify when the content (text) changes, and less than minimum.
            };

            gameRoot = new Canvas();
            gameRoot.Children.Add(scoreBoard);
        }

        private void CreateGameOverUI()
        {
            MenuButton = new Button
            {
                Content = new TextBlock { Font = spriteFont, Text = "Menu", TextColor = Color.Black, 
                    HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center},
                PressedImage = buttonImage,
                NotPressedImage = buttonImage,
                MouseOverImage = buttonImage,
                Padding = new Thickness(77, 29, 25, 35),
                MinimumWidth = 190f,
            };

            MenuButton.SetCanvasPinOrigin(new Vector3(0.5f, 0.5f, 1f));
            MenuButton.SetCanvasRelativePosition(new Vector3(0.70f, 0.7f, 0f));

            RetryButton = new Button
            {
                Content = new TextBlock { Font = spriteFont, Text = "Retry", TextColor = Color.Black, 
                    HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center},
                Padding = new Thickness(74, 29, 25, 35),
                MinimumWidth = 190f,
                PressedImage = buttonImage,
                MouseOverImage = buttonImage,
                NotPressedImage = buttonImage
            };

            RetryButton.SetCanvasPinOrigin(new Vector3(0.5f, 0.5f, 1f));
            RetryButton.SetCanvasRelativePosition(new Vector3(0.3f, 0.7f, 0f));

            var gameOverCanvas = new Canvas();
            gameOverCanvas.Children.Add(MenuButton);
            gameOverCanvas.Children.Add(RetryButton);

            gameOverRoot = new ModalElement
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Content = gameOverCanvas,
                MinimumWidth = 200f,
            };
        }

        /// <summary>
        /// Change UI mode to main menu
        /// </summary>
        public void StartMainMenuMode()
        {
            Entity.Get<UIComponent>().RootElement = mainMenuRoot;
        }

        /// <summary>
        /// Change UI mode to game
        /// </summary>
        public void StartPlayMode()
        {
            Entity.Get<UIComponent>().RootElement = gameRoot;
        }

        /// <summary>
        /// Change ui mode to game over
        /// </summary>
        public void StartGameOverMode()
        {
            Entity.Get<UIComponent>().RootElement = gameOverRoot;
        }

        /// <summary>
        /// A function to update UI distance element.
        /// </summary>
        /// <param name="distance"></param>
        public void SetDistance(int distance)
        {
            distanceTextBlock.Text = "Distance : {0,6}".ToFormat(distance);
        }
    }
}
