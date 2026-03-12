using System.Net;
using System.Text.Json;

namespace InfiniteWords_Server;

internal class Program
{
    private const int Port = 8080;
    private static readonly string DataPath = Path.Combine(Directory.GetCurrentDirectory(), "Data");

    static async Task Main(string[] args)
    {
        if (!Directory.Exists(DataPath))
        {
            Directory.CreateDirectory(DataPath);
        }

        using var listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{Port}/");
        listener.Start();

        Console.WriteLine($"Server started on port {Port}");
        Console.WriteLine($"Data directory: {DataPath}");

        while (true)
        {
            var context = await listener.GetContextAsync();
            _ = ProcessRequest(context);
        }
    }

    private static async Task ProcessRequest(HttpListenerContext context)
    {
        try
        {
            var request = context.Request;
            var response = context.Response;

            var path = request.Url?.AbsolutePath.TrimEnd('/');

            if (path == "" || path == "/list")
            {
                // List CSV files
                var files = Directory.GetFiles(DataPath, "*.csv");
                var fileNames = new List<string>();
                foreach (var file in files)
                {
                    fileNames.Add(Path.GetFileName(file));
                }

                var json = JsonSerializer.Serialize(fileNames);
                var buffer = System.Text.Encoding.UTF8.GetBytes(json);

                response.ContentType = "application/json";
                response.ContentLength64 = buffer.Length;
                await response.OutputStream.WriteAsync(buffer);
            }
            else if (path == "/download")
            {
                var filename = request.QueryString["filename"];
                if (string.IsNullOrEmpty(filename))
                {
                    response.StatusCode = 400; // Bad Request
                }
                else
                {
                    var filePath = Path.Combine(DataPath, filename);
                    if (File.Exists(filePath))
                    {
                        var buffer = await File.ReadAllBytesAsync(filePath);
                        response.ContentType = "text/csv; charset=UTF-8"; // Explicit UTF-8
                        response.ContentLength64 = buffer.Length;
                        response.AddHeader("Content-Disposition", $"attachment; filename=\"{filename}\"");
                        await response.OutputStream.WriteAsync(buffer);
                    }
                    else
                    {
                        response.StatusCode = 404; // Not Found
                    }
                }
            }
            else
            {
                response.StatusCode = 404;
            }

            response.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing request: {ex.Message}");
        }
    }
}
