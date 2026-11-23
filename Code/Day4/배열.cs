// 배열

// 왜 쓸까요?
// 반복문. 갯수가 정해졌을 때 쓰면 'for'

// 어떻게 배열을 상요하면 n개를 하나의 코드로 표현할 수 있을까요?
// 배열의 구현 원리를 알면 설명할 수 있다. -> 자료구조.

int[] numbers = new int[10];
for(int i =0; i < 10; i++)
{
    numbers[i] = i;
    Console.WriteLine($"현재 값은 : {numbers[i]}");

}

// 배열은 같은 자료형을 한번에 표현할 수 있다.
// 갯수를 정하면 시작부터 끝까지 데이터를 저장하고 언제든지 꺼내서 쓸 수 있다.

// 가격은 가격인데... (xx)가격이 너무 많은데? price 마음껏 써도 되나?

const int ITEM_MAX_COUNT = 10;

int[] numbers = new int[ITEM_MAX_COUNT];
string[] itemNames = new string[ITEM_MAX_COUNT]
{
            "낡은 단검",
            "가죽 갑옷",
            "작은 회복 물약",
            "마나 정수",
            "나무 방패",
            "철 검",
            "모험가 망토",
            "해독제",
            "강철 투구",
            "행운의 부적"
};
int[] prices = new int[ITEM_MAX_COUNT];

for (int i = 0; i < ITEM_MAX_COUNT; i++)
{
    numbers[i] = i + 1;
    prices[i] = 10 + 10 * i;
}

Console.WriteLine("+------+------------------+--------+");
Console.WriteLine("| 번호 | 아이템 이름      | 가격   |");
Console.WriteLine("+------+------------------+--------+");

for (int i = 0; i < ITEM_MAX_COUNT; i++)
{
    Console.WriteLine($"| {numbers[i],-4} | {itemNames[i],-14} | {prices[i],4} G |");
}

Console.WriteLine("+------+------------------+--------+");
}

// 4번째 등급의 아이템 가격은 얼마인가요?
Console.WriteLine("4번째 아이템 가격은 얼마인가요?");
Console.WriteLine($" {prices[3]} 입니다.");
