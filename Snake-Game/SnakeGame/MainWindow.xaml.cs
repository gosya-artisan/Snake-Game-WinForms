using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Security.Cryptography;
namespace SnakeGame
{
    public partial class MainWindow : Window
    {
        private readonly string[] food = new string[]
        {
            "Images/Food/food.png"
        };

        private readonly string[] _body = new string[]
        {
            "Images/Body/body.png"
        };

        private readonly string[] head = new string[]
        {
            "Images/Head/head.png"
        };
        private MediaPlayer _kissSound = new MediaPlayer();
        //private MediaPlayer _backgroundMusik = new MediaPlayer();
        private MediaPlayer _laughSound= new MediaPlayer();

        private bool _isPaused = false;
        private int pause_counter = 0;

        private Direction _currentDirection = Direction.Right;
        private Direction _nextDirection = Direction.Right;
        private const int TimerInterval = 100;
        private DispatcherTimer _timer = new DispatcherTimer();

        private const int SnakeSquareSize = 100;
        private const int FoodSquareSize = 100;
        private const int _snakeSpeed = 25;

        private Image _snakeHead;
        private Point _foodPos;
        private Point _snakeHeadStartPos;

        private int score = 0;
        
        private static readonly Random rand = new Random();

        private List<Image> _snake = new List<Image>();

        public MainWindow()
        {
            //PlayBackgroundMusik();
            InitializeComponent();
            GameCanvas.Loaded += GameCanvas_Loaded;

            this.KeyDown += MainWin_KeyDown;
        }

        private void GameCanvas_Loaded(object sender, RoutedEventArgs e)
        {
            InitialGame();
        }

        private void PlayKissSound()
        {
            _kissSound.Open(new Uri("Sounds/eat.wav"));
            _kissSound.Play();
        }
        // private void PlayBackgroundMusik()
        // {
        //     _backgroundMusik.Open(new Uri("Sounds/backgroundMusik.wav"));
        //     _backgroundMusik.Play();
        // }
        private void PlayLaughSound()
        {
            _laughSound.Open(new Uri("Sounds/laugh.wav"));
            _laughSound.Play();
        }

        private void InitialGame()
        {

            ScoreTextBlock.FontSize = 25;
            RestartButton.FontSize = 25;
            RestartButton.Width = 250;
            RestartButton.Height = 100;

            if (GameCanvas.ActualWidth > 0 && GameCanvas.ActualHeight > 0)
            {
                PlaceSnake();
                PlaceFood();


                _timer.Tick += Timer_Tick;
                _timer.Interval = TimeSpan.FromMilliseconds(TimerInterval);
                _timer.Start();
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            Point newHeadPos = CalculateNewHeadPos();

            if(newHeadPos == _foodPos)
            {
                score += 1;
                EatFood();
                PlaceFood();
            }

            if(newHeadPos.X < 0 ||  newHeadPos.Y < 0
                || newHeadPos.X >= GameCanvas.ActualWidth
                || newHeadPos.Y >= GameCanvas.ActualHeight)
            {
                EndGame();
                return;
            }

            if (_snake.Count >=3)
            {
                for(int i = _snake.Count - 2; i > 0; i--)
                {
                    if (newHeadPos.X == Canvas.GetLeft(_snake[i])
                        && newHeadPos.Y == Canvas.GetTop(_snake[i]))
                        {
                        EndGame();
                        return;
                    }
                }
            }

            for (int i = _snake.Count - 1; i > 0; i--)
            {
                if (newHeadPos.Y <SnakeSquareSize && newHeadPos.X <= SnakeSquareSize)
                {
                    ScoreTextBlock.Foreground = new SolidColorBrush(Colors.Black);
                }
                else
                {
                    ScoreTextBlock.Foreground = new SolidColorBrush(Colors.White);
                }
            }

            for (int i = _snake.Count - 1; i > 0; i--)
            {
                Canvas.SetLeft(_snake[i], Canvas.GetLeft(_snake[i-1]));
                Canvas.SetTop(_snake[i], Canvas.GetTop(_snake[i - 1]));
            }
            Canvas.SetLeft(_snakeHead, newHeadPos.X);
            Canvas.SetTop(_snakeHead, newHeadPos.Y);
        }


        private void EndGame()
        {
            PlayLaughSound();
            //_backgroundMusik.Stop();
            _timer.Stop();
            RestartButton.Visibility = Visibility.Visible;
        }

        private void EatFood()
        {
            try
            {
                PlayKissSound();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка воспроизведения звука: {ex.Message}");
            }
            
            score = _snake.Count;
            ScoreTextBlock.Text = "Score: " + score.ToString();
            if (GameCanvas.Children.Count > 0)
            {
                var lastElement = GameCanvas.Children[GameCanvas.Children.Count - 1] as Image;
                if (lastElement != null)
                {
                    GameCanvas.Children.Remove(lastElement);
                }
            }
            Image newBodyPart = CreateSnakeSegment(_foodPos);
            _snake.Add(newBodyPart);
            GameCanvas.Children.Add(newBodyPart);
        }

        private Point CalculateNewHeadPos()
        {
            _currentDirection = _nextDirection;

            double left = Canvas.GetLeft(_snakeHead);
            double top = Canvas.GetTop(_snakeHead);

            Point newHeadPos = new Point();

            switch (_currentDirection)
            {
                case Direction.Left:
                    newHeadPos = new Point(left - SnakeSquareSize, top);
                    break;
                case Direction.Right:
                    newHeadPos = new Point(left + SnakeSquareSize, top);
                    break;
                case Direction.Up:
                    newHeadPos = new Point(left, top - SnakeSquareSize);
                    break;
                case Direction.Down:
                    newHeadPos = new Point(left, top + SnakeSquareSize);
                    break;
            }
            return newHeadPos;
        }

        private Image CreateSnakeSegment(Point pos)
        {
            Image snakeImg = new Image()
            {
                Width = SnakeSquareSize,
                Height = SnakeSquareSize,
                Source = new BitmapImage(new Uri(_body[rand.Next(_body.Length)], UriKind.Relative)),
                Stretch = Stretch.Fill
            };
            Canvas.SetLeft(snakeImg, pos.X);
            Canvas.SetTop(snakeImg, pos.Y);
            return snakeImg;
        }

        private Image CreateSnakeHeadSegment(Point pos)
        {
            Image snakeHeadImg = new Image()
            {
                Width = SnakeSquareSize,
                Height = SnakeSquareSize,
                Source = new BitmapImage(new Uri(head[rand.Next(head.Length)], UriKind.Relative)),
                Stretch = Stretch.Fill
            };
            Canvas.SetLeft(snakeHeadImg, pos.X);
            Canvas.SetTop(snakeHeadImg, pos.Y);
            return snakeHeadImg;
        }

        private void PlaceSnake()
        {
            int maxX = (int)(GameCanvas.ActualWidth /SnakeSquareSize)/2;
            int maxY = (int)(GameCanvas.ActualHeight /SnakeSquareSize)/2;

            int snakeHeadX = maxX * SnakeSquareSize;
            int snakeHeadY = maxY * SnakeSquareSize;
            _snakeHeadStartPos = new Point(snakeHeadX, snakeHeadY);
            _snakeHead = CreateSnakeHeadSegment(_snakeHeadStartPos);
            GameCanvas.Children.Add(_snakeHead);
            _snake.Add(_snakeHead);
        }

        private void PlaceFood()
        {
            int maxX = (int)(GameCanvas.ActualWidth / FoodSquareSize);
            int maxY = (int)(GameCanvas.ActualHeight / FoodSquareSize);
            Point newFoodPos;
            do
            {
                int foodX = rand.Next(maxX);
                int foodY = rand.Next(maxY);
                newFoodPos = new Point(foodX * FoodSquareSize, foodY * FoodSquareSize);
            } while (newFoodPos == _snakeHeadStartPos);

            _foodPos = newFoodPos;

            Image foodImage = new Image()
            {
                Width = FoodSquareSize,
                Height = FoodSquareSize,
                Source = new BitmapImage(new Uri(food[rand.Next(food.Length)], UriKind.Relative)),
                Stretch = Stretch.Fill
            };

            Canvas.SetLeft(foodImage, _foodPos.X);
            Canvas.SetTop(foodImage, _foodPos.Y);
            GameCanvas.Children.Add(foodImage);
        }

        private void MainWin_KeyDown(object sender, KeyEventArgs e)
        {
            switch(e.Key)
            {
                case Key.Up:
                    if(_currentDirection!=Direction.Down) _nextDirection = Direction.Up; break;
                case Key.Down:
                    if (_currentDirection != Direction.Up) _nextDirection = Direction.Down; break; 
                case Key.Left:
                    if (_currentDirection != Direction.Right) _nextDirection = Direction.Left; break;
                case Key.Right:
                    if (_currentDirection != Direction.Left) _nextDirection = Direction.Right; break;
                case Key.Space:
                    if (_isPaused)
                    {
                        ResumeGame();
                    }
                    else
                    {
                        PauseGame();
                    }
                    break;
            }
        }

        private void PauseGame()
        {
            _isPaused = true;
            _timer.Stop();
        }

        private void ResumeGame()
        {
            _isPaused = false;
            _timer.Start();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            _timer.Stop();
            _timer.Tick -= Timer_Tick;
            _snakeHeadStartPos = new Point(0, 0);
            score = 0;
            ScoreTextBlock.Text = "Score: "+score.ToString();
            RestartButton.Visibility = Visibility.Collapsed;
            GameCanvas.Children.Clear();
            _snake.Clear();
            _currentDirection = Direction.Right;
            _nextDirection = Direction.Right;
            _isPaused = false;
            //_backgroundMusik.Play();
            _laughSound.Stop();

            InitialGame();

        }
        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isPaused)
            {
                ResumeGame();
                PauseButton.Content = "Pause"; // Change button text to "Pause"
            }
            else
            {
                PauseGame();
                PauseButton.Content = "Resume"; // Change button text to "Resume"
            }
        }
    }
}