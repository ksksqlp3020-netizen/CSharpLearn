// 클래스 만들기
Game lol; new Game("롤",1.01f);   // Game형태를 저장할 수 있고 이름이 lol이다. 공간은 아직 x 
//lol.tilte = "롤";
//lol.version = 1.01f;
lol.Run();              // new [타입], [타입] 메모리 크기만큼 공간을 만들어준다.

Game val = new Game("발로란트",1.11f);
//val.title = "발로란트";
//val.version = 1.11f;
val.Run();


// 클래스 만들기
// 실습 예제. 게임에 버전을 실수형으로 정의를 해서 실행할 때 {title} v{version}을 실행합니다.
// 생성자
class Game
{
    // 속성, 게임이라는 개념이 무엇을 가지고 있어야 하나.
    // 실행한다. 모든 게임이 실행하나요?
    public string title;

    public Game(string title, float version)
    {
        this.title = title;
    }

    
    public Game(string title,)
    
    public void Run()
    {
        Console.WriteLine($"{title}을 v{version}실행합니다.");
    }
}

// 클래스 왜 만들어요?
// 발로란트를 실행합니다.
// 롤을 실행합니다.
// 공통적인 기능과 속성을 가진 개념을 하나의 설계도로 만드는 방법을 클래스라고 합니다.
// 기능 : 함수
// 속성 : 변수

// 클래스 어떻게 써요? - ai야 어떻게 써?

// 클래스 사용하기