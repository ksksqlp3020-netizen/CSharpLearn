/*
 * 클래스의 생성자까지 사용 문법 실습한다.
 * 
 * -게임 폴더
 *      -program.cs
 *      -Unit.cs 
 *      
 * ai코드를 하나의 Main 작성을 하면 가독성이 매우 떨어집니다. Ai가 무엇을 만들었는지 1도 알아볼 수가 없다.
 * 이해 가능한 범위까지 코드를 줄이면 됩니다.
 * 1번째로, Main으로만 게임 만들어. -> 공통된 속성과 기능을 찾아서 클래스로 표현하고 그 클래스를 분할시켜라.
 * 
 */

using System;

class Unit
{
    public string Name;
    public int Health;
    public int Attack;
    public int Defense;

    public Unit(string name, int health, int attack, int defense)
    {
        Name = name;
        Health = health;
        Attack = attack;
        Defense = defense;
    }

    public void AttackTarget(Unit defender, out int damage)
    {
        damage = Math.Max(1, this.Attack - defender.Defense);
        defender.Health = Math.Max(0, defender.Health - damage);
    }
}

class Program
{
    static void Main(string[] args)
    {
        Unit player = new Unit("플레이어", 100, 20, 5);
        Unit enemy = new Unit("몬스터", 80, 15, 3);

        while (player.Health > 0 && enemy.Health > 0)
        {
            // 1. 입력
            Console.Write("행동을 선택하세요 (1: 공격, 0: 종료): ");
            string input = Console.ReadLine();

            // 2. 업데이트
            int playerDamage = 0;
            int enemyDamage = 0;
            bool acted = false;

            if (input == "0")
            {
                break;
            }
            else if (input == "1")
            {
                acted = true;
                player.AttackTarget(enemy, out playerDamage);

                if (enemy.Health > 0)
                {
                    enemy.AttackTarget(player, out enemyDamage);
                }
            }

            // 3. 렌더링
            if (acted)
            {
                Console.WriteLine($"{player.Name}가 {playerDamage} 피해를 줍니다. 남은 체력 : {enemy.Health}");

                if (enemy.Health > 0)
                {
                    Console.WriteLine($"{enemy.Name}가 {enemyDamage} 피해를 줍니다. 남은 체력 : {player.Health}");
                }
            }

            Console.WriteLine($"[{player.Name} HP: {player.Health} / {enemy.Name} HP: {enemy.Health}]");
            Console.WriteLine("----------------------------------------");
        }

        if (player.Health <= 0)
        {
            Console.WriteLine("패배했습니다.");
        }
        else if (enemy.Health <= 0)
        {
            Console.WriteLine("승리했습니다!");
        }
    }
}