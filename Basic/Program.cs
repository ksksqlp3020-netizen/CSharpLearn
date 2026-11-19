// 2026-11-18
// C# 프로그래밍 입문
// 학습 목표 : 화면에 내가 원하는 글자를 출력하고 싶다.

// 컴퓨터 : 작성한 글자를 어떻게 읽어야 해?
// 입력과 출력
// 내가 어떤 크기에 어떤 물건을 넣어야 할지 딱 맞는 상자를 알려줄게 -> type 형태

// 변수 (variable)
// 정수   int
// 실수   float
// 문자열 string
// 불     bool

// [데이터 + 로직] =>
// 변수
// 로직 : 어떻게 사용을 해야 진짜 게임이 끝난것처럼 표현이 될까요

// 두번째 학습 목표 : 변수를 변형해서 원하는 흐름을 얻기

// 슬라임hp
// 용사hp
// 슬라임atk
// 용사atk
// '전투' : [슬라임의 턴]. 슬라임이 용사를 슬라임의 공격력으로 공격합니다.

namespace Basic
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("모험자 RPG에 오신걸 환영합니다");
            Console.WriteLine("당신의 이름은 무엇인가요?");

            int hp = 100;
            hp = hp - 100; //
            float version = 1.01f;
            bool gmaeClear = false;

            Console.WriteLine(hp);
            Console.WriteLine(version);
            Console.WriteLine(gameClear);

            
            int slimeHP = 100;
            
            int p_hp = 100;

            int s_atk = 1;

            int p_atk = 20;

            Console.WriteLine("[슬라임 턴]");
            p_hp = p_hp - s_atk;
            Console.WriteLine($"용사의 체력은 : {p_hp)다");

            if(gameClear)
            {
                Console.WriteLine("출력이 되었나요?");
            }



        }
    }
}
