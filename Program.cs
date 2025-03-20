// This sample is to help solve problems.
// TEST: SNAKE


// All connected to GitHub, except for Lin

using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;

new Snake.Game().Start();

namespace Snake
{
    public class Game
    {
        private readonly Snake snake = new Snake();
        private readonly Fruit fruit = new Fruit();
        private readonly Canvas canvas = new Canvas();
        private readonly Score score = new Score();
        private readonly Gamer gamer = new Gamer();

        private DateTime fruitTime = DateTime.Now;  // store the fruit time 

        private Point preFruit = new Point();   // store the location of previous fruit

        public bool GameOver { get; set; } = false;

        public Game() => Initialize();

        public void Initialize()
        {
            canvas.Draw();
            snake.Initialize(canvas);
            fruit.Spawn(canvas, snake, score);
            GameOver = false;
            score.Initialize();
        }

        public void Start()
        {
            while (!GameOver)
            {
                if (Console.KeyAvailable)
                {
                    var keyInfo = Console.ReadKey(true);
                    snake.Direction = Utility.ConvertDirectionFromKey(keyInfo, snake.Direction);
                }
                // Snake touch itself --> End game
                if (snake.Tail.Contains(snake.Head))
                {
                    EndGame();
                    return;
                }
                // Snake touch the fruit --> Score increase
                else if (snake.Head == fruit.Location)
                {
                    preFruit = fruit.Location;
                    snake.Move(canvas, true, preFruit);
                    score.Current += fruit.Value;

                    // Fill that empty
                    preFruit = snake.Tail[0];
                    Utility.Write("f", preFruit.X, preFruit.Y, Settings.Snake.TailForeground, Settings.Snake.TailBackground);
                   
                    fruit.Spawn(canvas, snake, score);
                    fruitTime = DateTime.Now;   // Refresh the fruit time

                    Console.Title = $"Score: {score.Current}. Tail: {snake.Tail.Length}";   // Title shows the score
                }
                // Snake touch  the fruit --> End game
                else if (snake.ReachEdge == true)
                {
                    EndGame();
                    return;
                }
                else
                {
                    snake.Move(canvas, false, preFruit);
                }

                // The fruit's location will change
                if (score.Current > 10 && DateTime.Now - fruitTime > TimeSpan.FromSeconds(9 - fruit.Value))
                {
                    fruit.Spawn(canvas, snake, score);
                    fruitTime = DateTime.Now;
                }               

                // Control the fresh rate
                Thread.Sleep(Settings.HeartBeatMilliseconds);
            }
        }

        private void EndGame()
        {
            GameOver = true;

            if (gamer.AskRestart())
            {
                Initialize();
                Start();
            }
        }
    }

    public class Gamer
    {
        public string Email { get; set; } = default!;

        public string Name { get; set; } = default!;

        public bool TryLookup(string email, out string name)
        {
            throw new NotImplementedException();
        }

        public bool AskName()
        {
            throw new NotImplementedException();
        }

        public bool AskRestart()
        {
            Utility.Write("Game Over! Do you want to restart? (Y/N)", 0, 0, ConsoleColor.Red, ConsoleColor.White);
            Utility.WaitFor(out var key, ConsoleKey.Y, ConsoleKey.N);
            return key == ConsoleKey.Y;
        }
    }

    public class Score
    {
        public int Current { get; set; } = 0;

        public int HighScore { get; set; } = 0;

        public int[] HighScores { get; set; } = Array.Empty<int>();

        public void Initialize()
        {
            Current = 0;
        }

        public void GetHighScores(Gamer gamer)
        {
            throw new NotImplementedException();
        }

        public void SaveCurrent(Gamer gamer)
        {
            throw new NotImplementedException();
        }
    }

    public class Canvas
    {
        private Rectangle Border { get; set; }

        public Rectangle Inner => new(Border.X + 1, Border.Y - 1, Border.Width - 2, Border.Height - 2);

        public void Erase(Point point)
        {
            Utility.Write(Settings.Canvas.FillChar.ToString(), point.X, point.Y, null, Settings.Canvas.CanvasBackground);
        }

        public void Draw()
        {
            Console.Clear();

            FormatConsole();

            Border = Settings.Canvas.Size;

            string topRow = Settings.Canvas.TopLeftChar + new string(Settings.Canvas.HorizontalChar, Border.Width - 2) + Settings.Canvas.TopRightChar;
            string middleRows = new string(Settings.Canvas.FillChar, Border.Width - 2);
            string bottomRow = Settings.Canvas.BottomLeftChar + new string(Settings.Canvas.HorizontalChar, Border.Width - 2) + Settings.Canvas.BottomRightChar;

            // Write the top row
            Utility.Write(topRow, Border.Left, Border.Top, Settings.Canvas.BorderForeground, Settings.Canvas.BorderBackground);

            // Write the middle rows
            for (int y = Border.Top + 1; y < Border.Top + Border.Height - 1; y++)
            {
                Utility.Write(Settings.Canvas.VerticalChar.ToString(), Border.Left, y, Settings.Canvas.BorderForeground, Settings.Canvas.BorderBackground);
                Utility.Write(middleRows, Border.Left + 1, y, null, Settings.Canvas.CanvasBackground);
                Utility.Write(Settings.Canvas.VerticalChar.ToString(), Border.Left + Border.Width - 1, y, Settings.Canvas.BorderForeground, Settings.Canvas.BorderBackground);
            }

            // Write the bottom row
            Utility.Write(bottomRow, Border.Left, Border.Top + Border.Height - 1, Settings.Canvas.BorderForeground, Settings.Canvas.BorderBackground);

            void FormatConsole()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Console.SetWindowSize(Settings.Canvas.Size.Right, Settings.Canvas.Size.Bottom);
                    Console.SetBufferSize(Settings.Canvas.Size.Right, Settings.Canvas.Size.Bottom);
                }
                Console.CursorVisible = false;
                Console.Clear();
            }
        }
    }

    public class Snake
    {
        public Point Head { get; private set; }
        public Point[] Tail { get; private set; } = Array.Empty<Point>();
        public Point[] Points => Tail.Prepend(Head).ToArray();
        public bool ReachEdge { get; set; } = false;    // the flag that snake touch the edge
        public Settings.Direction Direction { get; set; } = Settings.Direction.Default;

     
        public void Initialize(Canvas canvas)
        {
            // Erase all points if the user re-start the game
            foreach (var point in Points)
            {
                canvas.Erase(point);
            }

            Tail = Array.Empty<Point>();
            Direction = Settings.Direction.Default;
            Head = Utility.GetRandomSafePoint(canvas);
            DrawHead();
            ReachEdge = false;
        }

        public void Move(Canvas canvas, bool increase, Point preFruit)
        {
            if (Direction == Settings.Direction.Default)
            {
                return;
            }

            var prevHead = Head;
            Head = Utility.MovePoint(Head, Direction);
            Head = Utility.WrapPoint(Head, Direction, canvas);


            // touch the edge, game over
            if (!Utility.IsPointWithinCanvas(Head, canvas.Inner))
            {
                ReachEdge = true;
                return;
            }

            bool isFruitLocation = Head == preFruit;

            if (Tail.Length == 0 && !increase)
            {
                DrawHead();
                // Solve the problem of empty display
                if (!isFruitLocation)
                {
                    canvas.Erase(prevHead); // If the snake doesn't on the original fruit location
                }                           // Draw the moving as usual
                else
                {
                    DrawTail(prevHead); // fill the empty location
                }
            }
            else if (!increase)
            {
                DrawHead();
                DrawTail(prevHead);
                canvas.Erase(Tail[^1]);
                Tail = Tail.Prepend(prevHead).ToArray()[..^1];
            }
            else if (increase)
            {
                DrawHead();
                DrawTail(prevHead);
                Tail = Tail.Prepend(prevHead).ToArray();
            }
        }

        private void DrawTail(params Point[] points)
        {
            foreach (var point in points)
            {
                Utility.Write(Settings.Snake.TailChar.ToString(), point.X, point.Y, Settings.Snake.TailForeground, Settings.Snake.TailBackground);
            }
        }

        private void DrawHead()
        {
            var headSymbol = Utility.ConvertDirectionToChar(Direction).ToString();
            Utility.Write(headSymbol, Head.X, Head.Y, Settings.Snake.HeadForeground, Settings.Snake.HeadBackground);
        }
    }

    public class Fruit
    {
        public Point Location { get; set; }
        public int Value { get; set; }

        public void Spawn(Canvas canvas, Snake snake, Score score)
        {
            canvas.Erase(Location);
            Location = Utility.GetRandomSafePoint(canvas, snake.Points);

            if (score.Current <= 10)
            {
                Value = Random.Shared.Next(1, 10);
            }
            else if (score.Current > 10)
            {
                Value = Random.Shared.Next(2, 5);
            }
            Utility.Write(Value.ToString(), Location.X, Location.Y, Settings.Fruit.Foreground, Settings.Fruit.Background);
        }

    }

    public static class Settings
    {
        public enum Direction { Left, Right, Up, Down, Default }
        public static readonly int HeartBeatMilliseconds = 100;     // Fresh rate

        // public static readonly string DatabaseConnectionString = Environment.GetEnvironmentVariable("SnakeDatabaseConnectionString") ?? throw new Exception("SnakeDatabaseConnectionString environment variable is missing.");

        public struct Canvas
        {
            public static readonly Rectangle Size = new(5, 5, 40, 20);
            public static readonly char HorizontalChar = '─';
            public static readonly char VerticalChar = '│';
            public static readonly char TopLeftChar = '┌';
            public static readonly char TopRightChar = '┐';
            public static readonly char BottomLeftChar = '└';
            public static readonly char BottomRightChar = '┘';
            public static readonly char FillChar = ' ';

            public static readonly ConsoleColor BorderBackground = ConsoleColor.White;
            public static readonly ConsoleColor BorderForeground = ConsoleColor.Blue;
            public static readonly ConsoleColor CanvasBackground = ConsoleColor.Gray;
        }

        public struct Snake
        {
            public static readonly char HeadRightChar = '▶'; // not all will render ▶
            public static readonly char HeadLeftChar = '◀'; // not all will render ◀
            public static readonly char HeadUpChar = '▲';
            public static readonly char HeadDownChar = '▼';
            public static readonly char HeadStopChar = '■';
            public static readonly char TailChar = ' ';

            public static readonly ConsoleColor HeadBackground = ConsoleColor.Gray;
            public static readonly ConsoleColor HeadForeground = ConsoleColor.Blue;
            public static readonly ConsoleColor TailBackground = ConsoleColor.Blue;
            public static readonly ConsoleColor TailForeground = ConsoleColor.Gray;
        }

        public struct Fruit
        {
            public static readonly ConsoleColor Background = ConsoleColor.White;
            public static readonly ConsoleColor Foreground = ConsoleColor.Blue;
        }
    }

    static class Utility
    {
        public static Point GetRandomSafePoint(Canvas canvas, params Point[] avoid)
        {
            Point point;
            do
            {
                var x = Random.Shared.Next(canvas.Inner.Left, canvas.Inner.Right - 1);    // Have to -1 +2 +1
                var y = Random.Shared.Next(canvas.Inner.Top + 2, canvas.Inner.Bottom + 1);  // Or the point would not safe
                point = new Point(x, y);

            } while (avoid.Contains(point));
            return point;
        }

        public static Point WrapPoint(Point point, Settings.Direction direction, Canvas canvas)
        {
            /*
            return (point.X, point.Y) switch
            {
                var (x, y) when direction == Settings.Direction.Up && y < canvas.Inner.Top => new(x, canvas.Inner.Bottom - 1),
                var (x, y) when direction == Settings.Direction.Down && y >= canvas.Inner.Bottom => new(x, canvas.Inner.Top),
                var (x, y) when direction == Settings.Direction.Left && x <= canvas.Inner.Left => new(canvas.Inner.Right, y),
                var (x, y) when direction == Settings.Direction.Right && x > canvas.Inner.Right => new(canvas.Inner.Left + 1, y),
                var (x, y) => point
            };
            */
            return point;
        }

        public static Point MovePoint(Point point, Settings.Direction direction) => direction switch
        {
            Settings.Direction.Up => new Point(point.X, point.Y - 1),
            Settings.Direction.Right => new Point(point.X + 1, point.Y),
            Settings.Direction.Down => new Point(point.X, point.Y + 1),
            Settings.Direction.Left => new Point(point.X - 1, point.Y),
            _ => point
        };

        public static void Write(string text, int x, int y, ConsoleColor? foreground = null, ConsoleColor? background = null)
        {
            Console.SetCursorPosition(left: x, top: y);
            Console.ForegroundColor = foreground ?? Console.ForegroundColor;
            Console.BackgroundColor = background ?? Console.BackgroundColor;
            Console.Write(text);
            Console.ResetColor();
        }

        public static char ConvertDirectionToChar(Settings.Direction direction)
        {
            return direction switch
            {
                Settings.Direction.Up => Settings.Snake.HeadUpChar,
                Settings.Direction.Right => Settings.Snake.HeadRightChar,
                Settings.Direction.Down => Settings.Snake.HeadDownChar,
                Settings.Direction.Left => Settings.Snake.HeadLeftChar,
                _ => Settings.Snake.HeadStopChar
            };
        }

        public static Settings.Direction ConvertDirectionFromKey(ConsoleKeyInfo keyInfo, Settings.Direction currentDirection)
        {
            // The key to control the direction
            var desiredDirection = keyInfo.Key switch
            {
                ConsoleKey.W => Settings.Direction.Up,  // WSAD key
                ConsoleKey.S => Settings.Direction.Down,
                ConsoleKey.A => Settings.Direction.Left,
                ConsoleKey.D => Settings.Direction.Right,

                ConsoleKey.LeftArrow => Settings.Direction.Left, // Arrow key
                ConsoleKey.RightArrow => Settings.Direction.Right,
                ConsoleKey.UpArrow => Settings.Direction.Up,
                ConsoleKey.DownArrow => Settings.Direction.Down,
                _ => currentDirection
            };

            if (ReverseDirection(currentDirection, desiredDirection))
            {
                return currentDirection;
            }
            else
            {
                return desiredDirection;
            }

            bool ReverseDirection(Settings.Direction currentDirection, Settings.Direction desiredDirection)
            {
                return (currentDirection == Settings.Direction.Up && desiredDirection == Settings.Direction.Down) ||
                       (currentDirection == Settings.Direction.Down && desiredDirection == Settings.Direction.Up) ||
                       (currentDirection == Settings.Direction.Left && desiredDirection == Settings.Direction.Right) ||
                       (currentDirection == Settings.Direction.Right && desiredDirection == Settings.Direction.Left);
            }
        }

        public static void WaitFor(out ConsoleKey key, params ConsoleKey[] allowed)
        {
            while (!allowed.Contains(key = Console.ReadKey(true).Key))
            {
                // Wait for one of the specified keys to be pressed
            }
        }

        public static bool IsPointWithinCanvas(Point point, Rectangle canvas)
        {
            //Console.Error.WriteLine($"Canvas: {canvas.Left}, {canvas.Top}, {canvas.Right}, {canvas.Bottom}");
            //Console.Error.WriteLine($"Snake's Head: {point.X}, {point.Y}");


            return point.X >= canvas.Left && point.X < canvas.Right &&
                   point.Y >= canvas.Top + 2 && point.Y <= canvas.Bottom + 1;
        }

        /*public static void PauseGame(out ConsoleKey key, params ConsoleKey[] allowed)
        {
            
        }*/


    }
}
