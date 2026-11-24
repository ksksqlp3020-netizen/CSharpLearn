using System;
using System.Threading;

Console.CursorVisible = false;

GameManager.Initialize(width: 40, height: 20);

while (GameManager.IsRunning)
{
    GameManager.ProcessInput();
    GameManager.Update();
    GameManager.Render();

    Thread.Sleep(50);
}

Console.Clear();
Console.CursorVisible = true;
Console.WriteLine("게임 종료!");
