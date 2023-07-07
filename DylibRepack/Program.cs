using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using CommandLine;

namespace DylibRepack;

static class Program
{
    [Verb("repack")]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    class Repack
    {
        [Option('o', "output", Required = true)]
        public string OutputPath { get; set; }
        
        [Option("verify")]
        public bool Verify { get; set; }
        
        [Value(0)]
        public IEnumerable<string> Libs { get; set; }
    }

    [Verb("verify")]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    class VerifyArgs
    {
        [Value(0)]
        public string Path { get; set; }
    }
    
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Repack))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(VerifyArgs))]
    public static void Main(string[] args)
    {
        CommandLine.Parser.Default.ParseArguments<Repack, VerifyArgs>(args)
            .WithParsed<Repack>(r => LibraryRepackager.Repackage(r.Libs.ToArray(), r.OutputPath, r.Verify))
            .WithParsed<VerifyArgs>(v => NativeLibrary.Load(v.Path));
    }
}