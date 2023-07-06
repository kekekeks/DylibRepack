namespace DylibRepack;

class OToolDependencyParser
{
    public static IEnumerable<string> GetDependencies(string library)
    {
        var result = ToolRunner.RunToolAndCheckSuccess("otool", new[]{"-L", library});
        foreach(var line in result.Stdout.Replace("\r","").Split("\n").Skip(2))
        {
            var dependency = line.TrimStart(' ', '\t').Split(' ')[0];
            if(string.IsNullOrWhiteSpace(dependency))
                yield break;
            yield return dependency;
        }

    }

    public static IEnumerable<string> GetNonSystemDeps(string library)
    {
        foreach(var dep in GetDependencies(library))
        {
            if(dep.StartsWith("/usr/lib/") || dep.StartsWith("/System"))
                continue;
            yield return dep;
        }
    }
}