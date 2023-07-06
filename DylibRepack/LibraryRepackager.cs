using System.Runtime.InteropServices;

namespace DylibRepack;

class LibraryRepackager
{
    [DllImport("libc", EntryPoint = "chmod", SetLastError = true)]
    private static extern int chmod(string path, uint mode);

    public static void Repackage(string[] libs, string outputPath, bool verify)
    {
        Directory.CreateDirectory(outputPath);
        var graph = BuildDependencyGraph(libs);
        var hashes = new Dictionary<string, string>();

        foreach(var item in graph)
        {
            var itemName = Path.GetFileName(item.Key);
            var data = File.ReadAllBytes(item.Key);
            var hash = Hash(data);
            if(hashes.TryGetValue(itemName, out var existing))
            {
                if (hash == existing)
                {
                    Console.WriteLine($"Skipping {item.Key}");
                    continue;
                }

                throw new InvalidOperationException($"Duplicate {itemName} with different hashes");
            }
            else
                hashes.Add(itemName, hash);

            Console.WriteLine($"Copying {item.Key}");
            var targetPath = Path.Combine(outputPath, itemName);
            File.WriteAllBytes(targetPath, data);

            chmod(targetPath, 0x000001FF);
            foreach (var dep in item.Value)
            {
                var depName = Path.GetFileName(dep);
                var newDep = "@loader_path/" + depName;
                Console.WriteLine($"Changing {dep} to {newDep}");
                ToolRunner.RunTool("install_name_tool", new[]
                {
                    "-change",
                    dep,
                    newDep,
                    targetPath
                });
            }
        }
        if (verify)
        {
            foreach (var lib in libs)
            {
                var path = Path.Combine(outputPath, Path.GetFileName(lib));
                Console.WriteLine("Verifying " + path);
                var libPtr = NativeLibrary.Load(path);
                if (libPtr == IntPtr.Zero)
                    throw new Exception("Not loaded");
            }
        }
    }


    static string Hash(byte[] data)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(data);
        return BitConverter.ToString(hash);
    }

    static Dictionary<string, List<string>> BuildDependencyGraph(string[] libs)
    {
        var dic = new Dictionary<string, List<string>>();
        var dependencyQueue = new Queue<string>();
        var visitedLibs = new HashSet<string>();
        foreach (var lib in libs)
            dependencyQueue.Enqueue(lib);
        while(dependencyQueue.Count > 0)
        {
            var currentLib = dependencyQueue.Dequeue();
            var deps = OToolDependencyParser.GetNonSystemDeps(currentLib);
            dic[currentLib] = deps.ToList();
            foreach(var dep in deps)
                if(visitedLibs.Add(dep))
                    dependencyQueue.Enqueue(dep);
        }
        return dic;
    }

}