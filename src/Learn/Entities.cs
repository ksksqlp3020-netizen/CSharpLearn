public class Player
{
    public int X { get; set; }
    public int Y { get; set; }

    public Player(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public class Bullet
{
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsActive { get; set; } = true;

    public Bullet(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public class Monster
{
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsAlive { get; set; } = true;

    public Monster(int x, int y)
    {
        X = x;
        Y = y;
    }
}
