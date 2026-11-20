// 반복문
// for 반복문
// while 반복문
// 왜 써야하는가?

using System.Runtime.InteropServices;

for (int dan = 2; dan <= 9; dan++)
{
    Console.WriteLine($"\n=== {dan}단 ===");
    for (int num = 1; num <= 9; num++)
    {
        Console.WriteLine($"{dan} × {num} = {dan * num}");
    }
}

// while 반복문 
// 왜 써야하는가? 무한 반복문 표현. 무한히 반복할 때 표현.
bool gamefinish = false;
int hp = 0;
while(true)
{
    // 게임이 종료되는 조건!
    if (hp <= 0) gamefinish = true;
    if(gamefinish)
    {
        break;
    }
}

// 게임은 항상 끊임없이 실행을 해야한다. 

// 게임이다.
// 마우스를 한번 왼클릭 한다.
// 화면에서 망치가 움직이고
// 확률에 따라서 성공할 수 도 있고 안할 수 도 있다.
// 강화게임

