using System;
using System.Threading;

namespace GameEngineDemo
{
    public abstract class BaseGameEngine
    {
        abstract public void Input();
        abstract public void Update();
        abstract public void Rendering();
    }

    // =========================================================================
    // MyGameEngine을 수정하지 않고 별도로 상속받아 구현한 테트리스 게임
    // =========================================================================
    public class TetrisFromEngine : MyGameEngine
    {
        private const int BoardWidth = 10;
        private const int BoardHeight = 20;

        private readonly ConsoleColor?[,] _board = new ConsoleColor?[BoardHeight, BoardWidth];

        private static readonly int[][,] Tetrominoes = new int[][,]
        {
            new int[,] { {1, 1, 1, 1} },                         // I
            new int[,] { {1, 1}, {1, 1} },                       // O
            new int[,] { {0, 1, 0}, {1, 1, 1} },                 // T
            new int[,] { {0, 1, 1}, {1, 1, 0} },                 // S
            new int[,] { {1, 1, 0}, {0, 1, 1} },                 // Z
            new int[,] { {1, 0, 0}, {1, 1, 1} },                 // J
            new int[,] { {0, 0, 1}, {1, 1, 1} }                  // L
        };

        private static readonly ConsoleColor[] TetrominoColors = new ConsoleColor[]
        {
            ConsoleColor.Cyan,
            ConsoleColor.Yellow,
            ConsoleColor.Magenta,
            ConsoleColor.Green,
            ConsoleColor.Red,
            ConsoleColor.Blue,
            ConsoleColor.DarkYellow
        };

        private int[,] _currentShape;
        private ConsoleColor _currentColor;
        private int _currentX;
        private int _currentY;

        private readonly Random _rng = new Random();
        private bool _isGameOver = false;
        private int _score = 0;

        private DateTime _lastTime;
        private double _fallTimer = 0.0;
        private const double FallInterval = 0.5;

        public TetrisFromEngine() : base(BoardWidth + 2, BoardHeight + 2)
        {
            SpawnPiece();
            _lastTime = DateTime.Now;
            UpdateHudText();
        }

        protected override void OnEngineStopped()
        {
            Console.WriteLine(_isGameOver ? $"GAME OVER! 최종 점수: {_score}" : $"게임 종료! 최종 점수: {_score}");
        }

        public override void Input()
        {
            while (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.A:
                        if (IsValidPosition(_currentShape, _currentX - 1, _currentY))
                            _currentX--;
                        break;

                    case ConsoleKey.RightArrow:
                    case ConsoleKey.D:
                        if (IsValidPosition(_currentShape, _currentX + 1, _currentY))
                            _currentX++;
                        break;

                    case ConsoleKey.DownArrow:
                    case ConsoleKey.S:
                        if (IsValidPosition(_currentShape, _currentX, _currentY + 1))
                        {
                            _currentY++;
                            _fallTimer = 0;
                        }
                        break;

                    case ConsoleKey.UpArrow:
                    case ConsoleKey.W:
                        int[,] rotated = RotateMatrix(_currentShape);
                        if (IsValidPosition(rotated, _currentX, _currentY))
                            _currentShape = rotated;
                        break;

                    case ConsoleKey.Spacebar:
                        _currentY = GetGhostY();
                        LockPiece();
                        ClearLines();
                        SpawnPiece();
                        _fallTimer = 0;
                        break;

                    case ConsoleKey.Escape:
                        IsRunning = false;
                        break;
                }
            }
        }

        public override void Update()
        {
            DateTime now = DateTime.Now;
            double deltaTime = (now - _lastTime).TotalSeconds;
            _lastTime = now;

            _fallTimer += deltaTime;
            if (_fallTimer >= FallInterval)
            {
                _fallTimer = 0;
                if (IsValidPosition(_currentShape, _currentX, _currentY + 1))
                {
                    _currentY++;
                }
                else
                {
                    LockPiece();
                    ClearLines();
                    SpawnPiece();
                }
            }

            UpdateHudText();
            ComposeFrameToEngineCanvas();
        }

        private void UpdateHudText()
        {
            SetHud($"TETRIS | 점수: {_score,-6} | 이동:←/→/↓ 회전:↑ 드롭:Space 종료:ESC", ConsoleColor.Cyan);
        }

        private void ComposeFrameToEngineCanvas()
        {
            ClearCanvas(' ', ConsoleColor.Gray);

            // 외곽 벽 그리기
            for (int x = 0; x < EngineWidth; x++)
            {
                DrawPixel(x, 0, '#', ConsoleColor.DarkGray);
                DrawPixel(x, EngineHeight - 1, '#', ConsoleColor.DarkGray);
            }
            for (int y = 0; y < EngineHeight; y++)
            {
                DrawPixel(0, y, '#', ConsoleColor.DarkGray);
                DrawPixel(EngineWidth - 1, y, '#', ConsoleColor.DarkGray);
            }

            // 고정된 블록 그리기
            for (int r = 0; r < BoardHeight; r++)
            {
                for (int c = 0; c < BoardWidth; c++)
                {
                    if (_board[r, c].HasValue)
                    {
                        DrawPixel(c + 1, r + 1, '@', _board[r, c].Value);
                    }
                }
            }

            // 착지 예상 위치(고스트 블록) 그리기
            int ghostY = GetGhostY();
            DrawPiece(_currentShape, _currentX, ghostY, '.', ConsoleColor.DarkGray);

            // 현재 떨어지는 블록 그리기
            DrawPiece(_currentShape, _currentX, _currentY, '@', _currentColor);
        }

        private void DrawPiece(int[,] shape, int pieceX, int pieceY, char symbol, ConsoleColor color)
        {
            if (shape == null) return;
            int rows = shape.GetLength(0);
            int cols = shape.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (shape[r, c] == 1)
                    {
                        DrawPixel(pieceX + c + 1, pieceY + r + 1, symbol, color);
                    }
                }
            }
        }

        private void SpawnPiece()
        {
            int idx = _rng.Next(Tetrominoes.Length);
            _currentShape = (int[,])Tetrominoes[idx].Clone();
            _currentColor = TetrominoColors[idx];
            _currentX = BoardWidth / 2 - _currentShape.GetLength(1) / 2;
            _currentY = 0;

            if (!IsValidPosition(_currentShape, _currentX, _currentY))
            {
                _isGameOver = true;
                IsRunning = false;
            }
        }

        private int GetGhostY()
        {
            int y = _currentY;
            while (IsValidPosition(_currentShape, _currentX, y + 1))
            {
                y++;
            }
            return y;
        }

        private bool IsValidPosition(int[,] shape, int x, int y)
        {
            int rows = shape.GetLength(0);
            int cols = shape.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (shape[r, c] == 0) continue;

                    int boardX = x + c;
                    int boardY = y + r;

                    if (boardX < 0 || boardX >= BoardWidth || boardY < 0 || boardY >= BoardHeight)
                        return false;

                    if (_board[boardY, boardX].HasValue)
                        return false;
                }
            }
            return true;
        }

        private void LockPiece()
        {
            int rows = _currentShape.GetLength(0);
            int cols = _currentShape.GetLength(1);

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    if (_currentShape[r, c] == 1)
                    {
                        int boardX = _currentX + c;
                        int boardY = _currentY + r;
                        if (boardY >= 0 && boardY < BoardHeight && boardX >= 0 && boardX < BoardWidth)
                        {
                            _board[boardY, boardX] = _currentColor;
                        }
                    }
                }
            }
        }

        private void ClearLines()
        {
            int cleared = 0;
            for (int r = BoardHeight - 1; r >= 0; r--)
            {
                bool full = true;
                for (int c = 0; c < BoardWidth; c++)
                {
                    if (!_board[r, c].HasValue)
                    {
                        full = false;
                        break;
                    }
                }

                if (full)
                {
                    cleared++;
                    for (int moveRow = r; moveRow > 0; moveRow--)
                    {
                        for (int c = 0; c < BoardWidth; c++)
                        {
                            _board[moveRow, c] = _board[moveRow - 1, c];
                        }
                    }
                    for (int c = 0; c < BoardWidth; c++)
                    {
                        _board[0, c] = null;
                    }
                    r++;
                }
            }

            if (cleared > 0)
            {
                _score += cleared * 100;
            }
        }

        private int[,] RotateMatrix(int[,] matrix)
        {
            int rows = matrix.GetLength(0);
            int cols = matrix.GetLength(1);
            int[,] rotated = new int[cols, rows];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    rotated[c, rows - 1 - r] = matrix[r, c];
                }
            }
            return rotated;
        }

        public static void Main()
        {
            TetrisFromEngine game = new TetrisFromEngine();
            game.Run();
        }
    }
}