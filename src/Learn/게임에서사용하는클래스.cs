// AI를 활용하여 배운 문법을 작성하는 페이지 입니다.

// 클래스. 생성자.
// 클래스로 무엇을 할래?

// RPG 턴제. 전투.
// '플레이어'(똑같은 존재) '적' (도감에 있는 모든 몬스터)
// Unit - 전투를 한다. 플레이어도 하고 상대방도 똑같은 방식으로 한다

using System.Runtime.CompilerServices;

public class Unit
{
    // 속성

    // 체력
    public int hp;
    // 공격력
    public int atk;
    // 방어력
    public int def;
    // 속도
    public int speed;

    public Unit(int hp, int atk, int def, int speed)
    {
        this.hp = hp;
        this.atk = atk;
        this.def = def;
        this.speed = speed;
    }

    // 기능  
    // 체력과 공격력과 방어력과 속도를 사용해서 전투하는 기능
    public void Battle(Unit target)
    {
        int damage = atk - target.def;
        if (damage < 1)
        {
            damage = 1;
        }

        target.hp -= damage;
    }





        // 개별 공격 처리
private void AttackTarget(Unit defender)
    {
        // 공격력에서 방어력을 차감하여 최소 1의 데미지 보장
        int damage = Math.Max(1, this.atk - defender.def);
        defender.hp = Math.Max(0, defender.hp - damage);

        // $"{}"

        Console.WriteLine($"공격자가{damage} 피해를 줍니다. 남은 체력 : {defender.hp}");
    }
}
