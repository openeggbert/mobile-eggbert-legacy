using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Xna.Framework.Content.Pipeline.Tasks;

internal sealed class ConsoleBuildEngine : IBuildEngine
{
    public bool ContinueOnError { get { return false; } }
    public int LineNumberOfTaskNode { get { return 0; } }
    public int ColumnNumberOfTaskNode { get { return 0; } }
    public string ProjectFileOfTaskNode { get { return "speedy-blupi-xna4"; } }

    public bool BuildProjectFile(string projectFileName, string[] targetNames,
        System.Collections.IDictionary globalProperties,
        System.Collections.IDictionary targetOutputs)
    {
        return false;
    }

    public void LogCustomEvent(CustomBuildEventArgs e) { Console.WriteLine(e.Message); }
    public void LogErrorEvent(BuildErrorEventArgs e) { Console.Error.WriteLine(e.Message); }
    public void LogMessageEvent(BuildMessageEventArgs e) { Console.WriteLine(e.Message); }
    public void LogWarningEvent(BuildWarningEventArgs e) { Console.Error.WriteLine(e.Message); }
}

internal static class Program
{
    private static ITaskItem Asset(string path, string name, string importer, string processor)
    {
        var item = new TaskItem(path);
        item.SetMetadata("Importer", importer);
        item.SetMetadata("Processor", processor);
        item.SetMetadata("Name", name);
        return item;
    }

    // The asset list is enumerated from the content tree rather than transcribed, so it stays in
    // step with mobile-eggbert-content. Stock importers with stock settings reproduce the
    // Content.mgcb rows: magenta colour key and premultiplied alpha are the TextureProcessor
    // defaults, Quality=Best is the SoundEffectProcessor default.
    private static void Collect(List<ITaskItem> assets, string contentDir, string subdir,
                                string pattern, string importer, string processor)
    {
        var dir = Path.Combine(contentDir, subdir);
        var files = Directory.GetFiles(dir, pattern);
        Array.Sort(files, StringComparer.OrdinalIgnoreCase);
        foreach (var file in files)
        {
            // Name is resolved relative to the asset's own directory under RootDirectory, so the
            // stem alone already lands the .xnb in Content/<subdir>/.
            assets.Add(Asset(file, Path.GetFileNameWithoutExtension(file), importer, processor));
        }
    }

    private static int Main(string[] args)
    {
        if (args.Length < 4)
        {
            Console.Error.WriteLine(
                "usage: XnaPipelineRunner <contentDir> <outputDir> <intermediateDir> <pipelineAssemblyDir>");
            return 2;
        }

        string contentDir = args[0];
        string outputDir = args[1];
        string intermediateDir = args[2];
        string pipelineDir = args[3];

        var assets = new List<ITaskItem>();
        Collect(assets, contentDir, "backgrounds", "*.png", "TextureImporter", "TextureProcessor");
        Collect(assets, contentDir, "icons", "*.png", "TextureImporter", "TextureProcessor");
        Collect(assets, contentDir, "sounds", "*.wav", "WavImporter", "SoundEffectProcessor");
        Console.WriteLine("assets: " + assets.Count);

        var task = new BuildContent
        {
            BuildEngine = new ConsoleBuildEngine(),
            ContentProjectGUID = "{0C0B5B1E-4E4C-4F52-9E1B-5A2E2E0B4C11}",
            BuildConfiguration = "Release",
            IntermediateDirectory = intermediateDir,
            OutputDirectory = outputDir,
            PipelineAssemblies = new ITaskItem[]
            {
                new TaskItem(Path.Combine(pipelineDir,
                    "Microsoft.Xna.Framework.Content.Pipeline.TextureImporter.dll")),
                new TaskItem(Path.Combine(pipelineDir,
                    "Microsoft.Xna.Framework.Content.Pipeline.AudioImporters.dll"))
            },
            RebuildAll = false,
            RootDirectory = contentDir,
            LoggerRootDirectory = contentDir,
            SourceAssets = assets.ToArray(),
            TargetPlatform = "Windows",
            TargetProfile = "Reach",
            CompressContent = false
        };

        try
        {
            bool ok = task.Execute();
            Console.WriteLine("BuildContent (Windows/Reach) result: " + ok);
            return ok ? 0 : 1;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine("BuildContent (Windows/Reach) threw:");
            Console.Error.WriteLine(e);
            return 1;
        }
    }
}
