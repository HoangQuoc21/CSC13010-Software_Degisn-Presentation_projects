using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace Car_Racing_Game
{
    /*This is the implementation of State design patterns*/
    public interface IGameState
    {
        abstract void Handle(MainWindow game);
    }

    public class NormalState : IGameState
    {
        public void Handle(MainWindow game)
        {
            // Reset to base speed
            game.displaySpeed = game.baseSpeed; 
            game.displayState = "Normal";
        }
    }

    public class FastState : IGameState
    {
        public void Handle(MainWindow game)
        {
            //Increase speed
            game.displaySpeed = game.fastSpeed; 
            game.displayState = "Fast";
        }
    }

    public class SlowState : IGameState
    {
        public void Handle(MainWindow game)
        {
            //Decrease speed
            game.displaySpeed = game.slowSpeed;
            game.displayState = "Slow";
        }
    }
    //==================================================================================================

    /* This is the GameStates class which stores all the possible state can happens in the gam */
    public class GameStates
    {
        public IGameState normal = new NormalState();
        public IGameState fast = new FastState();
        public IGameState slow = new SlowState();
    }
    //==================================================================================================

    /* This is the program's main class: MainWindow */
    public partial class MainWindow : Window
    {
        // These three variables are used to manage the game
        readonly DispatcherTimer gameTimer = new DispatcherTimer();
        readonly DispatcherTimer stateTimer = new DispatcherTimer();
        readonly List<Rectangle> itemRemover = new List<Rectangle>();

        // this is the random object used to generate random "fast" and "slow" objects
        readonly Random rand = new Random();

        // These three ImageBrush objects are used to set the images of the player, fast, and slow objects
        readonly ImageBrush playerImage = new ImageBrush();
        readonly ImageBrush fastImage = new ImageBrush();
        readonly ImageBrush slowImage = new ImageBrush();

        // This is the Rect object used to store the player's hitbox
        Rect playerHitBox;

        // These three variables are used to set the base, fast, and slow speeds
        public int baseSpeed = 20;
        public int fastSpeed = 50;
        public int slowSpeed = 5;

        // These three variables are used to store the player's speed, state, and the current speed of the game
        public int displaySpeed;
        public string displayState;
        public int playerSpeed = 20;

        // This is the int objectCounter used to count the number of objects created
        int objectCounter = 20;

        // These three boolean variables are used to check if the player is moving left, right
        bool moveLeft, moveRight;

        // This is the attribute currentState used to store the current state of the game
        IGameState currentState;

        // This is the possible GameStates
        public GameStates gameStates = new GameStates();


        #pragma warning disable CS8618
        public MainWindow()
        {
            InitializeComponent();
            currentState = gameStates.normal;
            myCanvas.Focus();
            gameTimer.Tick += GameLoop;
            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            stateTimer.Tick += StateTimerTick;
            stateTimer.Interval = TimeSpan.FromSeconds(5);
            StartGame();
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            stateText.Content = $"Current State: {displayState}";
            speedText.Content = $"Current Speed: {displaySpeed} km/h";

            objectCounter -= 1;
            playerHitBox = new Rect(Canvas.GetLeft(player), Canvas.GetTop(player), player.Width, player.Height);

            if (moveLeft && Canvas.GetLeft(player) > 0)
            {
                Canvas.SetLeft(player, Canvas.GetLeft(player) - playerSpeed);
            }
            if (moveRight && Canvas.GetLeft(player) + 90 < Application.Current.MainWindow.Width)
            {
                Canvas.SetLeft(player, Canvas.GetLeft(player) + playerSpeed);
            }

            if (objectCounter < 1)
            {
                MakeSpeedObject();
                objectCounter = rand.Next(200, 400);
            }

            foreach (var x in myCanvas.Children.OfType<Rectangle>())
            {
                if ((string)x.Tag == "roadMarks")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) + displaySpeed);
                    if (Canvas.GetTop(x) > 510)
                    {
                        Canvas.SetTop(x, -152);
                    }
                }
                else if ((string)x.Tag == "fast" || (string)x.Tag == "slow")
                {
                    Canvas.SetTop(x, Canvas.GetTop(x) + 5);
                    Rect objHitBox = new Rect(Canvas.GetLeft(x), Canvas.GetTop(x), x.Width, x.Height);
                    if (playerHitBox.IntersectsWith(objHitBox))
                    {
                        itemRemover.Add(x);
                        if ((string)x.Tag == "fast")
                        {
                            SetState(gameStates.fast);
                        }
                        else if ((string)x.Tag == "slow")
                        {
                            SetState(gameStates.slow);
                        }
                    }
                    if (Canvas.GetTop(x) > 400)
                    {
                        itemRemover.Add(x);
                    }
                }
            }

            foreach (Rectangle y in itemRemover)
            {
                myCanvas.Children.Remove(y);
            }
            itemRemover.Clear();
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                moveLeft = true;
            }
            if (e.Key == Key.Right)
            {
                moveRight = true;
            }
        }

        private void OnKeyUP(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                moveLeft = false;
            }
            if (e.Key == Key.Right)
            {
                moveRight = false;
            }
        }

        private void StartGame()
        {
            displaySpeed = baseSpeed;
            displayState = "Normal";

            gameTimer.Start();
            moveLeft = false;
            moveRight = false;

            playerImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/player.png"));
            fastImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/fast.png"));
            slowImage.ImageSource = new BitmapImage(new Uri("pack://application:,,,/images/slow.png"));
            myCanvas.Background = Brushes.Gray;

            foreach (var x in myCanvas.Children.OfType<Rectangle>())
            {
                if ((string)x.Tag == "fast" || (string)x.Tag == "slow")
                {
                    itemRemover.Add(x);
                }
            }
            itemRemover.Clear();
        }

        private void MakeSpeedObject()
        {
            // Create and add a "fast" object
            Rectangle newFast = new Rectangle
            {
                Height = 50,
                Width = 50,
                Tag = "fast",
                Fill = fastImage
            };
            Canvas.SetLeft(newFast, rand.Next(0, 430));
            Canvas.SetTop(newFast, (rand.Next(100, 400) * -1));
            myCanvas.Children.Add(newFast);

            // Create and add a "slow" object
            Rectangle newSlow = new Rectangle
            {
                Height = 50,
                Width = 50,
                Tag = "slow",
                Fill = slowImage
            };
            Canvas.SetLeft(newSlow, rand.Next(0, 430));
            Canvas.SetTop(newSlow, (rand.Next(100, 400) * -1));
            myCanvas.Children.Add(newSlow);
        }

        private void SetState(IGameState state)
        {
            currentState = state;
            currentState.Handle(this);
            stateTimer.Stop(); // Stop the state timer
            stateTimer.Start(); // Start the state timer
        }

        private void StateTimerTick(object? sender, EventArgs e)
        {
            stateTimer.Stop();
            SetState(gameStates.normal); // Return to normal state after timer expires
        }
    }
}