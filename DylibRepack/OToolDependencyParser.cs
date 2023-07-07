namespace DylibRepack;

record DylibDependency(string VerbatimName, string ResolvedPath);
class OToolDependencyParser
{
    private static string[] Prefixes = new[] { "@rpath/", @"loader_path/" };
    public static IEnumerable<DylibDependency> GetDependencies(string library)
    {
        var result = ToolRunner.RunToolAndCheckSuccess("otool", new[]{"-L", library});
        foreach(var line in result.Stdout.Replace("\r","").Split("\n").Skip(2))
        {
            var dependency = line.TrimStart(' ', '\t').Split(' ')[0];
            if(string.IsNullOrWhiteSpace(dependency))
                yield break;
            var resolvedName = dependency;
            foreach (var prefix in Prefixes)
            {
                if (dependency.StartsWith(prefix))
                {
                    resolvedName = Path.Combine(Path.GetDirectoryName(library), dependency.Substring(prefix.Length));
                    break;
                }
            }
            yield return new(dependency, resolvedName);
        }

    }

    public static IEnumerable<DylibDependency> GetNonSystemDeps(string library)
    {
        foreach(var dep in GetDependencies(library))
        {
            if(dep.ResolvedPath.StartsWith("/usr/lib/") || dep.ResolvedPath.StartsWith("/System"))
                continue;
            yield return dep;
        }
    }
}