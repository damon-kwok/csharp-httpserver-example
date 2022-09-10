using System.Text;

namespace ConsoleApp1;

public class SkipList<T>
{
    private const int MaxLevel = 10;
    private readonly Func<T, T, int> _sortComparator;
    private readonly Func<T, T, bool> _eraseComparator;
    private Node<T?>? _head = new(default, MaxLevel);
    private Random _random = new();
    private int _curMaxLevel = 1;

    public SkipList(Func<T, T, int> sortComparator, Func<T, T, bool> eraseComparator)
    {
        _sortComparator = sortComparator;
        _eraseComparator = eraseComparator;
    }

    public void Clear()
    {
        _head = new(default, MaxLevel);
        _random = new();
        _curMaxLevel = 1;
    }

    public int GetCurMaxLevel()
    {
        return _curMaxLevel;
    }

    public override string ToString()
    {
        var builder = new StringBuilder();
        var p = _head;
        while (p?.GetNext(0) != null)
        {
            builder.Append("\t{data:" + p.GetNext(0)!.GetData() + ",level:" +
                           p.GetNext(0)!.GetLevel() + "},\n");
            p = p.GetNext(0);
        }

        if (builder.Length != 0) builder.Remove(builder.Length - 1, 1);
        builder.Insert(0, "SkipList:: {\n");
        builder.Append("\n}");
        return builder.ToString();
    }

    private int RandomLevel()
    {
        var level = 1; // limit: min level is 1
        while (_random.Next(MaxLevel) % 2 != 0 && level < MaxLevel) level++;
        return level;
    }

    public List<T?> Top(Int64 n)
    {
        return Range(0, n);
    }

    public List<T?> Range(Int64 a, Int64 b)
    {
        var results = new List<T?>();
        var start = Math.Min(a, b);
        var end = Math.Max(a, b);
        if (start < 0)
            start = 0;
        if (end < 1)
            end = 1;
        var tmp = _head?.GetNext(0);
        Int64 i = 0;
        // high
        while (tmp != null && end > 0)
        {
            if (i++ >= start)
            {
                results.Add(tmp.GetData());
            }

            tmp = tmp.GetNext(0);
            end--;
        }

        return results;
    }

    public List<T> Around(T data, Int64 high, Int64 low)
    {
        if (high < 0)
            high = 0;
        if (low < 0)
            low = 0;
        var nodes = FindNode(data, true);
        var results = new List<T>();
        foreach (var tup in nodes)
        {
            var cur = tup.Item2;
            if (!_eraseComparator(cur.GetData(), data))
                continue;
            var tmp = cur.GetPre(0);
            // high
            while (tmp != null && tmp != _head && high > 0)
            {
                results.Insert(0, tmp.GetData());
                tmp = tmp.GetPre(0);
                high--;
            }

            results.Add(cur.GetData());

            // low
            tmp = cur.GetNext(0);
            while (tmp != null && low > 0)
            {
                results.Add(tmp.GetData());
                tmp = tmp.GetNext(0);
                low--;
            }

            break;
        }

        return results;
    }

    public void Insert(T data)
    {
        // Create `new node'
        var level = RandomLevel();
        var newNode = new Node<T?>(data, level);

        // Create index for the `new node':
        // First, Find from top layer,  and then: Take the smallest node in the i-th layer that is larger than this node
        // as the next node of the i-th layer of this node
        var p = _head;
        for (var i = level - 1; i >= 0; i--)
        {
          while (p?.GetNext(i) != null &&
                 _sortComparator(p.GetNext(i)!.GetData()!, data) <= 0)
                p = p.GetNext(i);
            // Insert the new node at the i-th layer
            newNode.SetPre(i, p);
            newNode.SetNext(i, p?.GetNext(i));
            p!.SetNext(i, newNode);
        }

        _curMaxLevel = Math.Max(_curMaxLevel, level);
    }

    public List<T?> Earse(T data)
    {
        var nodes = FindNode(data, true);
        var result = new List<T?>();
        foreach (var tup in nodes)
        {
            var cur = tup.Item2;
            if (_eraseComparator(cur.GetData(), data))
            {
                //var level = cur.GetLevel();
                var pre = cur.GetPre(0);
                var next = cur.GetNext(0);
                if (pre != null)
                    pre.SetNext(0, next);
                if (next != null)
                    next.SetPre(0, pre);
                result.Add(cur.GetData());
                break;
            }
        }

        return result;
    }

    public void Update(T oldData, T newData)
    {
        Earse(oldData);
        Insert(newData);
    }

    private List<Tuple<int, Node<T>>> FindNode(T data, bool multiple)
    {
        var p = _head;
        var result = new List<Tuple<int, Node<T>>>();
        for (var i = _curMaxLevel - 1; i >= 0; i--)
        {
            while (p?.GetNext(i) != null &&
                   _sortComparator(p.GetNext(i)!.GetData()!, data) < 0)
                p = p.GetNext(i);
            if (p?.GetNext(i) != null &&
                _sortComparator(p.GetNext(i)!.GetData()!, data) == 0)
            {
                result.Add(new Tuple<int, Node<T>>(i, p.GetNext(i)!));
                if (multiple)
                {
                    var pre = p.GetNext(i);
                    var next = p.GetNext(i);

                    //look back
                    while (next?.GetNext(0) != null &&
                           _sortComparator(next.GetNext(0)!.GetData()!, data) == 0)
                    {
                        result.Add(new Tuple<int, Node<T>>(i, next.GetNext(0)!));
                        next = next.GetNext(0);
                    }

                    // look forward
                    while (pre?.GetPre(0) != null &&
                           pre.GetPre(0)!.GetData() != null &&
                           _sortComparator(pre.GetPre(0)!.GetData()!, data) == 0)
                    {
                        result.Add(new Tuple<int, Node<T>>(i, pre.GetPre(0)!));
                        pre = pre.GetPre(0);
                    }
                }

                break;
            }
        }

        return result;
    }

    public List<T?> Find(T data, bool multiple)
    {
        var nodes = FindNode(data, multiple);
        var result = new List<T?>();
        foreach (var tup in nodes)
        {
            result.Add(tup.Item2.GetData());
        }

        return result;
    }

    public List<T?> FindAll(T data)
    {
        return Find(data, true);
    }

    public T? FindOne(T data)
    {
        var values = Find(data, false);
        return values.Count() == 0 ? default : values[0];
    }

    public List<T?> FindAll()
    {
        var p = _head;
        var result = new List<T?>();
        while (p?.GetNext(0) != null)
        {
            result.Add(p.GetNext(0)!.GetData());
            p = p.GetNext(0);
        }

        return result;
    }

    private class Node<T>
    {
        private readonly T _data;

        private readonly int _level;

        private readonly Node<T>?[] _nextArray;

        private readonly Node<T>?[] _preArray;

        public Node(T data, int level)
        {
            _data = data;
            _level = level;
            _nextArray = new Node<T>?[level];
            _preArray = new Node<T>?[level];
        }

        public T GetData()
        {
            return _data;
        }

        public int GetLevel()
        {
            return _level;
        }

        public Node<T>? GetNext(int i)
        {
            if (i >= 0 && i < _nextArray.Length)
                return _nextArray[i];
            return null;
        }

        public void SetNext(int i, Node<T>? node)
        {
            if (i >= 0 && i < _nextArray.Length)
                _nextArray[i] = node;
        }

        public Node<T>? GetPre(int i)
        {
            if (i >= 0 && i < _preArray.Length)
                return _preArray[i];
            return null;
        }

        public void SetPre(int i, Node<T>? node)
        {
            if (i >= 0 && i < _preArray.Length)
                _preArray[i] = node;
        }
    }
}
