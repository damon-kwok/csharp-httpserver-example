// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Text.RegularExpressions;
using ConsoleApp1;

var data = new SkipList<string, long, CustomerInfo>(true, 10);

var server = new ResTfulService(Environment.ProcessorCount);
server.GET("/", delegate
{
    string renderString;
    const int topN = 100;

    lock (data)
    {
        // Render TopN
        renderString =
            ResultPage.Render($"Home Page: Top {topN}:\n", data.Top(topN), 1);
    }

    var response = new Tuple<int, string>(200, renderString);
    return response;
});

server.GET("/reset", delegate
{
    lock (data)
    {
        data.Clear();
    }

    var response = new Tuple<int, string>(200, "All data is clear!");
    return response;
});

server.GET("/init", delegate
{
    string renderString;
    const int topN = 100;

    lock (data)
    {
        data.Clear();
        for (var i = 0; i < topN; i++)
            data.Insert($"{10001 + i}", i + 1,
                new CustomerInfo($"{10001 + i}", i + 1));

        renderString =
            ResultPage.Render($"Init: Top {topN}:\n", data.Top(topN), 1);
    }

    var response = new Tuple<int, string>(200, renderString);
    return response;
});

server.POST(@"/customer/\d+/score/\d+", delegate(HttpListenerContext context)
{
    var input = context.Request.Url?.LocalPath;
    const string pattern = @"\d+";
    var mc = Regex.Matches(input!, pattern);
    var customerId = mc[0].Value;
    var score = Convert.ToInt64(mc[1].Value);
    string renderString;
    const int topN = 100;

    lock (data)
    {
        var method = data.ContainsKey(customerId) ? "Update" : "Insert";
        data.Update(customerId, score, new CustomerInfo(customerId, score));
        // Render TopN
        renderString = ResultPage.Render($"{method} succeed! Top {topN}:\n",
            data.Top(topN), 1);
    }

    var response = new Tuple<int, string>(200, renderString);
    return response;
});

server.GET(@"/leaderboard", delegate(HttpListenerContext context)
{
    var query = context.Request.QueryString;
    long start = 0;
    long end = 10;
    if (!string.IsNullOrEmpty(query["start"]))
        start = Convert.ToInt64(query["start"]);
    if (!string.IsNullOrEmpty(query["end"]))
        end = Convert.ToInt64(query["end"]);
    start = Math.Min(start, end);
    end = Math.Max(start, end);
    if (start < 1)
        start = 1;
    if (end < 1)
        end = 1;

    string renderString;
    lock (data)
    {
        renderString = ResultPage.Render($"Leaderboard:{start}-{end}:\n",
            data.Range(start, end), start);
    }

    var response = new Tuple<int, string>(200, renderString);
    return response;
});

server.GET(@"/leaderboard/\d+", delegate(HttpListenerContext context)
{
    var input = context.Request.Url?.LocalPath;
    const string pattern = @"\d+";
    var mc = Regex.Matches(input!, pattern);
    var customerId = mc[0].Value;

    var query = context.Request.QueryString;
    long high = 0;
    long low = 0;
    if (!string.IsNullOrEmpty(query["high"]))
    {
        high = Convert.ToInt64(query["high"]);
    }

    if (!string.IsNullOrEmpty(query["low"]))
    {
        low = Convert.ToInt64(query["low"]);
    }

    string renderString;
    lock (data)
    {
        // Render around
        renderString = ResultPage.RenderHighlight(
            $"Leaderboard:: Customer:{customerId} (High:{high}-Low:{low}):\n",
            data.Around(customerId, high, low), customerId);
    }

    var response = new Tuple<int, string>(200, renderString);
    return response;
});

server.Default(delegate
{
    var response = new Tuple<int, string>(404, ResultPage.Render404());
    return response;
});

server.Start(8000);
