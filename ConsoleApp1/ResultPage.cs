namespace ConsoleApp1;

// View: Result

public static class ResultPage
{
    public static string Render(string title, List<CustomerInfo> results,
        long rankBegin)
    {
        var result = $"<h2>{title}</h2> <table border=\"1\">";
        result +=
            $"<tr> <th>Customer ID</th> <th>Score</th> <th>Rank</th> </tr>";
        const string t2 = "</td>";
        var i = 0;
        foreach (var info in results)
        {
            var rank = rankBegin + i++;
            var t1 = i % 2 == 0 ? "<td bgcolor=\"grey\">" : "<td>";
            result +=
                $"<tr> {t1}{info.Id}{t2} {t1}{info.Score}{t2}  {t1}{rank}{t2}</tr>";
        }

        result += "</table>";

        return result;
    }

    public static string RenderEmpty(string title)
    {
        return Render(title, new List<CustomerInfo>(), 0);
    }

    public static string RenderHighlight(string title,
        List<Tuple<long, CustomerInfo>> results, string highlight)
    {
        var result = $"<h2>{title}</h2> <table border=\"1\">";
        result +=
            $"<tr> <th>Customer ID</th> <th>Score</th> <th>Rank</th> </tr>";
        const string t2 = "</td>";
        foreach (var (rank, info) in results)
        {
            var t1 = info.Id == highlight ? "<td bgcolor=\"red\">" : "<td>";
            result +=
                $"<tr> {t1}{info.Id}{t2} {t1}{info.Score}{t2}  {t1}{rank}{t2}</tr>";
        }

        result += "</table>";

        return result;
    }

    public static string Render404()
    {
        return "404::\n Are you lost?";
    }

    public static string RenderTip(string title, string message)
    {
        return $"<h2>{title}</h2> {message}";
    }
}
