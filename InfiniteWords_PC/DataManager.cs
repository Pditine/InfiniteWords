using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;

namespace InfiniteWords_Win;

[JsonSerializable(typeof(List<string>))]
public partial class AppJsonContext : JsonSerializerContext
{
}

public static class DataManager
{
    private const string DataPath = "Data/";
    private static readonly Dictionary<string, WordContainer> WordsData = new();
    private static readonly object DataLock = new();
    public static event Action? OnDataUpdated;

    public static void LoadData()
    {
        lock (DataLock)
        {
            if (!System.IO.Directory.Exists(DataPath))
            {
                System.IO.Directory.CreateDirectory(DataPath);
            }
        
            var files = System.IO.Directory.GetFiles(DataPath, "*.csv");
            foreach (var file in files)
            {
                var name = System.IO.Path.GetFileNameWithoutExtension(file);
                var lines = System.IO.File.ReadAllLines(file);
                WordsData[name] = new WordContainer
                {
                    Name = name,
                    Words = ParseCsvLines(lines)
                };
            }
        }
    }

    public static async Task LoadRemoteDataAsync()
    {
        using var client = new HttpClient();
        
        client.BaseAddress = new Uri($"http://{Const.Server}:8080/");
        
        try 
        {
            var listJson = await client.GetStringAsync("list");
            var files = JsonSerializer.Deserialize(listJson, AppJsonContext.Default.ListString);
            if (files != null)
            {
                bool updated = false;
                foreach (var fileName in files)
                {
                    try 
                    {
                        var content = await client.GetStringAsync($"download?filename={fileName}");
                        var lines = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                        
                        var name = System.IO.Path.GetFileNameWithoutExtension(fileName);
                        lock (DataLock)
                        {
                            WordsData[name] = new WordContainer 
                            { 
                                Name = name, 
                                Words = ParseCsvLines(lines) 
                            };
                        }
                        updated = true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Failed to download {fileName}: {ex.Message}");
                    }
                }
                
                if (updated)
                {
                    OnDataUpdated?.Invoke();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load remote data: {ex.Message}");
        }
    }

    private static List<WordInfo> ParseCsvLines(string[] lines)
    {
        var words = new List<WordInfo>();
        foreach (var line in lines)
        {
            var parts = line.Split(',');
            if (parts.Length >= 2)
            {
                words.Add(new WordInfo
                {
                    Text = parts[0].Trim(),
                    Type = parts[1].Trim(),
                    Meaning = parts.Length > 2 ? parts[2].Trim() : string.Empty
                });
            }
        }
        return words;
    }
    
    public static List<string> GetCategories()
    {
        lock (DataLock)
        {
            return WordsData.Keys.ToList();
        }
    }
    
    public static WordContainer GetWordContainer(string category)
    {
        lock (DataLock)
        {
            return WordsData.TryGetValue(category, out var container) ? container : new WordContainer { Name = category, Words = new List<WordInfo>() };
        }
    }
}

public struct WordContainer
{
    public string Name;
    public List<WordInfo> Words;
}

public struct WordInfo
{
    public string Text;
    public string Type;
    public string Meaning;
}