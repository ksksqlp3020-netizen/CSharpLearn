// 아이템은 어떤 종류가 있을까요?
// 갯수는 몇개가 있을까요?
// 이름 콘솔에서 어떻게 보여지게 할것인가?

// '조건문, 반복문, 배열, 열거형'
// 아이템 10개, 10개를 어떻게 표현할까? 배열의 선언
// 아이템 1개씩 데이터를 넣어야 한다. 배열의 초기화
// 반복적으로 코드를 그려야한다.
// 입력 - 업데이트 - 렌더링 방식으로 상점 기능이 실행이 되게 만들어주세요.

// 어떤 입력을 해서 무엇이 변경해야하고 그것을 어떻게 보여줘야 한다.
// 숫자를 1~10까지를 클릭하고 예약되어있는 명령이 실행된다
// 예약되어있는 내용
// [1] 아이템 구매
// [2] 상점 나가기


using System;

enum ItemType
{
    무기,
    방어구,
    소비품,
    기타
}

enum ShopCommand
{
    아이템구매 = 1,
    상점나가기 = 2
}

class Program
{
    static void Main(string[] args)
    {
        const int ITEM_MAX_COUNT = 10;

        // 1. 배열 선언 및 초기화 (아이템 10개)
        int[] itemNumbers = new int[ITEM_MAX_COUNT];
        string[] itemNames = new string[ITEM_MAX_COUNT];
        ItemType[] itemTypes = new ItemType[ITEM_MAX_COUNT];
        int[] itemPrices = new int[ITEM_MAX_COUNT];
        bool[] isPurchased = new bool[ITEM_MAX_COUNT];

        itemNames[0] = "낡은 단검"; itemTypes[0] = ItemType.무기; itemPrices[0] = 10;
        itemNames[1] = "가죽 갑옷"; itemTypes[1] = ItemType.방어구; itemPrices[1] = 20;
        itemNames[2] = "작은 회복 물약"; itemTypes[2] = ItemType.소비품; itemPrices[2] = 30;
        itemNames[3] = "마나 정수"; itemTypes[3] = ItemType.소비품; itemPrices[3] = 40;
        itemNames[4] = "나무 방패"; itemTypes[4] = ItemType.방어구; itemPrices[4] = 50;
        itemNames[5] = "철 검"; itemTypes[5] = ItemType.무기; itemPrices[5] = 60;
        itemNames[6] = "모험가 망토"; itemTypes[6] = ItemType.방어구; itemPrices[6] = 70;
        itemNames[7] = "해독제"; itemTypes[7] = ItemType.소비품; itemPrices[7] = 80;
        itemNames[8] = "강철 투구"; itemTypes[8] = ItemType.방어구; itemPrices[8] = 90;
        itemNames[9] = "행운의 부적"; itemTypes[9] = ItemType.기타; itemPrices[9] = 100;

        for (int i = 0; i < ITEM_MAX_COUNT; i++)
        {
            itemNumbers[i] = i + 1;
            isPurchased[i] = false;
        }

        // 상점 상태 변수
        int playerGold = 200;
        bool isRunning = true;
        string statusMessage = "상점에 오신 것을 환영합니다! 명령을 선택하세요.";

        // 입력 상태 변수
        int selectedCommand = 0;
        int selectedItemIndex = -1; // 0 ~ 9

        // 콘솔 한글 폭 보정 로컬 함수
        int GetDisplayWidth(string text)
        {
            int width = 0;
            foreach (char c in text)
            {
                if ((c >= 0xAC00 && c <= 0xD7A3) ||
                    (c >= 0x3131 && c <= 0x318E) ||
                    (c >= 0x1100 && c <= 0x11FF) ||
                    (c >= 0xFF01 && c <= 0xFF60))
                {
                    width += 2;
                }
                else
                {
                    width += 1;
                }
            }
            return width;
        }

        string PadRightVisual(string text, int totalWidth)
        {
            int padding = totalWidth - GetDisplayWidth(text);
            return padding > 0 ? text + new string(' ', padding) : text;
        }

        string PadLeftVisual(string text, int totalWidth)
        {
            int padding = totalWidth - GetDisplayWidth(text);
            return padding > 0 ? new string(' ', padding) + text : text;
        }

        // [입력 - 업데이트 - 렌더링] 메인 루프
        while (isRunning)
        {
            // ================= 렌더링 (RENDER) =================
            Console.Clear();

            const int colNo = 4;
            const int colName = 16;
            const int colType = 8;
            const int colPrice = 8;
            const int colStatus = 8;

            string border = $"+{new string('-', colNo + 2)}+{new string('-', colName + 2)}+{new string('-', colType + 2)}+{new string('-', colPrice + 2)}+{new string('-', colStatus + 2)}+";

            Console.WriteLine($"보유 골드: {playerGold} G");
            Console.WriteLine(border);
            Console.WriteLine($"| {PadRightVisual("번호", colNo)} | {PadRightVisual("아이템 이름", colName)} | {PadRightVisual("종류", colType)} | {PadRightVisual("가격", colPrice)} | {PadRightVisual("상태", colStatus)} |");
            Console.WriteLine(border);

            for (int i = 0; i < ITEM_MAX_COUNT; i++)
            {
                string statusText = isPurchased[i] ? "구매완료" : "판매중";
                Console.WriteLine(
                    $"| {PadRightVisual(itemNumbers[i].ToString(), colNo)} " +
                    $"| {PadRightVisual(itemNames[i], colName)} " +
                    $"| {PadRightVisual(itemTypes[i].ToString(), colType)} " +
                    $"| {PadLeftVisual($"{itemPrices[i]} G", colPrice)} " +
                    $"| {PadRightVisual(statusText, colStatus)} |"
                );
            }

            Console.WriteLine(border);
            Console.WriteLine($"[알림] {statusMessage}");
            Console.WriteLine();
            Console.WriteLine($"[{(int)ShopCommand.아이템구매}] {ShopCommand.아이템구매}   [{(int)ShopCommand.상점나가기}] {ShopCommand.상점나가기}");

            // ================= 입력 (INPUT) =================
            Console.Write("명령 번호 입력: ");
            string cmdInput = Console.ReadLine();

            if (!int.TryParse(cmdInput, out selectedCommand))
            {
                selectedCommand = 0;
                selectedItemIndex = -1;
            }
            else if (selectedCommand == (int)ShopCommand.아이템구매)
            {
                Console.Write("구매할 아이템 번호(1~10) 입력: ");
                string itemInput = Console.ReadLine();

                if (int.TryParse(itemInput, out int itemNo) && itemNo >= 1 && itemNo <= ITEM_MAX_COUNT)
                {
                    selectedItemIndex = itemNo - 1;
                }
                else
                {
                    selectedItemIndex = -2; // 잘못된 아이템 번호 표시
                }
            }
            else
            {
                selectedItemIndex = -1;
            }

            // ================= 업데이트 (UPDATE) =================
            switch ((ShopCommand)selectedCommand)
            {
                case ShopCommand.아이템구매:
                    if (selectedItemIndex == -2)
                    {
                        statusMessage = "1부터 10 사이의 아이템 번호를 입력해 주세요.";
                    }
                    else if (selectedItemIndex >= 0 && selectedItemIndex < ITEM_MAX_COUNT)
                    {
                        if (isPurchased[selectedItemIndex])
                        {
                            statusMessage = $"'{itemNames[selectedItemIndex]}'은(는) 이미 구매한 아이템입니다.";
                        }
                        else if (playerGold < itemPrices[selectedItemIndex])
                        {
                            statusMessage = $"골드가 부족합니다! '{itemNames[selectedItemIndex]}' 가격: {itemPrices[selectedItemIndex]} G";
                        }
                        else
                        {
                            playerGold -= itemPrices[selectedItemIndex];
                            isPurchased[selectedItemIndex] = true;
                            statusMessage = $"'{itemNames[selectedItemIndex]}'({itemTypes[selectedItemIndex]})을(를) {itemPrices[selectedItemIndex]} G에 구매했습니다!";
                        }
                    }
                    break;

                case ShopCommand.상점나가기:
                    statusMessage = "상점을 나갑니다. 이용해 주셔서 감사합니다!";
                    isRunning = false;
                    break;

                default:
                    statusMessage = "잘못된 명령입니다. [1] 아이템 구매 또는 [2] 상점 나가기를 입력하세요.";
                    break;
            }
        }

        Console.Clear();
        Console.WriteLine(statusMessage);
        Console.WriteLine($"최종 보유 골드: {playerGold} G");
    }
}