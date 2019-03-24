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
using System.IO;
using System.Collections.Generic;

public class AISMain
{
    public static void Main(string[] args) 
    {
        AIS atmt = new AIS();
        atmt.Initialize();
        atmt.Exec(args);
    }
}

public class AIS
{
    private AISInterface ifs;
    private AISService svc;

    public void Initialize()
    {
        Console.OutputEncoding = new UTF8Encoding();
        svc = new AISService();
        ifs = new AISInterface(svc);
    }

    public void Exec(string[] args)
    {
        svc.RestoreOrCreateDB();

        if(args.Length > 0) 
        {
            MethodInfo method = ifs.GetType()
                .GetMethod(KebabToPascal(args[0]));

            if(null != method)
            {
                try 
                {
                    method.Invoke(ifs, new Object[]{ args });
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
    }

    public string UppercaseFirst(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return string.Empty;
        }
        return char.ToUpper(s[0]) + s.Substring(1);
    }

    public static string KebabToPascal(string kebab)
    {
        return kebab
            .Split(new [] {"-"}, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => char.ToUpperInvariant(s[0]) 
                    + s.Substring(1, s.Length - 1))
            .Aggregate(string.Empty, (s1, s2) => s1 + s2);
    }
}

public class AISInterface
{
    private AISService svc;

    public AISInterface(AISService svc) 
    {
        this.svc = svc;
    }

    public void Help(string[] args)
    {
        svc.Help(args);
    }

    public void Launch(string[] args) 
    {
        svc.Launch(args);
    }

    public void List(string[] args)
    {
        svc.List(args);
    }

    public void ListAll(string[] args)
    {
        svc.ListAll(args);
    }

    public void Add(string[] args)
    {
        svc.Add(args);
    }

    public void Del(string[] args)
    {
        svc.Del(args);
    }

    public void Archive(string[] args)
    {
        svc.Archive(args);
    }

    public void Mod(string[] args)
    {
        svc.Mod(args);
    }

    public void Test(string[] args)
    {
        svc.Test(args);
    }
}

public class AISService
{
    private string defaultDbPath;
    string tmpTxtPath;
    string vimrcUtf8FileName;
    private AISDB db;

    public AISService()
    {
        string basePath = System.AppDomain.CurrentDomain.BaseDirectory;
        defaultDbPath = basePath + @"db.json";
        tmpTxtPath = basePath + @"tmp";
        vimrcUtf8FileName = basePath + @".vimrc_utf8";
    }

    public void Test(string[] args)
    {
        CreateDB();
        InsertTestData();
        DumpDB();
    }

    public void CreateDB()
    {
        db = new AISDB();
        DumpDB();
    }

    public void DumpDB()
    {
        File.WriteAllText(defaultDbPath, AISJsonUtility.Serialize(db), Encoding.GetEncoding(932));
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

    private void ShowSub(Task t)
    {
        Console.WriteLine(
                "#" + t.Id + " " 
                + t.Status + " " 
                + ((t.IsArchived)? "[A] " : "") 
                + t.Name);
        if(t.Desc != "")
        {
            string[] lines = t.Desc.Split(new string[] { "\r\n" }
                    , StringSplitOptions.None);
            foreach(string l in lines)
            {
                Console.WriteLine("  " + l);
            }
        }
        Console.WriteLine("");
    }

    public void Show(string[] args)
    {
    }

    public void List(string[] args)
    {
        foreach(Task t in db.Task)
        {
            if(!t.IsArchived)
            {
                ShowSub(t);
            }
        }
    }

    public void ListAll(string[] args)
    {
        foreach(Task t in db.Task)
        {
            ShowSub(t);
        }
    }

    public void Add(string[] args)
    {
        Task t = new Task();
        
        t.Id = db.SequenceTask.GetSeq();
        t.Name = "{Name}";
        t.Desc = "{Desc}";
        t.Status = "*";

        List<string> lines = InputWithVimUTF8(t).Split(new string[] { "\r\n" }, 
                StringSplitOptions.None).ToList();

        t.Name = lines[0];

        if(lines.Count() > 3) 
        {
            // Vim inserts CRLF in tail of last line.
            lines.RemoveAt(lines.Count - 1);
            t.Desc = String.Join("\r\n", lines.Skip(2).ToArray());
        } else 
        {
            t.Desc = "";
        }

        // Console.WriteLine(t.Name);
        // Console.WriteLine();
        // Console.WriteLine(t.Desc);
        ShowSub(t);

        db.Task.Add(t);
    }

    public void Del(string[] args)
    {
        CheckNumArgumentsMin(args, 2);

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

        foreach(string arg in args.Skip(1).ToArray())
        {
            Task tgt = Task.SelectById(db, Int32.Parse(arg));

            if(tgt != null)
            {
                tgt.IsArchived = !tgt.IsArchived;
            }
        }
    }

    public void Mod(string[] args)
    {
        CheckNumArgumentsEqual(args, 2);

        Task tgt = Task.SelectById(db, Int32.Parse(args[1]));

        List<string> lines = InputWithVimUTF8(tgt).Split(
                new string[] { "\r\n" }, 
                StringSplitOptions.None).ToList();

        tgt.Name = lines[0];

        if(lines.Count() > 3) 
        {
            // Vim inserts CRLF in tail of last line.
            lines.RemoveAt(lines.Count - 1);
            tgt.Desc = String.Join("\r\n", lines.Skip(2).ToArray());
        } else 
        {
            tgt.Desc = "";
        }

        ShowSub(tgt);
    }

    private void CheckNumArgumentsEqual(string[] args, int num)
    {
        if(args.Count() != num) {
            throw new Exception("Invalid number of arguments");
        }
    }

    private void CheckNumArgumentsMin(string[] args, int min)
    {
        if(args.Count() <= min) {
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
        File.WriteAllText(tmpTxtPath, t.GetDescriptor(), Encoding.GetEncoding(65001));

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

public class Task
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Status { get; set; }
    public string Desc { get; set; }
    public bool IsArchived { get; set; }
    public string PlDelivSt { get; set; }
    public string PlDelivEd { get; set; }
    public string AcDelivSt { get; set; }
    public string AcDelivEd { get; set; }
    public double PlWL { get; set; }
    public double AcWL { get; set; }

    public string GetDescriptor() 
    {
        string NEWLINE = "\r\n";
        string d = "";
        d += Name + NEWLINE;
        d += NEWLINE;
        d += Desc;
        return d;
    }

    public static Task SelectById(AISDB db, int id)
    {
        foreach(Task t in db.Task)
        {
            if(t.Id == id)
            {
                return t;
            }
        }

        return null;
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

