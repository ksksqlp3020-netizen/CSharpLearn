using System.Text;

namespace GameEngineDemo
{
    // =========================================================================
    // 순수 엔진 코드: 2칸 밀림 방지 및 깜빡임 제거(더블 버퍼링) 담당
    // 상속받는 게임 클래스에서 테트리스 로직만 구현하도록 수정하지 않고 유지
    // =========================================================================
    public class MyGameEngine : BaseGameEngine
    {
        protected int EngineWidth { get; }
        protected int EngineHeight { get; }
        protected int BoardOffsetY { get; set; } = 2;
        protected bool IsRunning { get; set; } = true;

        private readonly char[,] _frontChar;
        private readonly ConsoleColor[,] _frontColor;
        private readonly char[,] _backChar;
        private readonly ConsoleColor[,] _backColor;

        private string _hudText = "";
        private ConsoleColor _hudColor = ConsoleColor.White;

        public MyGameEngine(int width, int height)
        {
            EngineWidth = width;
            EngineHeight = height;

            _frontChar = new char[EngineHeight, EngineWidth];
            _frontColor = new ConsoleColor[EngineHeight, EngineWidth];
            _backChar = new char[EngineHeight, EngineWidth];
            _backColor = new ConsoleColor[EngineHeight, EngineWidth];

            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;

            try
            {
                if (OperatingSystem.IsWindows())
                {
                    int targetCols = Math.Max(Console.WindowWidth, EngineWidth * 2 + 6);
                    int targetRows = Math.Max(Console.WindowHeight, EngineHeight + BoardOffsetY + 6);
                    Console.SetWindowSize(targetCols, targetRows);
                    Console.SetBufferSize(targetCols, targetRows);
                }
            }
            catch
            {
                // 일부 콘솔 환경에서 창/버퍼 크기 변경이 제한된 경우 무시
            }

            Console.Clear();
            ResetBuffers();
        }

        public void Run()
        {
            while (IsRunning)
            {
                Input();
                Update();
                Rendering();
                Thread.Sleep(33);
            }

            Console.ResetColor();
            Console.CursorVisible = true;
            Console.SetCursorPosition(0, EngineHeight + BoardOffsetY + 2);
            OnEngineStopped();
        }

        protected virtual void OnEngineStopped()
        {
        }

        protected void SetHud(string text, ConsoleColor color = ConsoleColor.White)
        {
            _hudText = text;
            _hudColor = color;
        }

        protected void ClearCanvas(char fillChar = ' ', ConsoleColor fillColor = ConsoleColor.Gray)
        {
            for (int y = 0; y < EngineHeight; y++)
            {
                for (int x = 0; x < EngineWidth; x++)
                {
                    _backChar[y, x] = fillChar;
                    _backColor[y, x] = fillColor;
                }
            }
        }

        protected void DrawPixel(int x, int y, char ch, ConsoleColor color)
        {
            if (x < 0 || x >= EngineWidth || y < 0 || y >= EngineHeight) return;
            _backChar[y, x] = ch;
            _backColor[y, x] = color;
        }

        private void ResetBuffers()
        {
            for (int y = 0; y < EngineHeight; y++)
            {
                for (int x = 0; x < EngineWidth; x++)
                {
                    _frontChar[y, x] = '\0';
                    _frontColor[y, x] = ConsoleColor.Gray;
                    _backChar[y, x] = ' ';
                    _backColor[y, x] = ConsoleColor.Gray;
                }
            }
        }

        override public void Input()
        {
        }

        override public void Update()
        {
        }

        override public void Rendering()
        {
            // 상단 HUD 갱신
            Console.SetCursorPosition(0, 0);
            Console.ForegroundColor = _hudColor;
            Console.Write(_hudText.PadRight(Math.Max(1, Console.WindowWidth - 1)));

            // 백버퍼와 프론트버퍼 비교 후 변경된 셀만 출력
            // - Console.Clear() 미사용으로 깜빡임 제거
            // - 단일 폭 문자 + (x * 2) 좌표 명시로 2칸씩 밀리는 현상 방지
            for (int y = 0; y < EngineHeight; y++)
            {
                for (int x = 0; x < EngineWidth; x++)
                {
                    char nextChar = _backChar[y, x];
                    ConsoleColor nextColor = _backColor[y, x];

                    if (_frontChar[y, x] != nextChar || _frontColor[y, x] != nextColor)
                    {
                        Console.SetCursorPosition(x * 2, y + BoardOffsetY);
                        Console.ForegroundColor = nextColor;
                        Console.Write(nextChar);
                        Console.Write(' ');

                        _frontChar[y, x] = nextChar;
                        _frontColor[y, x] = nextColor;
                    }
                }
            }

            Console.ResetColor();
        }
    }
}