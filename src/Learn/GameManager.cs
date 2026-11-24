using System;
using System.Collections.Generic;
using System.Text;

public static class GameManager
{
    public static bool IsRunning { get; private set; } = true;

    private static int _width;
    private static int _height;
    private static Player _player = null!;
    private static readonly List<Bullet> _bullets = new();
    private static readonly List<Monster> _monsters = new();
    private static readonly Random _random = new();

    public static void Initialize(int width, int height)
    {
        _width = width;
        _height = height;
        _player = new Player(width / 2, height - 2);
        _bullets.Clear();
        _monsters.Clear();

        WaveManager.Reset();
        WaveManager.StartNextWave();
    }

    public static void SpawnMonsters(int count)
    {
        _monsters.Clear();
        for (int i = 0; i < count; i++)
        {
            int x = _random.Next(1, _width - 1);
            int y = _random.Next(1, 5);
            _monsters.Add(new Monster(x, y));
        }
    }

    public static void ProcessInput()
    {
        while (Console.KeyAvailable)
        {
            ConsoleKey key = Console.ReadKey(intercept: true).Key;

            switch (key)
            {
                case ConsoleKey.W:
                    if (_player.Y > 1) _player.Y--;
                    break;
                case ConsoleKey.S:
                    if (_player.Y < _height - 2) _player.Y++;
                    break;
                case ConsoleKey.A:
                    if (_player.X > 1) _player.X--;
                    break;
                case ConsoleKey.D:
                    if (_player.X < _width - 2) _player.X++;
                    break;
                case ConsoleKey.Spacebar:
                    _bullets.Add(new Bullet(_player.X, _player.Y - 1));
                    break;
                case ConsoleKey.Escape:
                    IsRunning = false;
                    break;
            }
        }
    }

    public static void Update()
    {
        for (int i = _bullets.Count - 1; i >= 0; i--)
        {
            Bullet bullet = _bullets[i];
            bullet.Y--;

            if (bullet.Y < 1)
            {
                _bullets.RemoveAt(i);
                continue;
            }

            for (int j = 0; j < _monsters.Count; j++)
            {
                Monster monster = _monsters[j];
                if (monster.IsAlive && monster.X == bullet.X && monster.Y == bullet.Y)
                {
                    monster.IsAlive = false;
                    bullet.IsActive = false;
                    WaveManager.OnMonsterKilled();
                    break;
                }
            }

            if (!bullet.IsActive)
            {
                _bullets.RemoveAt(i);
            }
        }
    }

    public static void Render()
    {
        char[,] screen = new char[_height, _width];

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                bool isBorder = y == 0 || y == _height - 1 || x == 0 || x == _width - 1;
                screen[y, x] = isBorder ? '#' : ' ';
            }
        }

        foreach (Monster monster in _monsters)
        {
            if (monster.IsAlive)
            {
                screen[monster.Y, monster.X] = 'M';
            }
        }

        foreach (Bullet bullet in _bullets)
        {
            if (bullet.IsActive)
            {
                screen[bullet.Y, bullet.X] = '|';
            }
        }

        screen[_player.Y, _player.X] = 'A';

        StringBuilder sb = new();
        sb.AppendLine($"Wave: {WaveManager.CurrentWave} | 남은 몬스터: {WaveManager.AliveMonsters} | 이동: WASD | 발사: Space | 종료: ESC");

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                sb.Append(screen[y, x]);
            }
            sb.AppendLine();
        }

        Console.SetCursorPosition(0, 0);
        Console.Write(sb.ToString());
    }
}
