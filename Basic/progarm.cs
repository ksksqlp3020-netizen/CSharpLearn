using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarCraftPolymorphismGame
{
    // 1. 공통 열거형 (지상/공중, 생체/기계)
    public enum MovementType { Ground, Air }
    public enum UnitAttribute { Biological, Mechanical }

    // 2. 인터페이스 정의 (기능별 모듈화 및 다형성)
    public interface IClockable
    {
        bool IsCloaked { get; }
        void ToggleCloak();
    }

    public interface IDetector
    {
        int DetectionRange { get; }
        void Scan(IEnumerable<Unit> units);
    }

    public interface ITransport
    {
        int Capacity { get; }
        List<Unit> Passengers { get; }
        void Load(Unit unit);
        void UnloadAll();
    }

    public interface IWorker
    {
        int CarryingMinerals { get; }
        void GatherMineral();
        void BuildStructure(string structureName);
    }

    public interface IBurrowable
    {
        bool IsBurrowed { get; }
        void ToggleBurrow();
    }

    // 3. 최상위 추상 클래스: 유닛
    public abstract class Unit
    {
        public string Name { get; protected set; }
        public int Hp { get; protected set; }
        public int MaxHp { get; protected set; }
        public MovementType MoveType { get; protected set; }
        public UnitAttribute Attribute { get; protected set; }
        public int X { get; protected set; }
        public int Y { get; protected set; }
        public string LastActionLog { get; protected set; } = "대기 중";

        protected Unit(string name, int maxHp, MovementType moveType, UnitAttribute attribute, int x = 0, int y = 0)
        {
            Name = name;
            MaxHp = maxHp;
            Hp = maxHp;
            MoveType = moveType;
            Attribute = attribute;
            X = x;
            Y = y;
        }

        // 이동 공통 기능 (잠복 시 이동 불가 등 오버라이드 가능)
        public virtual void Move(int dx, int dy)
        {
            X += dx;
            Y += dy;
            LastActionLog = $"({X}, {Y}) 위치로 이동";
        }

        // 데미지 처리 (프로토스 등 쉴드 처리를 위해 virtual 선언)
        public virtual void TakeDamage(int amount)
        {
            Hp = Math.Max(0, Hp - amount);
            LastActionLog = $"{amount} 피해를 입음 (남은 체력: {Hp}/{MaxHp})";
        }

        // 매 턴(프레임) 갱신 시 종족별 고유 패시브 발동
        public abstract void UpdateTurn();

        // 상태 문자열 반환
        public virtual string GetStatus()
        {
            string moveStr = MoveType == MovementType.Ground ? "지상" : "공중";
            string attrStr = Attribute == UnitAttribute.Biological ? "생체" : "기계";
            return $"[{GetRaceName()}] {Name} ({moveStr}/{attrStr}) | HP: {Hp}/{MaxHp} | 위치: ({X},{Y}) | 상태: {LastActionLog}";
        }

        protected abstract string GetRaceName();
    }

    // 4. 종족별 추상 클래스 (테란, 저그, 프로토스)
    public abstract class TerranUnit : Unit
    {
        protected TerranUnit(string name, int maxHp, MovementType moveType, UnitAttribute attribute, int x = 0, int y = 0)
            : base(name, maxHp, moveType, attribute, x, y) { }

        protected override string GetRaceName() => "테란";

        // 테란 특성: 체력이 30% 미만이면 스파크/출혈 상태 경고 (자연 재생 없음)
        public override void UpdateTurn()
        {
            if (Hp > 0 && Hp < MaxHp * 0.3)
            {
                LastActionLog += " [경고: 수리/치료 필요!]";
            }
        }
    }

    public abstract class ZergUnit : Unit
    {
        protected int RegenRate { get; set; } = 2;

        protected ZergUnit(string name, int maxHp, MovementType moveType, UnitAttribute attribute, int x = 0, int y = 0)
            : base(name, maxHp, moveType, attribute, x, y) { }

        protected override string GetRaceName() => "저그";

        // 저그 특성: 매 턴 체력 자동 재생
        public override void UpdateTurn()
        {
            if (Hp > 0 && Hp < MaxHp)
            {
                int prevHp = Hp;
                Hp = Math.Min(MaxHp, Hp + RegenRate);
                LastActionLog = $"생체 체력 재생 (+{Hp - prevHp})";
            }
        }
    }

    public abstract class ProtossUnit : Unit
    {
        public int Shield { get; protected set; }
        public int MaxShield { get; protected set; }
        protected int ShieldRegenRate { get; set; } = 3;

        protected ProtossUnit(string name, int maxHp, int maxShield, MovementType moveType, UnitAttribute attribute, int x = 0, int y = 0)
            : base(name, maxHp, moveType, attribute, x, y)
        {
            MaxShield = maxShield;
            Shield = maxShield;
        }

        protected override string GetRaceName() => "프로토스";

        // 프로토스 특성: 쉴드가 먼저 피해를 흡수
        public override void TakeDamage(int amount)
        {
            if (Shield >= amount)
            {
                Shield -= amount;
                LastActionLog = $"쉴드로 {amount} 피해 흡수 (남은 쉴드: {Shield}/{MaxShield})";
            }
            else
            {
                int remaining = amount - Shield;
                Shield = 0;
                Hp = Math.Max(0, Hp - remaining);
                LastActionLog = $"쉴드 파괴 및 본체 {remaining} 피해 (HP: {Hp}/{MaxHp})";
            }
        }

        // 프로토스 특성: 매 턴 쉴드 자동 재생
        public override void UpdateTurn()
        {
            if (Hp > 0 && Shield < MaxShield)
            {
                int prevShield = Shield;
                Shield = Math.Min(MaxShield, Shield + ShieldRegenRate);
                LastActionLog = $"플라즈마 쉴드 재생 (+{Shield - prevShield})";
            }
        }

        public override string GetStatus()
        {
            string moveStr = MoveType == MovementType.Ground ? "지상" : "공중";
            string attrStr = Attribute == UnitAttribute.Biological ? "생체" : "기계";
            return $"[{GetRaceName()}] {Name} ({moveStr}/{attrStr}) | HP: {Hp}/{MaxHp}, Shield: {Shield}/{MaxShield} | 위치: ({X},{Y}) | 상태: {LastActionLog}";
        }
    }

    // 5. 구체 유닛 클래스 구현 (인터페이스 다중 구현)
    public class SCV : TerranUnit, IWorker
    {
        public int CarryingMinerals { get; private set; }

        public SCV(int x = 0, int y = 0) : base("SCV", 60, MovementType.Ground, UnitAttribute.Mechanical, x, y) { }

        public void GatherMineral()
        {
            CarryingMinerals += 8;
            LastActionLog = $"미네랄 채취 완료 (보유: {CarryingMinerals})";
        }

        public void BuildStructure(string structureName)
        {
            LastActionLog = $"'{structureName}' 건설 시작!";
        }
    }

    public class Wraith : TerranUnit, IClockable
    {
        public bool IsCloaked { get; private set; }

        public Wraith(int x = 1, int y = 1) : base("레이스", 120, MovementType.Air, UnitAttribute.Mechanical, x, y) { }

        public void ToggleCloak()
        {
            IsCloaked = !IsCloaked;
            LastActionLog = IsCloaked ? "클로킹(은폐) 활성화!" : "클로킹 해제";
        }

        public override string GetStatus() => base.GetStatus() + (IsCloaked ? " [은폐중]" : "");
    }

    public class Dropship : TerranUnit, ITransport
    {
        public int Capacity { get; } = 8;
        public List<Unit> Passengers { get; } = new List<Unit>();

        public Dropship(int x = 2, int y = 2) : base("드랍쉽", 150, MovementType.Air, UnitAttribute.Mechanical, x, y) { }

        public void Load(Unit unit)
        {
            if (unit == this)
            {
                LastActionLog = "자기 자신은 탑승할 수 없습니다.";
                return;
            }
            if (unit.MoveType != MovementType.Ground)
            {
                LastActionLog = $"{unit.Name}은(는) 공중 유닛이라 탑승 불가!";
                return;
            }
            if (Passengers.Count >= Capacity)
            {
                LastActionLog = "수송선 정원 초과!";
                return;
            }
            Passengers.Add(unit);
            LastActionLog = $"{unit.Name} 탑승 완료 (현재 {Passengers.Count}/{Capacity})";
        }

        public void UnloadAll()
        {
            int count = Passengers.Count;
            Passengers.Clear();
            LastActionLog = $"탑승 유닛 {count}기 전원 하차 완료!";
        }

        public override string GetStatus() => base.GetStatus() + $" [탑승: {Passengers.Count}/{Capacity}]";
    }

    public class Zergling : ZergUnit, IBurrowable
    {
        public bool IsBurrowed { get; private set; }

        public Zergling(int x = 3, int y = 0) : base("저글링", 35, MovementType.Ground, UnitAttribute.Biological, x, y) { }

        public void ToggleBurrow()
        {
            IsBurrowed = !IsBurrowed;
            LastActionLog = IsBurrowed ? "버로우(잠복) 완료 - 이동 불가" : "버로우 해제";
        }

        public override void Move(int dx, int dy)
        {
            if (IsBurrowed)
            {
                LastActionLog = "잠복 상태에서는 이동할 수 없습니다!";
                return;
            }
            base.Move(dx, dy);
        }

        public override string GetStatus() => base.GetStatus() + (IsBurrowed ? " [잠복중]" : "");
    }

    public class Overlord : ZergUnit, IDetector, ITransport
    {
        public int DetectionRange { get; } = 10;
        public int Capacity { get; } = 8;
        public List<Unit> Passengers { get; } = new List<Unit>();

        public Overlord(int x = 3, int y = 3) : base("오버로드", 200, MovementType.Air, UnitAttribute.Biological, x, y) { }

        public void Scan(IEnumerable<Unit> units)
        {
            var hiddenUnits = units.Where(u =>
                (u is IClockable c && c.IsCloaked) ||
                (u is IBurrowable b && b.IsBurrowed)
            ).Select(u => u.Name).ToList();

            LastActionLog = hiddenUnits.Any()
                ? $"디텍팅 성공! 은폐/잠복 유닛 발견: {string.Join(", ", hiddenUnits)}"
                : "디텍팅 스캔 완료 (은폐/잠복 유닛 없음)";
        }

        public void Load(Unit unit)
        {
            if (unit.MoveType != MovementType.Ground)
            {
                LastActionLog = $"{unit.Name}은(는) 지상 유닛이 아니어서 탑승 불가!";
                return;
            }
            Passengers.Add(unit);
            LastActionLog = $"{unit.Name} 수송 탑승 완료 (현재 {Passengers.Count}/{Capacity})";
        }

        public void UnloadAll()
        {
            int count = Passengers.Count;
            Passengers.Clear();
            LastActionLog = $"유닛 {count}기 하차 완료!";
        }
    }

    public class Observer : ProtossUnit, IClockable, IDetector
    {
        public bool IsCloaked { get; private set; } = true; // 옵저버는 기본 영구 은폐
        public int DetectionRange { get; } = 11;

        public Observer(int x = 5, int y = 5) : base("옵저버", 40, 20, MovementType.Air, UnitAttribute.Mechanical, x, y) { }

        public void ToggleCloak()
        {
            IsCloaked = !IsCloaked;
            LastActionLog = IsCloaked ? "은폐 모드 활성화" : "은폐 모드 해제";
        }

        public void Scan(IEnumerable<Unit> units)
        {
            var hiddenUnits = units.Where(u =>
                u != this &&
                ((u is IClockable c && c.IsCloaked) || (u is IBurrowable b && b.IsBurrowed))
            ).Select(u => u.Name).ToList();

            LastActionLog = hiddenUnits.Any()
                ? $"관측기 탐지! 발견된 은신 유닛: {string.Join(", ", hiddenUnits)}"
                : "주변 은신 유닛 없음";
        }

        public override string GetStatus() => base.GetStatus() + (IsCloaked ? " [영구은폐]" : "");
    }

    // 6. 메인 게임 루프 (입력 -> 갱신 -> 렌더링) - Program.cs
    public class Program
    {
        private static List<Unit> _units = new List<Unit>();
        private static int _selectedIndex = 0;
        private static int _turn = 1;
        private static bool _isRunning = true;

        public static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            InitializeGame();

            // 게임 루프: 입력(Input) -> 갱신(Update) -> 렌더링(Render)
            while (_isRunning)
            {
                Render();               // 1. 렌더링 (현재 상태 출력)
                string input = Input(); // 2. 입력 (사용자 명령 수신)
                Update(input);          // 3. 갱신 (명령 처리 및 턴/패시브 업데이트)
            }

            Console.WriteLine("게임을 종료합니다.");
        }

        private static void InitializeGame()
        {
            _units = new List<Unit>
            {
                new SCV(0, 0),
                new Wraith(2, 2),
                new Dropship(4, 1),
                new Zergling(1, 3),
                new Overlord(3, 4),
                new Observer(5, 5)
            };
        }

        // [입력 단계]
        private static string Input()
        {
            Console.Write(" ");
            return Console.ReadLine()?.Trim().ToLower() ?? "";
        }

        // [갱신 단계]
        private static void Update(string command)
        {
            if (string.IsNullOrEmpty(command)) return;

            Unit selectedUnit = _units[_selectedIndex];
            bool turnPassed = true;

            switch (command)
            {
                case "q":
                    _isRunning = false;
                    return;

                case "n": // 다음 유닛 선택
                    _selectedIndex = (_selectedIndex + 1) % _units.Count;
                    turnPassed = false;
                    break;

                case "m": // 이동
                    selectedUnit.Move(1, 1);
                    break;

                case "d": // 테스트 데미지 입히기 (종족별 체력/쉴드 재생 확인용)
                    selectedUnit.TakeDamage(25);
                    break;

                case "1": // 인터페이스 기능 1: 클로킹(IClockable) 또는 버로우(IBurrowable)
                    if (selectedUnit is IClockable clockable)
                        clockable.ToggleCloak();
                    else if (selectedUnit is IBurrowable burrowable)
                        burrowable.ToggleBurrow();
                    else
                        Console.WriteLine("이 유닛은 은폐/잠복 기능이 없습니다.");
                    break;

                case "2": // 인터페이스 기능 2: 탐지(IDetector)
                    if (selectedUnit is IDetector detector)
                        detector.Scan(_units);
                    else
                        Console.WriteLine("이 유닛은 디텍터(탐지) 기능이 없습니다.");
                    break;

                case "3": // 인터페이스 기능 3: 일꾼 채취/건설(IWorker)
                    if (selectedUnit is IWorker worker)
                    {
                        worker.GatherMineral();
                        worker.BuildStructure("서플라이 디폿");
                    }
                    else
                        Console.WriteLine("이 유닛은 일꾼 기능이 없습니다.");
                    break;

                case "4": // 인터페이스 기능 4: 수송선 탑승/하차(ITransport)
                    if (selectedUnit is ITransport transport)
                    {
                        if (transport.Passengers.Count == 0)
                        {
                            // 주변 첫 번째 지상 유닛 태우기 시도
                            var groundUnit = _units.FirstOrDefault(u => u != selectedUnit && u.MoveType == MovementType.Ground);
                            if (groundUnit != null) transport.Load(groundUnit);
                        }
                        else
                        {
                            transport.UnloadAll();
                        }
                    }
                    else
                        Console.WriteLine("이 유닛은 수송 기능이 없습니다.");
                    break;

                case "t": // 아무 행동 없이 턴 넘기기 (체력/쉴드 재생 확인)
                    break;

                default:
                    Console.WriteLine("잘못된 명령입니다.");
                    turnPassed = false;
                    break;
            }

            // 턴이 경과하면 모든 유닛의 종족별 UpdateTurn() 다형성 호출 (저그 체력재생, 프로토스 쉴드재생)
            if (turnPassed)
            {
                _turn++;
                foreach (var unit in _units)
                {
                    unit.UpdateTurn();
                }
            }
        }

        // [렌더링 단계]
        private static void Render()
        {
            Console.Clear();
            Console.WriteLine("====================================================================================");
            Console.WriteLine($"  [StarCraft 다형성 시뮬레이션]  현재 턴: {_turn}");
            Console.WriteLine("====================================================================================");

            for (int i = 0; i < _units.Count; i++)
            {
                string pointer = (i == _selectedIndex) ? "▶ " : "   ";
                Console.WriteLine($"{pointer}{i + 1}. {_units[i].GetStatus()}");
            }

            Console.WriteLine("------------------------------------------------------------------------------------");
            Console.WriteLine("[조작법] N: 유닛 변경 | M: 이동 | D: 피해 입히기(25) | T: 턴 넘기기(재생 확인)");
            Console.WriteLine("[특수기] 1: 은폐/잠복 | 2: 디텍팅 스캔 | 3: 미네랄 채취/건설 | 4: 수송선 탑승/하차 | Q: 종료");
            Console.WriteLine("====================================================================================");
        }
    }
}