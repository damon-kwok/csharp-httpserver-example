using System.Diagnostics.CodeAnalysis;

namespace ConsoleApp1;

// TODO
// InsertAt DeleteAt UpdateAt...

public class SkipList<TKey, TScore, TData>
    where TKey : IComparable<TKey>
    where TScore : IComparable<TScore>
    where TData : class
{
    private Node? _head;
    private Node? _tail;
    private int _curLevel;
    private int _count;

    private readonly int _maxLevel;
    private readonly bool _reverseOrder;

    private readonly Random _random = new Random();

    private readonly IDictionary<TKey, TScore> _scoreCaches =
        new Dictionary<TKey, TScore>();

    //private readonly IDictionary<TKey, Node> _nodeCaches =
    //    new Dictionary<TKey, Node>();

    public SkipList(bool reverseOrder, int maxLevel)
    {
        _maxLevel = maxLevel;
        _reverseOrder = reverseOrder;
        Clear();
    }

    public void Clear()
    {
        _curLevel = 1;
        _count = 0;
        _head = new Node(default(TKey)!, default(TScore)!, _maxLevel);
        for (var i = 0; i < _maxLevel; i++)
        {
            _head!.Levels[i].Forward = null;
            _head!.Levels[i].Span = 0;
        }

        _head!.Backward = null;
        _tail = null;

        // create caches
        _scoreCaches.Clear();
        //_nodeCaches.Clear();
    }

    public int Count()
    {
        return _count;
    }

    public int MaxLevel()
    {
        return _maxLevel;
    }

    public void Insert(TKey key, TScore score, TData? data)
    {
        if (this._scoreCaches.ContainsKey(key))
        {
            this.Update(key, score, data);
            return;
        }

        var cur = _head;
        var update = new Node?[_maxLevel];
        var rank = new int[_maxLevel];
        for (var i = _curLevel - 1; i >= 0; i--)
        {
            // store rank that is crossed to reach the insert position
            rank[i] = i == (_curLevel - 1) ? 0 : rank[i + 1];
            while (cur!.Levels[i].Forward != null && //
                   (CompareScore(cur.Levels[i].Forward!, score) < 0 ||
                    (CompareScore(cur.Levels[i].Forward!, score) == 0 &&
                     cur.Levels[i].Forward!.Key.CompareTo(key) != 0)))
            {
                rank[i] += cur.Levels[i].Span;
                cur = cur.Levels[i].Forward;
            }

            update[i] = cur;
        }

        var randLevel = this.RandomLevel();
        if (randLevel > _curLevel)
        {
            for (var i = _curLevel; i < randLevel; i++)
            {
                rank[i] = 0;
                update[i] = _head;
                update[i]!.Levels[i].Span = _count;
            }

            _curLevel = randLevel;
        }

        // create
        var node = CreateNode(key, score, data, randLevel);

        // insert
        for (var i = 0; i < randLevel; i++)
        {
            node.Levels[i].Forward = update[i]?.Levels[i].Forward;
            update[i]!.Levels[i].Forward = node;
            /* update span covered by update[i] as x is inserted here */
            node.Levels[i].Span =
                update[i]!.Levels[i].Span - (rank[0] - rank[i]);
            update[i]!.Levels[i].Span = (rank[0] - rank[i]) + 1;
        }

        /* increment span for untouched levels */
        for (var i = randLevel; i < _curLevel; i++)
            update[i]!.Levels[i].Span++;

        node.Backward = (update[0] == _head) ? null : update[0];
        if (node.Levels[0].Forward != null)
            node.Levels[0].Forward!.Backward = node;
        else
            _tail = node;

        _count++;

        // update cache
        _scoreCaches[key] = score;
        //_nodeCaches[key] = node;
        //Console.WriteLine($"Successfully inserted key: {key} , Score: {score}");
    }

    private void DeleteNode(Node node, IReadOnlyList<Node> update)
    {
        for (var i = 0; i < _curLevel; i++)
        {
            if (update[i].Levels[i].Forward == node)
            {
                update[i].Levels[i].Span = node.Levels[i].Span - 1;
                update[i].Levels[i].Forward = node.Levels[i].Forward;
            }
            else
            {
                update[i].Levels[i].Span -= 1;
            }
        }

        if (node.Levels[0].Forward != null)
            node.Levels[0].Forward!.Backward = node.Backward;
        else
            _tail = node.Backward;

        // Remove levels which have no elements
        while (_curLevel > 1 && _head!.Levels[_curLevel - 1].Forward == null)
            _curLevel--;

        _count--;

        // update cache
        _scoreCaches.Remove(node.Key);
        //_nodeCaches.Remove(key);
    }

    public bool ContainsKey(TKey key)
    {
        return this._scoreCaches.ContainsKey(key);
        //var (_, node) = this.GetNodeByKey(key);
        //return node != null;
    }

    public void Delete(TKey key, TScore score)
    {
        var update = new Node[_maxLevel];
        var cur = _head;

        for (var i = _curLevel - 1; i >= 0; i--)
        {
            while (cur?.Levels[i].Forward != null && //
                   (CompareScore(cur.Levels[i].Forward!, score) < 0 ||
                    (CompareScore(cur.Levels[i].Forward!, score) == 0 &&
                     cur.Levels[i].Forward!.Key.CompareTo(key) != 0)))
            {
                cur = cur.Levels[i].Forward;
            }

            update[i] = cur!;
        }

        // We may have multiple elements with the same score, what we need
        // is to find the element with both the right 'score' and 'key'.
        cur = cur!.Levels[0].Forward;
        if (cur == null ||
            score.CompareTo(cur.Score) != 0 ||
            cur.Key.CompareTo(key) != 0)
        {
            return;
        }

        this.DeleteNode(cur, update);
        //Console.WriteLine($"Successfully deleted key:{key}");
        //return cur;
    }

    public void Update(TKey key, TScore newScore, TData? d)
    {
        var (_, node) = this.GetNodeByKey(key);
        if (node == null)
        {
            this.Insert(key, newScore, d);
            return;
        }

        // current score
        var curScore = node.Score;

        // don't need update score
        if (curScore.CompareTo(newScore) == 0)
        {
            // only update data
            node.Data = d;
        }
        else
        {
            this.Delete(key, curScore);
            this.Insert(key, newScore, d);
        }
        /*
        // We need to seek to element to update to start: this is useful anyway,
        // we'll have to update or remove it.
        var update = new Node[_maxLevel];
        var cur = _head;
        for (var i = _curLevel - 1; i >= 0; i--)
        {
            while (cur!.Levels[i].Forward != null && //
                   (CompareScore(cur.Levels[i].Forward!, curScore) < 0 ||
                    (CompareScore(cur.Levels[i].Forward!, curScore) == 0 &&
                     cur.Levels[i].Forward!.Key.CompareTo(key) != 0)))
            {
                cur = cur.Levels[i].Forward;
            }

            update[i] = cur;
        }

        // Jump to our element: note that this function assumes that the
        // element with the matching score exists.
        cur = cur!.Levels[0].Forward;

        // If the node, after the score update, would be still exactly
        // at the same position, we can just update the score without
        // actually removing and re-inserting the element in the SkipList.
        if ((cur!.Backward != null &&
             CompareScore(cur.Backward!, newScore) >= 0) ||
            (cur.Levels[0].Forward != null &&
             CompareScore(cur.Levels[0].Forward!, newScore) <= 0)) return;
        cur.Score = newScore;

        // update cache
        _scoreCaches[key] = newScore;
        //_nodeCaches[key] = cur;

        //Console.WriteLine($"Successfully updated key:{key} to newScore:{newScore}");
        //return cur;

        // No way to reuse the old node: we need to remove and insert a new
        // one at a different place.
        this.DeleteNode(cur, update);
        this.Insert(key, newScore, d);*/
    }

    public override string ToString()
    {
        var result = "\n============Skip List============\n";
        for (var i = 0; i <= _curLevel; i++)
        {
            var node = _head!.Levels[i].Forward;
            result += $"Level {i}: ";
            while (node != null)
            {
                result += $"{node.Key}:{node.Score};";
                node = node.Levels[i].Forward;
            }

            result += "\n";
        }

        return result;
    }

    private const int Threshold = (0xffff / 4);

    private int RandomLevel()
    {
        var level = 1; // limit: min level is 1
        while (_random.Next(_maxLevel) < Threshold && level < _maxLevel)
            level++;
        return level;
    }

    private int CompareScore(TScore a, TScore b)
    {
        var v = a.CompareTo(b);
        v = _reverseOrder switch
        {
            true when v == -1 => 1,
            true when v == 1 => -1,
            _ => v
        };
        return v;
    }

    private int CompareScore(Node a, TScore score)
    {
        var v = a.Score.CompareTo(score);
        v = _reverseOrder switch
        {
            true when v == -1 => 1,
            true when v == 1 => -1,
            _ => v
        };
        return v;
    }

    public List<TData> Top(int n)
    {
        var results = new List<TData>();
        var cur = _head!.Levels[0].Forward;
        while (cur != null && n > 0)
        {
            if (cur.Data != null)
            {
                results.Add(cur.Data);
                cur = cur.Levels[0].Forward;
            }

            n--;
        }

        return results;
    }

    private Node? GetNodeByScore(TScore score)
    {
        var cur = _head;
        for (var i = _curLevel - 1; i >= 0; i--)
        {
            while (cur?.Levels[i].Forward != null &&
                   this.CompareScore(cur.Levels[i].Forward!, score) >= 0)
                cur = cur.Levels[i].Forward;
        }

        // This is an inner range, so the next node cannot be NULL.
        cur = cur!.Levels[0].Forward;
        return cur;
    }

    public TData? GetDataByScore(TScore score)
    {
        var node = this.GetNodeByScore(score);

        if (node != null && node.Data != null)
            return node.Data;
        return default(TData);
    }

    private Node? GetNodeByRank(long rank)
    {
        if (this.Count() < rank)
            return null;
        long traversed = 0;
        var cur = _head;
        for (var i = _curLevel - 1; i >= 0; i--)
        {
            while (cur?.Levels[i].Forward != null &&
                   (traversed + cur.Levels[i].Span) <= rank)
            {
                traversed += cur.Levels[i].Span;
                cur = cur.Levels[i].Forward;
            }

            if (traversed == rank)
                return cur;
        }

        return null;
    }


    public Tuple<long, TData?> GetDataByKey(TKey key)
    {
        var (rank, node) = this.GetNodeByKey(key);

        return new Tuple<long, TData?>(rank, node?.Data);
    }

    private Tuple<long, Node?> GetNodeByKey(TKey key)
    {
        if (!this.ContainsKey(key))
            return new Tuple<long, Node?>(0, null);

        long traversed = 0;
        var score = _scoreCaches[key];
        var cur = _head;

        for (var i = _curLevel - 1; i >= 0; i--)
        {
            while (cur?.Levels[i].Forward != null && //
                   (CompareScore(cur.Levels[i].Forward!, score) < 0 ||
                    (CompareScore(cur.Levels[i].Forward!, score) == 0 &&
                     cur.Levels[i].Forward!.Key.CompareTo(key) != 0)))
            {
                cur = cur.Levels[i].Forward;
                traversed += cur!.Levels[i].Span;
            }
        }

        // We may have multiple elements with the same score, what we need
        // is to find the element with both the right 'score' and 'key'.
        cur = cur!.Levels[0].Forward;
        if (cur == null ||
            score.CompareTo(cur.Score) != 0 ||
            cur.Key.CompareTo(key) != 0)
        {
            return new Tuple<long, Node?>(0, null);
        }

        traversed += cur.Levels[0].Span;

        return new Tuple<long, Node?>(traversed, cur);
    }

    private bool IsInScoreRange(TScore start, TScore end)
    {
        var cur = _tail;

        // Test <= end
        if (cur == null || this.CompareScore(cur, end) >= 0)
            return false;

        cur = _head!.Levels[0].Forward;
        // Test: >= start
        return cur != null && this.CompareScore(cur, start) >= 0;
    }

    public List<TData> ScoreRange(TScore a, TScore b)
    {
        var results = new List<TData>();
        var low = CompareScore(a, b) < 0 ? a : b;
        var high = CompareScore(a, b) > 0 ? a : b;

        if (!IsInScoreRange(low, high))
            return results;

        var cur = _head;
        for (var i = _curLevel - 1; i >= 0; i--)
        {
            while (cur?.Levels[i].Forward != null &&
                   this.CompareScore(cur.Score, high) <= 0)
            {
                cur = cur.Levels[i].Forward;
                if (cur!.Data != null)
                    results.Add(cur.Data);
            }
        }

        return results;
    }

    public List<TData> Range(long rank1, long rank2)
    {
        var results = new List<TData>();
        var start = Math.Min(rank1, rank2);
        var end = Math.Max(rank1, rank2);
        if (start < 1)
            start = 1;
        if (end < 1)
            end = 1;

        var n = end - start;
        var cur = this.GetNodeByRank(start);
        while (cur != null && n-- >= 0)
        {
            if (cur.Data != null)
                results.Add(cur.Data);
            cur = cur.Levels[0].Forward;
        }

        return results;
    }

    public List<Tuple<long, TData>> Around(TKey key, long a, long b)
    {
        var results = new List<Tuple<long, TData>>();
        if (!this._scoreCaches.ContainsKey(key))
            return results;

        var high = Math.Min(a, b);
        var low = Math.Max(a, b);
        if (high < 0)
            high = 0;
        if (low < 0)
            low = 0;

        var (rank, node) = this.GetNodeByKey(key);

        if (node == null)
            return results;

        if (node.Data != null)
            results.Add(new Tuple<long, TData>(rank, node.Data));
        // high
        var i = 0;
        var cur = node;
        while (cur != null && cur != _head && high > 0)
        {
            cur = cur.Backward;
            i++;
            if (cur != null && cur.Data != null)
                results.Insert(0, new Tuple<long, TData>(rank - i, cur.Data));
            high--;
        }

        // low
        cur = node;
        i = 0;
        while (cur != null && low > 0)
        {
            cur = cur.Levels[0].Forward;
            i++;
            if (cur != null && cur.Data != null)
                results.Add(new Tuple<long, TData>(rank - i, cur.Data));
            low--;
        }

        return results;
    }

    private static Node CreateNode(TKey k, TScore score, TData? d, int level)
    {
        var node = new Node(k, score, level)
        {
            Data = d
        };
        return node;
    }

    private class NodeLevel
    {
        public Node? Forward { get; set; }
        public int Span { get; set; }
    }

    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    private class Node
    {
        public TKey Key { get; }

        public TScore Score { get; set; }

        //private int LevelValue { get; }
        public NodeLevel[] Levels { get; }
        public Node? Backward { get; set; }

        public int Span { get; set; }
        public TData? Data { get; set; }

        public Node(TKey k, TScore score, int level)
        {
            this.Key = k;
            this.Score = score;
            this.Data = default(TData);
            //this.Span = 0;
            this.Backward = null;
            this.Levels = new NodeLevel[level];
            for (var i = 0; i < level; i++)
            {
                this.Levels[i] = new NodeLevel();
            }
        }
    }
}
