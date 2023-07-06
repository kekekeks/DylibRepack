using System.Diagnostics;

namespace DylibRepack;

record class ToolResult(int Code, string Stdout, string Stderr);
class ToolRunner
{
    public static ToolResult RunToolAndCheckSuccess(string exe, IEnumerable<string> args)
    {
        var res = RunTool(exe, args);
        if(res.Code != 0)
            throw new Exception($"{exe} exited with code {res.Code}\n{res.Stderr}");
        return res;
    }

    public void RunToolNoRedirect(string exe, IEnumerable<string> args)
    {
        var info = new ProcessStartInfo
        {
            FileName = exe
        };
        foreach (var arg in args)
            info.ArgumentList.Add(arg);
        var proc = Process.Start(info);
        proc.WaitForExit();
        if (proc.ExitCode != 0)
            throw new Exception($"{exe} exited with code");
    }
    
    public static ToolResult RunTool(string exe, IEnumerable<string> args)
    {
        var info = new ProcessStartInfo
        {
            FileName = exe,
            RedirectStandardInput = true,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false
        };
        foreach (var arg in args)
            info.ArgumentList.Add(arg);
        var proc = Process.Start(info);
        proc.StandardInput.Close();
        var stdout = proc.StandardOutput.ReadToEndAsync();
        var stderr = proc.StandardError.ReadToEndAsync();
        proc.WaitForExit();
        return new ToolResult(proc.ExitCode, stdout.Result, stderr.Result);
    }
}