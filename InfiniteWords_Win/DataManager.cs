using System.Collections.Generic;
using System.Linq;

namespace InfiniteWords_Win;

public static class DataManager
{
    private const string DataPath = "Data/";
    private static readonly Dictionary<string, WordContainer> WordsData = new();

    public static void LoadData()
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
                        Meaning = parts[2].Trim()
                    });
                }
                else
                {
                    // todo: debug
                }
            }
            WordsData[name] = new WordContainer
            {
                Name = name,
                Words = words
            };
        }
    }
    
    public static List<string> GetCategories()
    {
        return WordsData.Keys.ToList();
    }
    
    public static WordContainer GetWordContainer(string category)
    {
        return WordsData.TryGetValue(category, out var container) ? container : new WordContainer { Name = category, Words = new List<WordInfo>() };
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