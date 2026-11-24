using System;

public static class WaveManager
{
    public static int CurrentWave { get; private set; } = 0;
    public static int AliveMonsters { get; private set; } = 0;

    public static void StartNextWave()
    {
        CurrentWave++;
        int spawnCount = CurrentWave * 3;
        AliveMonsters = spawnCount;
        GameManager.SpawnMonsters(spawnCount);
    }

    public static void OnMonsterKilled()
    {
        if (AliveMonsters > 0)
        {
            AliveMonsters--;
        }

        if (AliveMonsters == 0)
        {
            StartNextWave();
        }
    }

    public static void Reset()
    {
        CurrentWave = 0;
        AliveMonsters = 0;
    }
}
