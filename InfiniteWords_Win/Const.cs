namespace InfiniteWords_Win;

public static class Const
{
    public const string Version = "v0.2.0";
    public const string Website = "https://github.com/Pditine/InfiniteWords";
#if DEBUG
    public const string Server  = "localhost";    
#else
    public const string Server = "172.81.247.80";
#endif
    
}