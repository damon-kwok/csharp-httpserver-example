// See https://aka.ms/new-console-template for more information

using System.Net;
using System.Text.RegularExpressions;
using ConsoleApp1;

IDictionary<string, Int64> scoreCaches = new Dictionary<string, Int64>();
var data = new SkipList<CustomerInfo?>(delegate(CustomerInfo a, CustomerInfo b)
{
    if (a.Score == b.Score)
        return 0;
    if (a.Score < b.Score)
        return 1;
    return -1;
}, delegate(CustomerInfo a, CustomerInfo b) { return a.ID == b.ID; });

// Test SkipList
/*
lock (data)
{
    data.Insert(new CustomerInfo("1001", 10));
    data.Insert(new CustomerInfo("1002", 51));
    data.Insert(new CustomerInfo("1003", 10));
    data.Insert(new CustomerInfo("1004", 20));
    data.Insert(new CustomerInfo("1005", 32));
    
    Console.WriteLine(data);

    Console.WriteLine(Utils.ToString(data.FindAll(new CustomerInfo(10))));

    data.Update(new CustomerInfo("1003", 10), new CustomerInfo("1003", 999));
    Console.WriteLine(data);

    Console.WriteLine(Utils.ToString(data.FindAll(new CustomerInfo(10))));

    data.Earse(new CustomerInfo("1005", 32));
    Console.WriteLine(data);

    data.Clear();
    Console.WriteLine(data);
}
*/

var server = new RESTfulService(Environment.ProcessorCount);
server.GET("/", delegate(HttpListenerContext context)
{
    var renderString = "";
    var topN = 10;

    lock (data)
    {
        // Render TopN
        renderString = ResultPage.Render($"Home Page: Top {topN}:\n", data.Top(topN));
    }

    var response = new Tuple<int, string>(200, renderString);
    return response;
});

server.GET("/reset", delegate(HttpListenerContext context)
{
    lock (data)
    {
        data.Clear();
    }

    var response = new Tuple<int, string>(200, "All data is clear!");
    return response;
});

server.GET("/init", delegate(HttpListenerContext context)
{
    var renderString = "";
    var topN = 10;

    lock (data)
    {
        data.Clear();
        data.Insert(new CustomerInfo("1001", 1));
        scoreCaches["1001"] = 1;
        data.Insert(new CustomerInfo("1002", 2));
        scoreCaches["1002"] = 2;
        data.Insert(new CustomerInfo("1003", 3));
        scoreCaches["1003"] = 3;
        data.Insert(new CustomerInfo("1004", 4));
        scoreCaches["1004"] = 4;
        data.Insert(new CustomerInfo("1005", 5));
        scoreCaches["1005"] = 5;

        renderString = ResultPage.Render($"Init: Top {topN}:\n", data.Top(topN));
    }

    var response = new Tuple<int, string>(200, renderString);
    return response;
});

server.POST(@"/customer/\d+/score/\d+", delegate(HttpListenerContext context)
{
    var input = context.Request.Url?.LocalPath;
    var pattern = @"\d+";
    var mc = Regex.Matches(input, pattern);
    var customerId = mc[0].Value;
    var score = Convert.ToInt64(mc[1].Value);
    var method = "Insert";
    var renderString = "";
    var topN = 10;

    lock (data)
    {
        if (scoreCaches.ContainsKey(customerId))
        {
            method = "Update";
            if (score != scoreCaches[customerId])
            {
                var info = new CustomerInfo(customerId, scoreCaches[customerId]);
                data.Update(info, new CustomerInfo(customerId, score));
            }
        }
        else
        {
            data.Insert(new CustomerInfo(customerId, score));
        }

        scoreCaches[customerId] = score;
        // Render TopN
        renderString = ResultPage.Render($"{method} succeed! Top {topN}:\n", data.Top(topN));
    }

    var response = new Tuple<int, string>(200, renderString);
    return response;
});

server.GET(@"/leaderboard", delegate(HttpListenerContext context)
{
    var query = context.Request.QueryString;
    Int64 start = 0;
    Int64 end = 10;
    if (!String.IsNullOrEmpty(query["start"]))
    {
        start = Convert.ToInt64(query["start"]);
    }

    if (!String.IsNullOrEmpty(query["end"]))
    {
        end = Convert.ToInt64(query["end"]);
    }

    var renderString = "";
    lock (data)
    {
        renderString = ResultPage.Render($"Leaderboard:{start}-{end}:\n", data.Range(start, end));
    }

    var response = new Tuple<int, string>(200, renderString);
    return response;
});

server.GET(@"/leaderboard/\d+", delegate(HttpListenerContext context)
{
    var input = context.Request.Url?.LocalPath;
    var pattern = @"\d+";
    var mc = Regex.Matches(input, pattern);
    var customerId = mc[0].Value;

    var query = context.Request.QueryString;
    Int64 high = 0;
    Int64 low = 0;
    if (!String.IsNullOrEmpty(query["high"]))
    {
        high = Convert.ToInt64(query["high"]);
    }

    if (!String.IsNullOrEmpty(query["low"]))
    {
        low = Convert.ToInt64(query["low"]);
    }

    var renderString = "";
    if (scoreCaches.ContainsKey(customerId))
    {
        var info = new CustomerInfo(customerId, scoreCaches[customerId]);
        lock (data)
        {
            // Render around
            renderString = ResultPage.RenderHighlight($"Leaderboard:: Customer:{customerId} (High:{high}-Low:{low}):\n",
                data.Around(info, high, low), customerId);
        }
    }
    else
    {
        renderString = ResultPage.RenderEmpty($"Leaderboard:: Customer:{customerId} (High:{high}-Low:{low}):\n");
    }

    var response = new Tuple<int, string>(200, renderString);
    return response;
});

server.DEFAULT(delegate(HttpListenerContext context)
{
    var response = new Tuple<int, string>(404, ResultPage.Render404());
    return response;
});

server.Start(8000);