using GameData;


//
// 1. 입력
// 2. 렌더링 (Rendering)
// 3. Update
class Program
{
    static void Main(string[] args)
    {
        MonsterType monsterType = MonsterType.Fire; //[cite: 1]
        int monsterHp = 100; //[cite: 1]
        Tile currentTile = Tile.Earth; // 1번째 타일: 땅[cite: 1]

        for (int step = 1; step <= 4; step++) //[cite: 1]
        {
            // 1. 입력
            Console.Write($"[{step}단계] 이동하려면 Enter를 입력하세요: ");
            Console.ReadLine();

            // 2. 렌더링 (Rendering)
            Console.WriteLine($"몬스터 생성 완료! (속성: {monsterType}, 체력: {monsterHp})"); //[cite: 1]
            Console.WriteLine("------------------------------------------"); //[cite: 1]
            Console.WriteLine($"[{step}번째 이동] 현재 밟은 타일: {currentTile}"); //[cite: 2]

            // 3. Update
            // 단계별로 타일 변경 (땅 -> 불 -> 물 -> 불)[cite: 1, 2]
            if (step == 1) currentTile = Tile.Earth; //[cite: 1]
            else if (step == 2) currentTile = Tile.Fire; //[cite: 2]
            else if (step == 3) currentTile = Tile.Water; //[cite: 2]
            else if (step == 4) currentTile = Tile.Fire; //[cite: 2]

            // 몬스터가 불타입일 때 타일별 효과 처리[cite: 2]
            if (monsterType == MonsterType.Fire) //[cite: 2]
            {
                switch (currentTile) //[cite: 2]
                {
                    case Tile.Earth:
                        break;
                    case Tile.Fire:
                        monsterHp += 10;
                        break;
                    case Tile.Water:
                        monsterHp -= 20;
                        break;
                }
            }
        }
    }
}

