namespace ConsoleApp1;

// View: Result

public static class ResultPage
{
    public static string Render(string title, List<CustomerInfo?>? results)
    {
        return RenderHighlight(title, results, "");
    }

    public static string RenderEmpty(string title)
    {
        return RenderHighlight(title, null, "");
    }

    public static string RenderHighlight(string title, List<CustomerInfo?>? results, string highlight)
    {
        string result = $"<h2>{title}</h2> <table border=\"1\">";
        result += $"<tr> <th>Customer ID</th> <th>Score</th> </tr>";

        if (results != null)
        {
            foreach (var info in results)
            {
                var t1 = info!.ID == highlight ? "<td bgcolor=\"red\">" : "<td>";
                var t2 = "</td>";
                result += $"<tr> {t1}{info.ID}{t2} {t1}{info.Score}{t2} </tr>";
            }
        }

        result += "</table>";
        return result;
    }

    public static string Render404()
    {
        var str = "404:\n\n Hello,Jerry\n Are you lost?";
        return str;
    }
}
