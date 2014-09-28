using System;
using System.Threading.Tasks;

namespace softWrench.iOS.Utilities
{
    public sealed class AsyncSearchOptions<T>
    {
        private const int DefaultMininumSearchTextLength = 3;
        private const int DefaultDelayMilliseconds = 1200;

        private readonly Func<string, Task<T>> _search;
        private readonly Action<T> _reload;
        private readonly int _mininumSearchTextLength;
        private readonly TimeSpan _delay;

        public AsyncSearchOptions(Func<string, Task<T>> search, Action<T> reload)
            : this(search, reload, DefaultMininumSearchTextLength, TimeSpan.FromMilliseconds(DefaultDelayMilliseconds))
        {
        }

        public AsyncSearchOptions(Func<string, Task<T>> search, Action<T> reload, int mininumSearchTextLength, TimeSpan delay)
        {
            _search = search;
            _reload = reload;
            _mininumSearchTextLength = mininumSearchTextLength;
            _delay = delay;
        }

        public Func<string, Task<T>> Search
        {
            get { return _search; }
        }

        public Action<T> Reload
        {
            get { return _reload; }
        }

        public int MininumSearchTextLength
        {
            get { return _mininumSearchTextLength; }
        }

        public TimeSpan Delay
        {
            get { return _delay; }
        }
    }
}