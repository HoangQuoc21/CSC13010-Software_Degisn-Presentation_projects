using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace Car_Racing_Game
{
    public partial class MainWindow : Window
    {
        DispatcherTimer gameTimer = new DispatcherTimer();
        DispatcherTimer stateTimer = new DispatcherTimer();
        List<Rectangle> itemRemover = new List<Rectangle>();

        Random rand = new Random();

        ImageBrush playerImage = new ImageBrush();
        ImageBrush fastImage = new ImageBrush();
        ImageBrush slowImage = new ImageBrush();

        Rect playerHitBox;

        public int baseSpeed = 10;
        public int speed;
        public string state;

        public int playerSpeed = 10;

        int objectCounter = 100;

        bool moveLeft, moveRight, gameOver;
        IGameState currentState;
        readonly IGameState normalState;
        readonly IGameState fastState;
        readonly IGameState slowState;

        public MainWindow()
        {
            InitializeComponent();
            myCanvas.Focus();
            gameTimer.Tick += GameLoop;
            gameTimer.Interval = TimeSpan.FromMilliseconds(20);
            stateTimer.Tick += StateTimerTick;
            stateTimer.Interval = TimeSpan.FromSeconds(5);
            normalState = new NormalState();
            fastState = new FastState();
            slowState = new SlowState();
            currentState = normalState;
            StartGame();
        }

        private void GameLoop(object? sender, EventArgs e)
        {
            objectCounter -= 1;
            stateText.Content = $"Current State: {state}";
            speedText.Content = $"Speed: {speed} km/h";
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
                    Canvas.SetTop(x, Canvas.GetTop(x) + speed);
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
                            SetState(fastState);
                        }
                        else if ((string)x.Tag == "slow")
                        {
                            SetState(slowState);
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

            AdjustSpeedBasedOnScore();
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
            if (e.Key == Key.Enter && gameOver)
            {
                StartGame();
            }
        }

        private void StartGame()
        {
            speed = baseSpeed;
            state = "Normal";
            gameTimer.Start();
            moveLeft = false;
            moveRight = false;
            gameOver = false;

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
            stateTimer.Stop();
            stateTimer.Start(); // Start the state timer
        }

        private void StateTimerTick(object? sender, EventArgs e)
        {
            stateTimer.Stop();
            SetState(normalState); // Return to normal state after timer expires
        }

        private void AdjustSpeedBasedOnScore()
        {
            if (currentState is NormalState)
            {
                speed = baseSpeed;
            }
        }
    }

    public interface IGameState
    {
        void Handle(MainWindow game);
    }

    public class NormalState : IGameState
    {
        public void Handle(MainWindow game)
        {
            game.speed = game.baseSpeed; // Reset to base speed
            game.state = "Normal";
        }
    }

    public class FastState : IGameState
    {
        public void Handle(MainWindow game)
        {
            game.speed = game.baseSpeed * 2; // Double the speed
            game.state = "Fast";
        }
    }

    public class SlowState : IGameState
    {
        public void Handle(MainWindow game)
        {
            game.speed = game.baseSpeed / 2; // Halve the speed
            game.state = "Slow";
        }
    }
}