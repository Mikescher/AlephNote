namespace AlephNote.GitBackupService;

public class ColorConsole
{
    private const string ColorReset  = "\x1b[0m";
    private const string ColorRed    = "\x1b[31m";
    private const string ColorGreen  = "\x1b[32m";
    private const string ColorYellow = "\x1b[33m";
    private const string ColorBlue   = "\x1b[34m";
    private const string ColorPurple = "\x1b[35m";
    private const string ColorCyan   = "\x1b[36m";
    private const string ColorGray   = "\x1b[37m";
    private const string ColorWhite  = "\x1b[97m";

    private static readonly bool ColorOutput;
    
    static ColorConsole()
    {
        ColorOutput = !Console.IsOutputRedirected;
    }

    private static readonly ColorConsole _out = new ColorConsole(Console.Out);
    private static readonly ColorConsole _err = new ColorConsole(Console.Error);

    private readonly TextWriter tw;
    
    private ColorConsole(TextWriter tw)
    {
        this.tw = tw;
    }

    public static ColorConsole Out => _out;
    public static ColorConsole Error => _err;

    public void WriteLineDefault(string str) => tw.WriteLine(str);
    public void WriteLineRed(string str)     => tw.WriteLine((ColorOutput) ? (ColorRed    + str + ColorReset) : (str));
    public void WriteLineGreen(string str)   => tw.WriteLine((ColorOutput) ? (ColorGreen  + str + ColorReset) : (str));
    public void WriteLineYellow(string str)  => tw.WriteLine((ColorOutput) ? (ColorYellow + str + ColorReset) : (str));
    public void WriteLineBlue(string str)    => tw.WriteLine((ColorOutput) ? (ColorBlue   + str + ColorReset) : (str));
    public void WriteLinePurple(string str)  => tw.WriteLine((ColorOutput) ? (ColorPurple + str + ColorReset) : (str));
    public void WriteLineCyan(string str)    => tw.WriteLine((ColorOutput) ? (ColorCyan   + str + ColorReset) : (str));
    public void WriteLineGray(string str)    => tw.WriteLine((ColorOutput) ? (ColorGray   + str + ColorReset) : (str));
    public void WriteLineWhite(string str)   => tw.WriteLine((ColorOutput) ? (ColorWhite  + str + ColorReset) : (str));

}