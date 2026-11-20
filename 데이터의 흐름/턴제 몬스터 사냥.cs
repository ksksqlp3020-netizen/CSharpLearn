using System;

class Program
{
    static void Main()
    {
        Random rand = new Random();
        int 내체력 = 100;
        int 몬스터체력 = 80;

        while (내체력 > 0 && 몬스터체력 > 0)
        {
            Console.WriteLine($"\n[상태] 내 HP: {내체력} | 몬스터 HP: {몬스터체력}");
            Console.Write("행동 선택 (1. 공격 / 2. 회복): ");
            string 선택 = Console.ReadLine();

            if (선택 == "1")
            {
                int 데미지 = rand.Next(15, 26); // 15~25 데미지
                몬스터체력 -= 데미지;
                Console.WriteLine($"몬스터에게 {데미지}의 피해를 입혔습니다!");
            }
            else if (선택 == "2")
            {
                내체력 += 20;
                Console.WriteLine("체력을 20 회복했습니다!");
            }

            if (몬스터체력 <= 0)
            {
                Console.WriteLine("몬스터를 처치했습니다! 승리!");
                break;
            }

            // 몬스터 반격
            int 몬스터공격 = rand.Next(10, 21);
            내체력 -= 몬스터공격;
            Console.WriteLine($"몬스터의 반격! {몬스터공격}의 피해를 입었습니다.");

            if (내체력 <= 0)
            {
                Console.WriteLine("쓰러졌습니다... 패배!");
            }
        }
    }
}