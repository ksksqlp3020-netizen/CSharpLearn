using System;
using System.Collections.Generic;
using System.Threading;

namespace ConsoleTetris
{
    // 모든 로직, 블록 클래스(블록, MorphBlock, LineClearBlock), 입력/갱신/렌더링을 하나의 메인 클래스 안에 통합 구현
    public class TetrisGame
    {
        // ==========================================
        // 1. 블록 클래스 정의 (상속 및 특수 기능 구현)
        // ==========================================
        public class 블록
        {
            public string shape = "■";
            public ConsoleColor color = ConsoleColor.White;

            public 블록(string shape, ConsoleColor color = ConsoleColor.White)
            {
                this.shape = shape;
                this.color = color;
            }
        }

        // 설치 시 확률에 따라 다른 블록으로 변신하고, 변신 누적 10회마다 특수 블록을 해금하는 블록
        public class MorphBlock : 블록
        {
            public const int MorphThreshold = 10;           // 특수 블록 해금 기준 변신 횟수 (10회)
            public static double currentMorphChance = 0.10; // 초기 변신 확률 10%
            public static int totalMorphCount = 0;          // 누적 변신 횟수
            public static bool pendingClearBlock = false;   // 10회 달성 시 다음 스폰 예약 플래그

            public MorphBlock(string shape, ConsoleColor color = ConsoleColor.White)
                : base(shape, color)
            {
            }

            // 설치 시 현재 확률로 변신 판정:
            // - 변신 성공 시: 다른 모양으로 바뀌고 확률이 10%로 초기화, 변신 횟수 +1
            // - 변신 실패 시: 다음 설치를 위해 확률 +1% 증가
            public int[,] TryMorph(int[,] currentShape, int[][,] allShapes, Random rng, out int newColorIndex, out bool morphed)
            {
                double roll = rng.NextDouble();
                if (roll < currentMorphChance)
                {
                    morphed = true;
                    int nextIdx = rng.Next(allShapes.Length);
                    newColorIndex = nextIdx;
                    this.shape = "★"; // 변신한 블록 표시

                    // 확률 10%로 초기화
                    currentMorphChance = 0.10;

                    // 변신 횟수 누적 및 10번 바뀔 때마다 3줄 클리어 특수 블록 예약
                    totalMorphCount++;
                    if (totalMorphCount % MorphThreshold == 0)
                    {
                        pendingClearBlock = true;
                    }

                    return (int[,])allShapes[nextIdx].Clone();
                }

                morphed = false;
                newColorIndex = -1;

                // 변신하지 않았으므로 다음 설치를 위해 확률 1% 증가 (최대 100%)
                currentMorphChance = Math.Min(1.0, Math.Round(currentMorphChance + 0.01, 2));
                return currentShape;
            }
        }

        // 블록이 10번 바뀔 때마다 등장하는 특수 블록: 착지(설치) 시 아래 3줄을 즉시 클리어!
        public class LineClearBlock : 블록
        {
            public int clearLinesCount = 3;

            public LineClearBlock(string shape = "◈", ConsoleColor color = ConsoleColor.White)
                : base(shape, color)
            {
            }
        }

        // ==========================================
        // 2. 게임 상태 및 보드 설정
        // ==========================================
        private const int Width = 10;
        private const int Height = 20;

        private static 블록[,] board = new 블록[Height, Width];

        // 7가지 기본 테트로미노 모양 정의
        private static readonly int[][,] Tetrominoes = new int[][,]
        {
            new int[,] { {1, 1, 1, 1} },                         // I (0)
            new int[,] { {1, 1}, {1, 1} },                       // O (1)
            new int[,] { {0, 1, 0}, {1, 1, 1} },                 // T (2)
            new int[,] { {0, 1, 1}, {1, 1, 0} },                 // S (3)
            new int[,] { {1, 1, 0}, {0, 1, 1} },                 // Z (4)
            new int[,] { {1, 0, 0}, {1, 1, 1} },                 // J (5)
            new int[,] { {0, 0, 1}, {1, 1, 1} }                  // L (6)
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

        // 다음에 나올 블록 대기열 (항상 3개 이상 유지)
        private static Queue<int> nextQueue = new Queue<int>();

        // 현재 조종 중인 블록 정보
        private static int[,] currentShape;
        private static 블록 currentBlockType;
        private static int currentX;
        private static int currentY;

        private static Random rng = new Random();
        private static bool isGameOver = false;
        private static int score = 0;
        private static string statusMessage = $"변신 {MorphBlock.MorphThreshold}회 달성 시 3줄을 삭제하는 [◈] 특수 블록이 내려옵니다!";

        // 프레임 및 낙하 시간 제어 변수
        private static DateTime lastFrameTime;
        private static double fallTimer = 0.0;
        private static double fallInterval = 0.6;

        // ==========================================
        // 3. 메인 게임 루프 (입력 -> 갱신 -> 렌더링)
        // ==========================================
        public static void Main(string[] args)
        {
            Console.CursorVisible = false;
            Console.Title = "C# Console Tetris (Next 3 + 10-Morph 3-Line Clear)";

            MorphBlock.currentMorphChance = 0.10;
            MorphBlock.totalMorphCount = 0;
            MorphBlock.pendingClearBlock = false;

            // 다음 블록 3개 미리 채우기
            nextQueue.Clear();
            for (int i = 0; i < 3; i++)
            {
                nextQueue.Enqueue(rng.Next(Tetrominoes.Length));
            }

            SpawnPiece();
            lastFrameTime = DateTime.Now;

            while (!isGameOver)
            {
                DateTime now = DateTime.Now;
                double deltaTime = (now - lastFrameTime).TotalSeconds;
                lastFrameTime = now;

                // 1) 입력 (Input)
                HandleInput();

                // 2) 갱신 (Update)
                Update(deltaTime);

                // 3) 렌더링 (Render)
                Render();

                // CPU 과부하 방지 (~30 FPS)
                Thread.Sleep(33);
            }

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("=========================");
            Console.WriteLine("       GAME OVER!        ");
            Console.WriteLine($"     최종 점수: {score}     ");
            Console.WriteLine($"   총 블록 변신: {MorphBlock.totalMorphCount}회   ");
            Console.WriteLine("=========================");
            Console.ResetColor();
            Console.WriteLine("아무 키나 누르면 종료합니다...");
            Console.ReadKey(true);
        }

        // ==========================================
        // 4. 입력 처리 (Input)
        // ==========================================
        private static void HandleInput()
        {
            while (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                switch (keyInfo.Key)
                {
                    case ConsoleKey.LeftArrow:
                    case ConsoleKey.A:
                        if (IsValidPosition(currentShape, currentX - 1, currentY))
                            currentX--;
                        break;

                    case ConsoleKey.RightArrow:
                    case ConsoleKey.D:
                        if (IsValidPosition(currentShape, currentX + 1, currentY))
                            currentX++;
                        break;

                    case ConsoleKey.DownArrow:
                    case ConsoleKey.S:
                        if (IsValidPosition(currentShape, currentX, currentY + 1))
                        {
                            currentY++;
                            fallTimer = 0;
                        }
                        break;

                    case ConsoleKey.UpArrow:
                    case ConsoleKey.W:
                        int[,] rotated = RotateMatrix(currentShape);
                        if (IsValidPosition(rotated, currentX, currentY))
                            currentShape = rotated;
                        break;

                    case ConsoleKey.Spacebar:
                        // 하드 드롭 (고스트 블록 위치까지 즉시 낙하)
                        currentY = GetGhostY();
                        LockPiece();
                        ClearLines();
                        SpawnPiece();
                        fallTimer = 0;
                        break;

                    case ConsoleKey.Escape:
                        isGameOver = true;
                        break;
                }
            }
        }

        // ==========================================
        // 5. 게임 상태 갱신 (Update)
        // ==========================================
        private static void Update(double deltaTime)
        {
            fallTimer += deltaTime;
            if (fallTimer >= fallInterval)
            {
                fallTimer = 0;
                if (IsValidPosition(currentShape, currentX, currentY + 1))
                {
                    currentY++;
                }
                else
                {
                    LockPiece();
                    ClearLines();
                    SpawnPiece();
                }
            }
        }

        // ==========================================
        // 6. 화면 출력 (Render) - 보드 + 우측 NEXT 3개 미리보기
        // ==========================================
        private static void Render()
        {
            Console.SetCursorPosition(0, 0);

            int percent = (int)Math.Round(MorphBlock.currentMorphChance * 100);
            int ghostY = GetGhostY();
            int morphProgress = MorphBlock.totalMorphCount % MorphBlock.MorphThreshold;

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("=== C# CONSOLE TETRIS ===");
            Console.ResetColor();
            Console.WriteLine($"점수: {score,-8} | 변신 확률: {percent,3}% (변신 시 10% 초기화)");
            Console.WriteLine($"누적 변신: {MorphBlock.totalMorphCount}회 ({morphProgress}/{MorphBlock.MorphThreshold} -> 3줄 폭파 블록 등장!)");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"{statusMessage,-68}");
            Console.ResetColor();
            Console.WriteLine();

            int[] nextItems = nextQueue.ToArray();
            bool specialNext = MorphBlock.pendingClearBlock;

            // 상단 테두리 + NEXT 헤더
            Console.Write("┌");
            for (int c = 0; c < Width; c++) Console.Write("──");
            Console.Write("┐   ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("┌── NEXT 3 ──┐");
            Console.ResetColor();

            for (int r = 0; r < Height; r++)
            {
                // 왼쪽: 메인 게임 보드
                Console.Write("│");
                for (int c = 0; c < Width; c++)
                {
                    블록 activeCell = GetPieceCellAt(currentShape, currentX, currentY, c, r, currentBlockType);

                    if (activeCell != null)
                    {
                        Console.ForegroundColor = activeCell.color;
                        Console.Write(activeCell.shape);
                        Console.ResetColor();
                    }
                    else if (IsPieceCellAt(currentShape, currentX, ghostY, c, r))
                    {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.Write("□");
                        Console.ResetColor();
                    }
                    else if (board[r, c] != null)
                    {
                        블록 cell = board[r, c];
                        Console.ForegroundColor = cell.color;
                        Console.Write(cell.shape);
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write("  ");
                    }
                }
                Console.Write("│   ");

                // 오른쪽: NEXT 3개 블록 미리보기 패널
                RenderNextPanelRow(r, nextItems, specialNext);
            }

            // 하단 테두리
            Console.Write("└");
            for (int c = 0; c < Width; c++) Console.Write("──");
            Console.WriteLine("┘");
            Console.WriteLine("조작: ←/→ 이동 | ↑ 회전 | ↓ 소프트드롭 | Space 하드드롭");
        }

        private static void RenderNextPanelRow(int r, int[] nextItems, bool specialNext)
        {
            if (r == 14)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("└────────────┘");
                Console.ResetColor();
                return;
            }
            if (r > 14)
            {
                Console.WriteLine("              ");
                return;
            }

            int slotIndex = r / 5;
            int localRow = r % 5;

            if (localRow == 4)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("│────────────│");
                Console.ResetColor();
                return;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("│");
            Console.ResetColor();

            if (localRow == 0)
            {
                if (slotIndex == 0 && specialNext)
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write(" 1.[3줄폭파] ");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write($" {slotIndex + 1}번째 블록 ");
                }
                Console.ResetColor();
            }
            else
            {
                int shapeRow = localRow - 1;
                if (slotIndex == 0 && specialNext)
                {
                    if (shapeRow == 0 || shapeRow == 1)
                    {
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("    ◈◈      ");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write("            ");
                    }
                }
                else
                {
                    int queueIdx = specialNext ? slotIndex - 1 : slotIndex;
                    if (queueIdx >= 0 && queueIdx < nextItems.Length)
                    {
                        int pieceId = nextItems[queueIdx];
                        int[,] previewShape = Tetrominoes[pieceId];
                        ConsoleColor previewColor = TetrominoColors[pieceId];

                        Console.Write("  ");
                        for (int c = 0; c < 4; c++)
                        {
                            if (shapeRow < previewShape.GetLength(0) && c < previewShape.GetLength(1) && previewShape[shapeRow, c] == 1)
                            {
                                Console.ForegroundColor = previewColor;
                                Console.Write("■");
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.Write("  ");
                            }
                        }
                        Console.Write("  ");
                    }
                    else
                    {
                        Console.Write("            ");
                    }
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("│");
            Console.ResetColor();
        }

        // ==========================================
        // 7. 보조 로직 (스폰, 고스트, 변신 설치, 3줄 클리어, 줄 삭제)
        // ==========================================
        private static void SpawnPiece()
        {
            if (MorphBlock.pendingClearBlock)
            {
                MorphBlock.pendingClearBlock = false;
                currentShape = new int[,] { { 1, 1 }, { 1, 1 } };
                currentX = Width / 2 - 1;
                currentY = 0;
                currentBlockType = new LineClearBlock("◈", ConsoleColor.White);
                statusMessage = $"★ {MorphBlock.MorphThreshold}회 변신 달성! 착지 시 아래 3줄을 완전히 지우는 [◈] 특수 블록 등장!";
            }
            else
            {
                int idx = nextQueue.Dequeue();
                nextQueue.Enqueue(rng.Next(Tetrominoes.Length));

                currentShape = (int[,])Tetrominoes[idx].Clone();
                currentX = Width / 2 - currentShape.GetLength(1) / 2;
                currentY = 0;
                currentBlockType = new MorphBlock("■", TetrominoColors[idx]);
            }

            if (!IsValidPosition(currentShape, currentX, currentY))
            {
                isGameOver = true;
            }
        }

        private static int GetGhostY()
        {
            int y = currentY;
            while (IsValidPosition(currentShape, currentX, y + 1))
            {
                y++;
            }
            return y;
        }

        private static bool IsValidPosition(int[,] shape, int x, int y)
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

                    if (boardX < 0 || boardX >= Width || boardY < 0 || boardY >= Height)
                        return false;

                    if (board[boardY, boardX] != null)
                        return false;
                }
            }
            return true;
        }

        private static void LockPiece()
        {
            if (currentBlockType is LineClearBlock clearBlock)
            {
                int targetStartRow = Math.Min(Height - 1, currentY + currentShape.GetLength(0) - 1);
                ClearThreeRowsFrom(targetStartRow, clearBlock.clearLinesCount);
                score += 300;
                statusMessage = "◈ 특수 블록 발동! 3줄을 완전히 클리어했습니다! (+300점)";
                return;
            }

            if (currentBlockType is MorphBlock morphBlock)
            {
                int appliedPercent = (int)Math.Round(MorphBlock.currentMorphChance * 100);
                int newColorIdx;
                bool morphed;
                int[,] finalShape = morphBlock.TryMorph(currentShape, Tetrominoes, rng, out newColorIndex, out morphed);
                int nextPercent = (int)Math.Round(MorphBlock.currentMorphChance * 100);

                if (morphed)
                {
                    morphBlock.color = TetrominoColors[newColorIdx];

                    if (MorphBlock.pendingClearBlock)
                    {
                        statusMessage = $"★ 변신 발동 ({MorphBlock.totalMorphCount}회 달성)! 다음 블록으로 [3줄 폭파 블록]이 내려옵니다!";
                    }
                    else
                    {
                        statusMessage = $"★ 변신 발동! ({appliedPercent}% 성공, 누적 {MorphBlock.totalMorphCount}회) -> 확률 10% 초기화!";
                    }

                    int newCols = finalShape.GetLength(1);
                    if (currentX + newCols > Width)
                        currentX = Width - newCols;
                    if (currentX < 0)
                        currentX = 0;

                    while (currentY > 0 && !IsValidPosition(finalShape, currentX, currentY))
                    {
                        currentY--;
                    }
                }
                else
                {
                    statusMessage = $"일반 설치 ({appliedPercent}% 미발동) -> 다음 확률 {nextPercent}% (+1%)";
                }

                int rows = finalShape.GetLength(0);
                int cols = finalShape.GetLength(1);

                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < cols; c++)
                    {
                        if (finalShape[r, c] == 0) continue;

                        int boardX = currentX + c;
                        int boardY = currentY + r;

                        if (boardY >= 0 && boardY < Height && boardX >= 0 && boardX < Width)
                        {
                            board[boardY, boardX] = new 블록(morphBlock.shape, morphBlock.color);
                        }
                    }
                }
            }
        }

        private static void ClearThreeRowsFrom(int anchorRow, int count)
        {
            int startRow = anchorRow;
            int endRow = startRow + count - 1;

            if (endRow >= Height)
            {
                endRow = Height - 1;
                startRow = Math.Max(0, endRow - count + 1);
            }

            int actualCleared = endRow - startRow + 1;

            for (int r = endRow; r >= actualCleared; r--)
            {
                for (int c = 0; c < Width; c++)
                {
                    board[r, c] = board[r - actualCleared, c];
                }
            }

            for (int r = 0; r < actualCleared; r++)
            {
                for (int c = 0; c < Width; c++)
                {
                    board[r, c] = null;
                }
            }
        }

        private static void ClearLines()
        {
            int linesCleared = 0;

            for (int r = Height - 1; r >= 0; r--)
            {
                bool isFull = true;
                for (int c = 0; c < Width; c++)
                {
                    if (board[r, c] == null)
                    {
                        isFull = false;
                        break;
                    }
                }

                if (isFull)
                {
                    linesCleared++;
                    for (int moveRow = r; moveRow > 0; moveRow--)
                    {
                        for (int c = 0; c < Width; c++)
                        {
                            board[moveRow, c] = board[moveRow - 1, c];
                        }
                    }
                    for (int c = 0; c < Width; c++)
                    {
                        board[0, c] = null;
                    }
                    r++;
                }
            }

            if (linesCleared > 0)
            {
                score += linesCleared * 100;
            }
        }

        private static int[,] RotateMatrix(int[,] matrix)
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

        private static bool IsPieceCellAt(int[,] shape, int pieceX, int pieceY, int x, int y)
        {
            if (shape == null) return false;

            int relX = x - pieceX;
            int relY = y - pieceY;

            if (relY >= 0 && relY < shape.GetLength(0) &&
                relX >= 0 && relX < shape.GetLength(1))
            {
                return shape[relY, relX] == 1;
            }
            return false;
        }

        private static 블록 GetPieceCellAt(int[,] shape, int pieceX, int pieceY, int x, int y, 블록 blockRef)
        {
            return IsPieceCellAt(shape, pieceX, pieceY, x, y) ? blockRef : null;
        }
    }
}