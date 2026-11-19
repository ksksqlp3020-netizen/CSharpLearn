using System;

class Program
{
    static void Main()
    {
        int 내점수 = 0;
        int 상대점수 = 0;

        Random rand = new Random();
        string[] 선택지 = { "가위", "바위", "보" };

        Console.Write("나 (가위, 바위, 보 중 입력): ");
        string 나 = Console.ReadLine();

        // 상대(가위, 바위, 보) 자동 선택
        int 인덱스 = rand.Next(0, 3);
        string 상대 = 선택지[인덱스];
        Console.WriteLine($"상대: {상대}");

        // 3x3 조건문 (이긴 사람은 점수를 1점 얻는다)
        // 1. 나 : 가위일 때
        if (나 == "가위" && 상대 == "가위")
        {
            Console.WriteLine("결과: 무승부");
        }
        else if (나 == "가위" && 상대 == "바위")
        {
            Console.WriteLine("결과: 상대 승리");
            상대점수 += 1;
        }
        else if (나 == "가위" && 상대 == "보")
        {
            Console.WriteLine("결과: 나 승리");
            내점수 += 1;
        }
        // 2. 나 : 바위일 때
        else if (나 == "바위" && 상대 == "가위")
        {
            Console.WriteLine("결과: 나 승리");
            내점수 += 1;
        }
        else if (나 == "바위" && 상대 == "바위")
        {
            Console.WriteLine("결과: 무승부");
        }
        else if (나 == "바위" && 상대 == "보")
        {
            Console.WriteLine("결과: 상대 승리");
            상대점수 += 1;
        }
        // 3. 나 : 보일 때
        else if (나 == "보" && 상대 == "가위")
        {
            Console.WriteLine("결과: 상대 승리");
            상대점수 += 1;
        }
        else if (나 == "보" && 상대 == "바위")
        {
            Console.WriteLine("결과: 나 승리");
            내점수 += 1;
        }
        else if (나 == "보" && 상대 == "보")
        {
            Console.WriteLine("결과: 무승부");
        }

        // 점수판 출력
        Console.WriteLine($"[점수판] 나: {내점수}점 | 상대: {상대점수}점");
    }
}
