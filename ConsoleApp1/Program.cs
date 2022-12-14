// See https://aka.ms/new-console-template for more information

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

server.POST(@"/customer/\d+/score/-?\d+", context=>
{
    var input = context.Request.Url?.LocalPath;
    const string pattern = @"-?\d+";
    var mc = Regex.Matches(input!, pattern);
    var customerId = mc[0].Value;
    var score = Convert.ToInt64(mc[1].Value);

    string renderString;
    if (score is < -1000 or > 1000)
    {
        renderString = ResultPage.RenderTip("Tip::",
            $"score: {score} is invalid, 'score' is a decimal number in range of (-1000, 1000).");
    }
    else
    {
        const int topN = 100;
        lock (data)
        {
            var (_, info) = data.GetDataByKey(customerId);
            if (info != null)
            {
                const string method = "Update";
                data.Update(customerId, info.Score + score,
                    new CustomerInfo(customerId, info.Score + score));
                // Render TopN
                renderString = ResultPage.Render(
                    $"{method} succeed! \n Added {score} score for customer: {customerId}",
                    data.Top(topN), 1);
            }
            else
            {
                const string method = "Insert";
                data.Insert(customerId, score,
                    new CustomerInfo(customerId, score));
                var (rank, _) = data.GetDataByKey(customerId);
                // Render TopN
                renderString = ResultPage.Render(
                    $"{method} succeed! The new Customer: {customerId} current Rank: {rank}",
                    data.Top(topN), 1);
            }
        }
    }

    var response = new Tuple<int, string>(200, renderString);
    return response;
});

server.GET("/leaderboard", context=>
{
    var query = context.Request.QueryString;
    long start = 0;
    long end = 10;
    if (!string.IsNullOrEmpty(query["start"]))
        start = Math.Max(1, Convert.ToInt64(query["start"]));
    if (!string.IsNullOrEmpty(query["end"]))
        end = Math.Max(1, Convert.ToInt64(query["end"]));
    if(start >end)
        (start,end) = (end,start);

    string renderString;
    lock (data)
    {
        renderString = ResultPage.Render($"Leaderboard:: Rank({start}-{end})\n",
            data.Range(start, end), start);
    }

    var response = new Tuple<int, string>(200, renderString);
    return response;
});

server.GET(@"/leaderboard/\d+", context=>
{
    var path = context.Request.Url?.LocalPath;
    const string pattern = @"\d+";
    var mc = Regex.Matches(path!, pattern);
    var customerId = mc[0].Value;

    var query = context.Request.QueryString;
    long high = 0;
    long low = 0;
    if (!string.IsNullOrEmpty(query["high"]))
        high = Math.Max(0, Convert.ToInt64(query["high"]));
    if (!string.IsNullOrEmpty(query["low"]))
        low = Math.Max(0, Convert.ToInt64(query["low"]));

    string renderString;
    lock (data)
    {
        // Render around
        renderString = ResultPage.RenderHighlight(
            $"Leaderboard:: View the Customer: {customerId} (High:{high}-Low:{low})\n",
            data.Around(customerId, high, low), customerId);
    }

    var response = new Tuple<int, string>(200, renderString);
    return response;
});

server.DEFAULT(delegate
{
    var response = new Tuple<int, string>(404, ResultPage.Render404());
    return response;
});

server.Start(8000);
