using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using static System.Threading.WaitHandle;

namespace ConsoleApp1;

public class ResTfulService : IDisposable
{
    private readonly HttpListener _listener; // HTTP listener
    private readonly Thread _listenerThread; // thread for listen
    private readonly Queue<HttpListenerContext> _queue; // http request queue
    private readonly ManualResetEvent _stop, _ready; // status for: stop, ready
    private readonly Thread[] _workers; // work thread array
    private event Action<HttpListenerContext>? ProcessRequest;

    // URL Routes
    private readonly IDictionary<Regex, Func<HttpListenerContext, Tuple<int, string>>>?
        _getRoute = new Dictionary<Regex, Func<HttpListenerContext, Tuple<int, string>>>();

    private readonly IDictionary<Regex, Func<HttpListenerContext, Tuple<int, string>>>?
        _postRoute = new Dictionary<Regex, Func<HttpListenerContext, Tuple<int, string>>>();

    private readonly IDictionary<Regex, Func<HttpListenerContext, Tuple<int, string>>>?
        _putRoute = new Dictionary<Regex, Func<HttpListenerContext, Tuple<int, string>>>();

    private readonly IDictionary<Regex, Func<HttpListenerContext, Tuple<int, string>>>?
        _patchRoute = new Dictionary<Regex, Func<HttpListenerContext, Tuple<int, string>>>();

    private readonly IDictionary<Regex, Func<HttpListenerContext, Tuple<int, string>>>?
        _deleteRoute = new Dictionary<Regex, Func<HttpListenerContext, Tuple<int, string>>>();

    private readonly IDictionary<Regex, Func<HttpListenerContext, Tuple<int, string>>>
        _optionsRoute = new Dictionary<Regex, Func<HttpListenerContext, Tuple<int, string>>>();

    private readonly IDictionary<Regex, Func<HttpListenerContext, Tuple<int, string>>>
        _traceRoute = new Dictionary<Regex, Func<HttpListenerContext, Tuple<int, string>>>();

    private readonly IDictionary<Regex, Func<HttpListenerContext, Tuple<int, string>>>
        _headRoute = new Dictionary<Regex, Func<HttpListenerContext, Tuple<int, string>>>();

    private Func<HttpListenerContext, Tuple<int, string>>? _defaultRoute;

    // URL regex cache
    //private readonly IDictionary<string, Regex> _regexCaches = new Dictionary<string, Regex>();

    public ResTfulService(int maxThreads)
    {
        _workers = new Thread[maxThreads];
        _queue = new Queue<HttpListenerContext>();
        _stop = new ManualResetEvent(false);
        _ready = new ManualResetEvent(false);
        _listener = new HttpListener();
        _listenerThread = new Thread(HandleRequests);
        _defaultRoute = delegate
        {
            var response = new Tuple<int, string>(200, "404");
            return response;
        };
    }

    public void Dispose()
    {
        Stop();
    }

    public void GET(string url, Func<HttpListenerContext, Tuple<int, string>> method)
    {
        url = AdjustUrl(url);
        Regex regex = new Regex(url);
        //_regexCaches[url] = regex;
        _getRoute![regex] = method;
    }

    public void POST(string url, Func<HttpListenerContext, Tuple<int, string>> method)
    {
        url = AdjustUrl(url);
        Regex regex = new Regex(url);
        //_regexCaches[url] = regex;
        _getRoute![regex] = method;
    }

    public void PUT(string url, Func<HttpListenerContext, Tuple<int, string>> method)
    {
        url = AdjustUrl(url);
        Regex regex = new Regex(url);
        //_regexCaches[url] = regex;
        _getRoute![regex] = method;
    }

    public void PATCH(string url, Func<HttpListenerContext, Tuple<int, string>> method)
    {
        url = AdjustUrl(url);
        Regex regex = new Regex(url);
        //_regexCaches[url] = regex;
        _getRoute![regex] = method;
    }

    public void DELETE(string url, Func<HttpListenerContext, Tuple<int, string>> method)
    {
        url = AdjustUrl(url);
        Regex regex = new Regex(url);
        //_regexCaches[url] = regex;
        _getRoute![regex] = method;
    }

    public void OPTIONS(string url, Func<HttpListenerContext, Tuple<int, string>> method)
    {
        url = AdjustUrl(url);
        Regex regex = new Regex(url);
        //_regexCaches[url] = regex;
        _optionsRoute[regex] = method;
    }

    public void TRACE(string url, Func<HttpListenerContext, Tuple<int, string>> method)
    {
        url = AdjustUrl(url);
        Regex regex = new Regex(url);
        //_regexCaches[url] = regex;
        _traceRoute[regex] = method;
    }

    public void HEAD(string url, Func<HttpListenerContext, Tuple<int, string>> method)
    {
        url = AdjustUrl(url);
        Regex regex = new Regex(url);
        //_regexCaches[url] = regex;
        _headRoute[regex] = method;
    }

    private string AdjustUrl(string url)
    {
        if (String.IsNullOrEmpty(url))
            url = "/";
        if (url.Length > 1 && url[^1] == '/')
            url = url.Substring(0, url.Length - 1);
        if (url[^1] != '$')
            url = url + "$";
        return url;
    }

    public void Default(Func<HttpListenerContext, Tuple<int, string>>? method)
    {
        if (method != null)
            _defaultRoute = method;
    }

    private void ProcessHttpRequest(HttpListenerContext context)
    {
        Func<HttpListenerContext, Tuple<int, string>>? method = null;
        IDictionary<Regex, Func<HttpListenerContext, Tuple<int, string>>>? routeDict = null;

        string path = context.Request.Url!.LocalPath;

        if (path[^1] == '/')
            path = path.Substring(0, path.Length - 1);

        switch (context.Request.HttpMethod)
        {
            case "GET":
                routeDict = _getRoute;
                break;
            case "POST":
                routeDict = _postRoute;
                break;
            case "PUT":
                routeDict = _putRoute;
                break;
            case "PATCH":
                routeDict = _patchRoute;
                break;
            case "DELETE":
                routeDict = _deleteRoute;
                break;
            case "OPTIONS":
                routeDict = _optionsRoute;
                break;
            case "TRACE":
                routeDict = _traceRoute;
                break;
            case "HEAD":
                routeDict = _headRoute;
                break;
        }

        if (method == null)
        {
            foreach (var pair in routeDict!)
            {
                if (pair.Key.IsMatch(path))
                {
                    method = pair.Value;
                    break;
                }
            }
        }


        method ??= _defaultRoute;

        var response = method!(context);
        var buffer = Encoding.UTF8.GetBytes(response.Item2);
        context.Response.StatusCode = response.Item1;
        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        context.Response.OutputStream.Close();
        context.Response.Close();
    }

    public void Start(int port)
    {
        // register process
        ProcessRequest += ProcessHttpRequest;

        // start httpserver
        _listener.Prefixes.Add($"http://*:{port}/");
        //_listener.Prefixes.Add($"https://*:{port}/");
        _listener.Start();
        _listenerThread.Start();

        // start work threads
        for (var i = 0; i < _workers.Length; i++)
        {
            _workers[i] = new Thread(Worker);
            _workers[i].Start();
        }
    }


    public void Stop()
    {
        _stop.Set();
        _listenerThread.Join();
        foreach (var worker in _workers) worker.Join();
        _listener.Stop();
    }

    private void HandleRequests()
    {
        while (_listener.IsListening)
        {
            var context = _listener.BeginGetContext(ContextReady, null);
            if (0 == WaitAny(new[] { _stop, context.AsyncWaitHandle })) return;
        }
    }

    private void ContextReady(IAsyncResult result)
    {
        try
        {
            lock (_queue)
            {
                _queue.Enqueue(_listener.EndGetContext(result));
                _ready.Set();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"[RESTfulService::ContextReady]err:{e.Message}");
        }
    }

    private void Worker()
    {
      // ReSharper disable once InconsistentlySynchronizedField
      WaitHandle[] wait = { _ready, _stop };
        while (0 == WaitAny(wait))
        {
            HttpListenerContext context;
            lock (_queue)
            {
                if (_queue.Count > 0)
                {
                    context = _queue.Dequeue();
                }
                else
                {
                    _ready.Reset();
                    continue;
                }
            }

            try
            {
                ProcessRequest?.Invoke(context);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[RESTfulService::Worker]err:{e.Message}");
            }
        }
    }
}
