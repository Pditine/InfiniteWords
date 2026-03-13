using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace InfiniteWords_Server;

[JsonSerializable(typeof(List<string>))]
internal partial class AppJsonContext : JsonSerializerContext
{
}

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
#if DEBUG
        listener.Prefixes.Add($"http://localhost:{Port}/");
#else
        listener.Prefixes.Add($"http://*:{Port}/");
#endif
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
#if !DEBUG
        try
        {
#endif
            Console.WriteLine($"Received request: {context.Request.HttpMethod} {context.Request.Url}");
            var request = context.Request;
            var response = context.Response;

            var path = request.Url?.AbsolutePath.TrimEnd('/');

            if (path == "" || path == "/list")
            {
                // 列出 CSV 文件
                var files = Directory.GetFiles(DataPath, "*.csv");
                var fileNames = new List<string>();
                foreach (var file in files)
                {
                    fileNames.Add(Path.GetFileName(file));
                }

                var json = JsonSerializer.Serialize(fileNames, AppJsonContext.Default.ListString);
                var buffer = System.Text.Encoding.UTF8.GetBytes(json);

                response.ContentType = "application/json";
                response.ContentLength64 = buffer.Length;
                Console.WriteLine($"Responding with file list: {string.Join(", ", fileNames)}");
                await response.OutputStream.WriteAsync(buffer);
            }
            else if (path == "/download")
            {
                var filename = request.QueryString["filename"];
                if (string.IsNullOrEmpty(filename))
                {
                    response.StatusCode = 400; // 错误请求
                }
                else
                {
                    // 清理文件名以防止目录遍历和 CRLF 注入
                    filename = Path.GetFileName(filename); 
                    filename = filename.Replace("\r", "").Replace("\n", "");

                    var filePath = Path.Combine(DataPath, filename);
                    if (File.Exists(filePath))
                    {
                        var buffer = await File.ReadAllBytesAsync(filePath);
                        response.ContentType = "text/csv; charset=UTF-8"; // 显式指定 UTF-8
                        response.ContentLength64 = buffer.Length;
                        // response.AddHeader("Content-Disposition", $"attachment; filename=\"{filename}\"");
                        Console.WriteLine($"Responding with file: {filename} ({buffer.Length} bytes)");
                        await response.OutputStream.WriteAsync(buffer);
                    }
                    else
                    {
                        response.StatusCode = 404; // 未找到
                    }
                }
            }
            else
            {
                response.StatusCode = 404;
            }

            response.Close();
        }
#if !DEBUG
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing request: {ex.Message}");
        }
    }
#endif
}