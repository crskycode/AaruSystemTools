using System.Collections;

namespace ScriptTool
{
    internal class CommandAssembly : IReadOnlyCollection<CommandRecord>
    {
        private readonly List<CommandRecord> _records = [];

        public CommandAssembly()
        {
        }

        public void Add(int code, int addr, int size)
        {
            _records.Add(new CommandRecord
            {
                Code = code,
                Addr = addr,
                Size = size,
                NewAddr = addr
            });
        }

        public void Clear()
        {
            _records.Clear();
        }

        public IEnumerator<CommandRecord> GetEnumerator()
        {
            return _records.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count
        {
            get => _records.Count;
        }

        public int CodeSize
        {
            get => _records.Sum(x => x.Size);
        }
    }
}
