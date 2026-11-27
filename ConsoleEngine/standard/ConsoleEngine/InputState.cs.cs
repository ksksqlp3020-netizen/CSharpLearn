using System;
using System.Collections.Generic;
using System.Text;

namespace ConsoleEngine
{
    public class InputState
    {
        private readonly List<ConsoleKey> _pressed = new List<ConsoleKey>();

        /// <summary>이번 프레임에 이 키가 한 번이라도 눌렸는가.</summary>
        public bool WasPressed(ConsoleKey key)
        {
            return _pressed.Contains(key);
        }

        /// <summary>이번 프레임에 이 키가 눌린 횟수. 길게 누르면 키 반복으로 여러 번 온다.</summary>
        public int CountPressed(ConsoleKey key)
        {
            int count = 0;
            foreach (ConsoleKey k in _pressed)
                if (k == key) count++;
            return count;
        }

        internal void BeginFrame()
        {
            _pressed.Clear();
        }

        internal void Add(ConsoleKey key)
        {
            _pressed.Add(key);
        }

        public class KeyMap
        {
            private readonly Dictionary<ConsoleKey, ICommand> _bindings = new Dictionary<ConsoleKey, ICommand>();

            public void Bind(ConsoleKey key, ICommand command)
            {
                _bindings[key] = command;               // 있으면 바꾸고, 없으면 더한다
            }

            public void Unbind(ConsoleKey key)
            {
                _bindings.Remove(key);
            }

            public bool IsBound(ConsoleKey key)
            {
                return _bindings.ContainsKey(key);
            }

            public void Rebind(ConsoleKey oldKey, ConsoleKey newKey)
            {
                if (!_bindings.TryGetValue(oldKey, out ICommand? command)) return;
                _bindings.Remove(oldKey);
                _bindings[newKey] = command;
            }

            /// <summary>key에 묶인 명령이 있으면 실행하고 true, 없으면 false.</summary>
            public bool Handle(ConsoleKey key)
            {
                if (!_bindings.TryGetValue(key, out ICommand? command)) return false;
                command.Execute();
                return true;
            }
        }
    }
}
