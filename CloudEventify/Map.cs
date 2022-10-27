namespace CloudEventify;

public class Map<T1, T2>
{
    private Dictionary<T1, T2> _forward = new Dictionary<T1, T2>();
    private Dictionary<T2, T1> _reverse = new Dictionary<T2, T1>();

    public Map()
    {
        Forward = new Indexer<T1, T2>(_forward);
        Reverse = new Indexer<T2, T1>(_reverse);
    }

    public class Indexer<T3, T4>
    {
        private Dictionary<T3, T4> _dictionary;
        public Indexer(Dictionary<T3, T4> dictionary)
        {
            _dictionary = dictionary;
        }
        public T4 this[T3 index]
        {
            get { return _dictionary[index]; }
            set { _dictionary[index] = value; }
        }

        public IReadOnlyDictionary<T3, T4> Items { get { return _dictionary; } }
        public IReadOnlyCollection<T3> Keys => _dictionary.Keys;
        public IReadOnlyCollection<T4> Values => _dictionary.Values;
    }

    public void Add(T1 t1, T2 t2)
    {
        _forward.Add(t1, t2);
        _reverse.Add(t2, t1);
    }

    public Indexer<T1, T2> Forward { get; private set; }
    public Indexer<T2, T1> Reverse { get; private set; }
}

public static class DictionaryExtension
{
    public static Map<T1, T2> ToMap<T1,T2>(this IDictionary<T1, T2> initalMap)
    {
        var map = new Map<T1, T2>();
        foreach (var item in initalMap)
        {
            map.Add(item.Key, item.Value);
        }
        return map;
    }

}
