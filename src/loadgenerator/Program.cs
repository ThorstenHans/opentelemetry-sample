public class Program
{

    private static string GetTargetHost(){
        var target = Environment.GetEnvironmentVariable("HOST");
        if (string.IsNullOrEmpty(target))
        {
            target = "service-a";
        }
        return target;

    }

    private static int GetPort(){
        var p = Environment.GetEnvironmentVariable("PORT");
        if (string.IsNullOrEmpty(p) || !int.TryParse(p, out var port))
        {
            port = 8080;
        }
        return port;
    }
    public static async Task Main(string[] args)
    {
        var target = GetTargetHost();
        var port = GetPort();
       
        while (true) {
        
            var think_time = new Random().Next(750, 2500);
            var number_of_requests = new Random().Next(1, 5);
            await SendRequests(number_of_requests, target, port);
            await WaitFor(think_time);
        }    
    }

    private static async Task SendRequests(int number_of_requests, string target, int port)
    {
        var tasks = new List<Task>();
        for (int i = 0; i < number_of_requests; i++)
        {
            Console.WriteLine("Sending Request...");
            var client = new HttpClient();
            tasks.Add(client.PostAsync($"http://{target}:{port}/ping", null));
        }
        await Task.WhenAll(tasks);
    }

    private static async Task WaitFor(int milliseconds)
    {
        Console.WriteLine("Hibernating for {0} ms...", milliseconds);
        await Task.Delay(milliseconds);
    }
}
