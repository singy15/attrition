/*
 * Attrition Issue Tracking System
 */

/*
 * TASK STATUS
 *
 * OPEN    : *
 * WIP     : >
 * PENDING : ?
 * ABORT   : x
 * CLOSED  : -
 */

using System;
using System.Reflection;
using System.Linq;
using System.Diagnostics;
using System.Runtime.Serialization.Json;
using System.Runtime;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;

public class AISMain
{
    public static int Main(string[] args)
    {
        AIS atmt = new AIS();
        atmt.Initialize();

        if((args.Count() > 0) && (args[0] == "i"))
        {
            while(true)
            {
                Console.Write("> ");
                string input = Console.ReadLine();
                if(input == "exit")
                {
                    return 0;
                }
                atmt.Exec(input.Split(new string[] { " " },
                            StringSplitOptions.None | StringSplitOptions.RemoveEmptyEntries));
            }
        }
        else
        {
            return atmt.Exec(args);
        }
    }
}

public class AIS
{
    public static string NEWLINE = "\r\n";

    private AISInterface ifs;
    private AISService svc;

    public void Initialize()
    {
        Console.OutputEncoding = new UTF8Encoding();
        Console.InputEncoding = new UTF8Encoding();
        svc = new AISService();
        ifs = new AISInterface(svc);
    }

    public int Exec(string[] args)
    {
        int result = 0;

        svc.LoadOrCreateConfig();
        svc.RestoreOrCreateDB();

        if(args.Length > 0)
        {
            MethodInfo method = ifs.GetType()
                .GetMethod(AISStringUtil.KebabToPascal(args[0]));

            if(null != method)
            {
                try
                {
                    result = (int)method.Invoke(ifs, new Object[]{ args });
                }
                catch(Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            else
            {
                Console.WriteLine("ERROR : Command not found!");
                ifs.Help(args);
            }
        }
        else
        {
            ifs.Help(args);
        }

        svc.DumpDB();

        return result;
    }

    public string UppercaseFirst(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return string.Empty;
        }
        return char.ToUpper(s[0]) + s.Substring(1);
    }
}

public class AISInterface
{
    private AISService svc;

    public AISInterface(AISService svc)
    {
        this.svc = svc;
    }

    public int Help(string[] args) { svc.Help(args); return 0; }
    public int List(string[] args) { svc.List(args); return 0; }
    public int Ls(string[] args) { svc.List(args); return 0; }
    public int ListAll(string[] args) { svc.ListAll(args); return 0; }
    public int LsAll(string[] args) { svc.ListAll(args); return 0; }
    public int Show(string[] args) { svc.Show(args); return 0; }
    public int Add(string[] args) { svc.Add(args); return 0; }
    public int Del(string[] args) { svc.Del(args); return 0; }
    public int Archive(string[] args) { svc.Archive(args); return 0; }
    public int Mod(string[] args) { svc.Mod(args); return 0; }
    public int Find(string[] args) { svc.Find(args); return 0; }
    public int Cls(string[] args) { svc.Cls(args); return 0; }
    public int Get(string[] args) { svc.Get(args); return 0; }
    public int Set(string[] args) { svc.Set(args); return 0; }
    public int New(string[] args) { return svc.New(args); }
    public int Ins(string[] args) { svc.Ins(args); return 0; }
    public int Test(string[] args) { svc.Test(args); return 0; }
}

public class AISService
{
    private string defaultDbPath;
    private string defaultConfigPath;
    string tmpTxtPath;
    string vimrcUtf8FileName;
    private AISDB db;
    private AISConfig config;

    public AISService()
    {
        string basePath = System.AppDomain.CurrentDomain.BaseDirectory;
        defaultDbPath = basePath + @"db.json";
        tmpTxtPath = basePath + @"tmp";
        vimrcUtf8FileName = basePath + @".vimrc_utf8";
        defaultConfigPath = basePath + @"config.json";
    }

    public void Test(string[] args)
    {
    }

    public void CreateDB()
    {
        db = new AISDB();
        DumpDB();
    }

    public void DumpDB()
    {
        File.WriteAllText(defaultDbPath, AISJsonUtility.Serialize(db),
                Encoding.GetEncoding(932));
    }

    public void RestoreDB()
    {
        db = AISJsonUtility.Deserialize<AISDB>(
                File.ReadAllText(defaultDbPath, Encoding.GetEncoding(932)));
    }

    public void RestoreOrCreateDB()
    {
        if(!(File.Exists(defaultDbPath)))
        {
            CreateDB();
        }

        RestoreDB();
    }

    public void InsertTestData()
    {
        for(int i = 0; i < 3; i++)
        {
            Task t = new Task();
            t.Id = i;
            t.Name = "task " + i.ToString();
            t.Desc = "a task " + i.ToString() + " \nfoo bar\nbaz";
            db.Task.Add(t);
        }
    }

    public void Help(string[] args)
    {
        Console.WriteLine("usage: ais <command> <args>");
        PrintCommandDesc("help", "List help.");
    }

    public void Launch(string[] args)
    {
        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = args[1];
        if(args.Length > 2)
        {
            psi.Arguments = string.Join(" ", args.Skip(2).ToArray());
        }
        Process.Start(psi);
    }

    private void PrintCommandDesc(string commandName, string description)
    {
        Console.WriteLine(
                String.Format("{0, -20}", "    " + commandName) + description);
    }

    public void ClearConsole()
    {
        Console.WriteLine("\u001b[2J");
        Console.WriteLine("\u001b[0;0H");
    }

    public string Ansi(string sq, string s)
    {
        if(config.enableAnsiEsc)
        {
            return String.Format("\u001b[{0}m{1}\u001b[0m", sq, s);
        }
        else
        {
            return s;
        }
    }

    private string AnsiUnderline(string s)
    {
        if(config.enableAnsiEsc)
        {
            return String.Format("\u001b[4m{0}\u001b[0m", s);
        }
        else
        {
            return s;
        }
    }

    public string AnsiReset()
    {
        if(config.enableAnsiEsc)
        {
            return "\u001b[0m";
        }
        else
        {
            return "";
        }
    }

    public string AnsiStrike()
    {
        if(config.enableAnsiEsc)
        {
            return "\u001b[9m";
        }
        else
        {
            return "";
        }
    }

    private void ShowSub(Task t, bool showAll = false)
    {
        bool isWIP = t.Status == Task.StatusNameToCode(">");

        Console.WriteLine(
                Ansi("33" + (isWIP ? ";1" : ""), String.Format("{0, -1}", "#" + t.Id.ToString())) + " "
                + Ansi("35" + (isWIP ? ";1" : ""), Task.StatusCodeToName(t.Status))
                + " "
                + ((t.IsArchived)? "[A] " : "")
                + ((t.Status == Task.StatusNameToCode("-"))? AnsiStrike() : "")
                + Ansi("36" + (isWIP ? ";1" : ""), t.Name)
                + AnsiReset());
        if(t.Desc != "")
        {
            string[] lines = t.Desc.Split(new string[] { "\r\n" }
                    , StringSplitOptions.None);
            foreach(string l in lines)
            {
                bool isComment = Regex.IsMatch(l, @"^;.*$");
                if(!showAll && isComment) continue;
                Console.WriteLine("  " + ((isComment)? Ansi("32", l) : l));
            }
        }
        Console.WriteLine("");
    }

    public void Show(string[] args)
    {
        CheckNumArgumentsEqual(args, 2);
        ShowSub(Task.SelectById(db, Int32.Parse(args[1])), true);
    }

    public void List(string[] args)
    {
        int cnt = (args.Count() == 2)? Int32.Parse(args[1]) : 50;
        db.Task
            .Where(x => !(x.IsArchived))
            .OrderBy(t => t.Status)
            .ThenByDescending(t => t.Priority)
            .ThenBy(t => t.Id)
            .Take(cnt)
            .ToList()
            .ForEach(x => ShowSub(x));
    }

    public void ListAll(string[] args)
    {
        int cnt = (args.Count() == 2)? Int32.Parse(args[1]) : 50;
        db.Task
            .OrderBy(t => t.Id)
            .Take(cnt)
            .ToList()
            .ForEach(x => ShowSub(x));
    }

    private Task CreateNew()
    {
        Task t = new Task();

        t.Id = db.SequenceTask.GetSeq();
        t.Status = Task.StatusNameToCode("*");
        t.Name = "<name>";
        t.Desc = "# <description>";

        return t;
    }

    public void Add(string[] args)
    {
        Task t = CreateNew();

        string input = InputWithVimUTF8(t);

        if(null == input)
        {
            return;
        }

        t.LoadDescriptor(input, true);
        string dt = DateTimeOffset.Now.ToString(Task.DATETIME_FORMAT);
        t.Created = dt;
        t.Updated = dt;

        db.Task.Add(t);

        ShowSub(t);
    }

    public void Del(string[] args)
    {
        CheckNumArgumentsMin(args, 2);


        Console.Write("Delete #" + string.Join(",", args.Skip(1).ToArray()) + ". Are you sure? (Y/n): ");
        string confirm = Console.ReadLine();
        if(confirm.ToLower() == "n")
        {
            return;
        }

        foreach(string arg in args.Skip(1).ToArray())
        {
            Task tgt = Task.SelectById(db, Int32.Parse(arg));

            if(tgt != null)
            {
                db.Task.Remove(tgt);
            }
        }
    }

    public void Archive(string[] args)
    {
        CheckNumArgumentsMin(args, 2);

        args
            .Skip(1)
            .ToList()
            .ForEach(arg => db.Task
                    .Where(x => x.Id == Int32.Parse(arg))
                    .ToList()
                    .ForEach(x => x.IsArchived = !(x.IsArchived)));
    }

    public void Mod(string[] args)
    {
        CheckNumArgumentsEqual(args, 2);

        Task tgt = Task.SelectById(db, Int32.Parse(args[1]));

        string input = InputWithVimUTF8(tgt);

        if(null == input)
        {
            return;
        }

        List<string> lines = input.Split(
                new string[] { "\r\n" },
                StringSplitOptions.None).ToList();

        tgt.LoadDescriptor(input, true);
        tgt.Updated = DateTimeOffset.Now.ToString(Task.DATETIME_FORMAT);

        // ShowSub(tgt);
    }

    public void Find(string[] args)
    {
        CheckNumArgumentsEqual(args, 2);

        db.Task
            .Where(x =>
                Regex.IsMatch(x.GetDescriptor(false), @".*" + args[1] + ".*",
                    RegexOptions.IgnoreCase | RegexOptions.Singleline))
            .ToList()
            .ForEach(x => ShowSub(x));
    }

    public void Cls(string[] args)
    {
        ClearConsole();
    }

    public void Get(string[] args)
    {
        CheckNumArgumentsEqual(args, 2);
        Console.Write(Task
                .SelectById(db, Int32.Parse(args[1]))
                .GetDescriptor(false));
    }

    public void Set(string[] args)
    {
        CheckNumArgumentsEqual(args, 2);
        Task.SelectById(db, Int32.Parse(args[1]))
            .LoadDescriptor(Console.In.ReadToEnd(), true);
    }

    public int New(string[] args)
    {
        Task t = CreateNew();
        Console.Write(t.GetDescriptor(false));
        return t.Id;
    }

    public void Ins(string[] args)
    {
        Task t = new Task();
        t.LoadDescriptor(Console.In.ReadToEnd(), true);
        t.Id = Int32.Parse(args[1]);

        string dt = DateTimeOffset.Now.ToString(Task.DATETIME_FORMAT);
        t.Created = dt;
        t.Updated = dt;

        db.Task.Add(t);
    }

    public void LoadOrCreateConfig()
    {
        if(!(File.Exists(defaultConfigPath)))
        {
            config = new AISConfig();

            // Default config.
            config.enableAnsiEsc = true;

            File.WriteAllText(defaultConfigPath, AISJsonUtility.Serialize(config),
                    Encoding.GetEncoding(932));
        }
        else
        {
            config = AISJsonUtility.Deserialize<AISConfig>(
                    File.ReadAllText(defaultConfigPath, Encoding.GetEncoding(932)));
        }
    }

    private void CheckNumArgumentsEqual(string[] args, int num)
    {
        if(args.Count() != num) {
            throw new Exception("Invalid number of arguments :"
                    + " Expect " + num.ToString()
                    + " Actual " + args.Count());
        }
    }

    private void CheckNumArgumentsMin(string[] args, int min)
    {
        if(!(args.Count() >= min)) {
            throw new Exception("Invalid number of arguments");
        }
    }

    private string InputWithNotepad()
    {
        // Prepare temporary text file.
        File.WriteAllText(tmpTxtPath, "", Encoding.GetEncoding(932));

        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = @"notepad";
        psi.Arguments = tmpTxtPath;
        psi.UseShellExecute = true;

        Process p = Process.Start(psi);
        p.WaitForExit();

        return File.ReadAllText(tmpTxtPath, Encoding.GetEncoding(932));
    }

    private string InputWithVimUTF8(Task t)
    {
        File.WriteAllText(tmpTxtPath, t.GetDescriptor(false), Encoding.GetEncoding(65001));

        var startinfo = new ProcessStartInfo("vim")
        {
            CreateNoWindow = true,
            Arguments = " --not-a-term" + " -u " + vimrcUtf8FileName + " " + tmpTxtPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        var process = new Process { StartInfo = startinfo };
        process.Start();

        var reader = process.StandardOutput;
        // Console.WriteLine(reader.CurrentEncoding.EncodingName);
        // Console.WriteLine(reader.CurrentEncoding.CodePage);
        while (!reader.EndOfStream)
        {
            int nextLine = reader.Read();
            Console.Write((char)nextLine);
        }

        process.WaitForExit();

        if(process.ExitCode == 1)
        {
            return null;
        }

        return File.ReadAllText(tmpTxtPath, Encoding.GetEncoding(65001));
    }
}

public class AISDB
{
    public List<Task> Task { get; set; }
    public Sequence SequenceTask { get; set; }

    public AISDB()
    {
        Task = new List<Task>();
        SequenceTask = new Sequence();
    }
}

public class EditableProperty : Attribute
{
    public EditableProperty() {}
}

public class Task
{
    public static string DATETIME_FORMAT = "yyyy/MM/dd hh:mm";

    public int Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public string Desc { get; set; }
    public bool IsArchived { get; set; }

    [EditableProperty]
    public string Priority { get; set; }

    [EditableProperty]
    public string Delivery { get; set; }

    [EditableProperty]
    public string PlanSt { get; set; }

    [EditableProperty]
    public string PlanEd { get; set; }

    [EditableProperty]
    public string ActualSt { get; set; }

    [EditableProperty]
    public string ActualEd { get; set; }

    public string Created { get; set; }
    public string Updated { get; set; }

    public static string StatusNameToCode(string name) {
        if(name == ">")
        {
            return "0";
        }
        if(name == "*")
        {
            return "1";
        }
        if(name == "?")
        {
            return "2";
        }
        if(name == "x")
        {
            return "3";
        }
        if(name == "-")
        {
            return "4";
        }

        return "0";
    }

    public static string StatusCodeToName(string code) {
        if(code == "0")
        {
            return ">";
        }
        if(code == "1")
        {
            return "*";
        }
        if(code == "2")
        {
            return "?";
        }
        if(code == "3")
        {
            return "x";
        }
        if(code == "4")
        {
            return "-";
        }

        return " ";
    }

    public string GetDescriptor(bool withUsage)
    {
        string properties =
             "###" + AIS.NEWLINE
            + this.GetType()
                .GetProperties()
                .Where(p => null != Attribute.GetCustomAttribute(p, typeof(EditableProperty)))
                .Aggregate("", (m, e) =>
                    m + String.Format("# @{0} {1}{2}",
                            AISStringUtil.PascalToKebab(e.Name),
                            (string)e.GetValue(this),
                            AIS.NEWLINE))
            + "###" + AIS.NEWLINE;

        string usage = String.Format(
                "###   Task descriptor format{0}"
                + "###   #<id> <status> <name>{0}"
                + "###   {0}"
                + "###   <description>{0}"
                + "###   <description>{0}",
                AIS.NEWLINE);

        return String.Format(
                "#{1} {2} {3}{0}"
                + "{0}"
                + "{6}"
                + "{4}{0}"
                + "{5}",
                AIS.NEWLINE,
                Id,
                Task.StatusCodeToName(Status),
                Name,
                Desc,
                (withUsage)? usage : "",
                properties);
    }

    private static Task ParseDescriptor(string descriptor, bool trimLast)
    {
        Task t = new Task();

        List<string> lines = descriptor.Split(
                new string[] { "\r\n" },
                StringSplitOptions.None).ToList();

        Regex regex = new Regex( @"\#(.+?) (.+?) ([\s\S]+)$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline);

        MatchCollection mc = regex.Matches(lines[0]);
        Match m = mc[0];

        t.Id = Int32.Parse(m.Groups[1].Value);
        t.Status = Task.StatusNameToCode(m.Groups[2].Value);
        t.Name = m.Groups[3].Value;

        if(lines.Count() > 3)
        {
            if(trimLast)
            {
                lines.RemoveAt(lines.Count - 1);
            }
            List<string> tmpLine = lines.Skip(2).ToList();

            List<string> tmpLine2 = new List<string>();

            foreach(string s in tmpLine)
            {
                if((Regex.IsMatch(s, @"^#.*$")))
                {
                    t.ReadIfPropertySetter(s);
                }
                else
                {
                    tmpLine2.Add(s);
                }
            }

            t.Desc = String.Join("\r\n", tmpLine2);
        } else
        {
            t.Desc = "";
        }

        return t;
    }

    private void ReadIfPropertySetter(string l)
    {
        string p = "^\\# @(.+?) (.*)$";
        if(Regex.IsMatch(l, p))
        {
            Match m = new Regex(p,
                    RegexOptions.IgnoreCase
                    | RegexOptions.Singleline)
                .Matches(l)[0];
            string name = m.Groups[1].Value;
            string val = (m.Groups.Count > 2)? m.Groups[2].Value.Trim() : null;
            if(val == null) return;

            var prop = this.GetType()
                .GetProperty(AISStringUtil.KebabToPascal(name));

            if(null != prop)
            {
                prop.SetValue(this, val);
            }
        }
    }

    public void LoadDescriptor(string descriptor, bool trimLast)
    {
        Task t = Task.ParseDescriptor(descriptor, trimLast);
        this.Name = t.Name;
        this.Status = t.Status;
        this.Desc = t.Desc;

        this.GetType()
            .GetProperties()
            .Where(p => null != Attribute.GetCustomAttribute(p, typeof(EditableProperty)))
            .ToList()
            .ForEach(e => e.SetValue(this, (string)e.GetValue(t)));
    }

    public static Task SelectById(AISDB db, int id)
    {
        return db.Task.FirstOrDefault(x => x.Id == id);
    }
}

public class Sequence
{
    public string Type { get; set; }
    public int Seq { get; set; }

    public int GetSeq() {
        Seq = Seq + 1;
        return Seq;
    }
}

public class AISConfig
{
    public bool enableAnsiEsc { get; set; }
}

public class AISJsonUtility
{
    public static string Serialize(object o)
    {
        using (var stream = new MemoryStream())
        {
            var serializer = new DataContractJsonSerializer(o.GetType());
            serializer.WriteObject(stream, o);
            return Encoding.UTF8.GetString(stream.ToArray());
        }
    }

    public static T Deserialize<T>(string str)
    {
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(str)))
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            return (T)serializer.ReadObject(stream);
        }
    }
}

public class AISStringUtil
{
    public static string KebabToPascal(string kebab)
    {
        return kebab
            .Split(new [] {"-"}, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => char.ToUpperInvariant(s[0])
                    + s.Substring(1, s.Length - 1))
            .Aggregate(string.Empty, (s1, s2) => s1 + s2);
    }

    public static string PascalToKebab(string pascal)
    {
        if (string.IsNullOrEmpty(pascal))
            return pascal;

        return Regex.Replace(
            pascal,
            "(?<!^)([A-Z][a-z]|(?<=[a-z])[A-Z])",
            "-$1",
            RegexOptions.Compiled)
            .Trim()
            .ToLower();
    }
}

