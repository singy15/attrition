/*
 * Attrition Command Interface
 */

using System;
using System.Reflection;
using System.Linq;
using System.Diagnostics;

public class ACIMain
{
    public static void Main(string[] args) 
    {
        ACIService svc = new ACIService();
        svc.Exec(args);

        // Create object by name.
        // Type type = Type.getType("ACIInterface");
        // object o = Activator.CreateInstance(type);
        // MethodInfo method = o.GetType().GetMethod("Help");
        // method.Invoke(o, new Object[]{ args });
    }
}

public class ACIService
{
    public void Exec(string[] args)
    {
        ACIInterface cmd = new ACIInterface();
        if(args.Length > 0) 
        {
            MethodInfo method = cmd.GetType().GetMethod(KebabToPascal(args[0]));
            if(null != method)
            {
                try 
                {
                    method.Invoke(cmd, new Object[]{ args });
                } 
                catch(Exception e) 
                {
                    Console.WriteLine(e);
                }
            }
            else 
            {
                Console.WriteLine("ERROR : Command not found!");
                cmd.Help(args);
            }
        }
        else 
        {
            cmd.Help(args);
        }
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
            .Select(s => char.ToUpperInvariant(s[0]) + s.Substring(1, s.Length - 1))
            .Aggregate(string.Empty, (s1, s2) => s1 + s2);
    }
}

// Add commands here.
public class ACIInterface
{
    private ACICommandCore cmdCore = new ACICommandCore();

    public void Help(string[] args)
    {
        cmdCore.Help(args);
    }

    public void Launch(string[] args) 
    {
        cmdCore.Launch(args);
    }
}

public class ACICommandCore
{
    public void Help(string[] args)
    {
        Console.WriteLine("usage: ac <command> <args>");
        PrintCommandDesc("help", "Show help.");
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
}

