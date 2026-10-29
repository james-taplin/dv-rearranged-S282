using System;
using System.IO;
using RearrangedS282;

internal static class Program
{
    static int checks;
    static void Check(bool condition, string why) { checks++; if (!condition) throw new Exception(why); }
    static void Main(string[] args)
    {
        if (args[0] == "--export")
        {
            var paths = new MeshExportPaths(args[1], args[2]);
            using var process = System.Diagnostics.Process.Start(paths.CreateStartInfo("s282_mech_wheels_connect"));
            if (!process.WaitForExit(60000)) { process.Kill(); throw new Exception("Clean-install export timed out"); }
            Check(process.ExitCode == 0, "Bundled exporter failed");
            var obj = Path.Combine(paths.ExportDirectory, "s282_mech_wheels_connect.obj");
            Check(File.Exists(obj) && new FileInfo(obj).Length > 1000, "Native rod OBJ was not generated");
            Console.WriteLine("Clean ZIP extraction: bundled exporter launched and generated the native coupling rod");
            return;
        }
        foreach (string drive in new[] { "C", "D", "E", "Z" })
        {
            string game = drive + @":\Steam Library\Derail Valley";
            foreach (string tail in new[] { "", "\\" })
            {
                var paths = new MeshExportPaths(game + @"\Mods\RearrangedS282" + tail, game + @"\DerailValley_Data" + tail);
                Check(paths.Executable == game + @"\Mods\RearrangedS282\" + MeshExportPaths.ToolFolder + @"\AssetStudioModCLI.exe", "Absolute UMM install path changed");
                Check(paths.Resources == game + @"\DerailValley_Data\resources.assets", "Running game data path changed");
                var relative = new MeshExportPaths(@"Mods\RearrangedS282", game + @"\DerailValley_Data" + tail);
                Check(relative.Executable == paths.Executable, "Relative UMM path used shell current directory");
            }
        }
        string root = Path.GetFullPath(args[0]);
        var local = new MeshExportPaths(Path.Combine(root, "Mods", "RearrangedS282"), Path.Combine(root, "DerailValley_Data"));
        try { local.CreateStartInfo("s282_mech_wheels_connect"); throw new Exception("Missing runtime accepted"); }
        catch (FileNotFoundException ex) { Check(ex.Message.Contains("complete Nexus ZIP") && ex.FileName.StartsWith(local.ToolDirectory), "Missing runtime has no actionable explanation"); }
        Directory.CreateDirectory(local.ToolDirectory);
        foreach (var name in MeshExportPaths.RuntimeFiles) File.WriteAllText(Path.Combine(local.ToolDirectory, name), "fixture");
        try { local.CreateStartInfo("s282_mech_wheels_connect"); throw new Exception("Missing game data accepted"); }
        catch (FileNotFoundException ex) { Check(ex.FileName == local.Resources, "Missing game resources misreported"); }
        Directory.CreateDirectory(Path.GetDirectoryName(local.Resources)); File.WriteAllText(local.Resources, "fixture");
        foreach (var name in MeshExportPaths.RuntimeFiles)
        {
            var file = Path.Combine(local.ToolDirectory, name); File.Delete(file);
            try { local.CreateStartInfo("s282_mech_wheels_connect"); throw new Exception("Missing dependency accepted: " + name); }
            catch (FileNotFoundException ex) { Check(ex.FileName == file, "Missing dependency not identified"); }
            File.WriteAllText(file, "fixture");
        }
        var info = local.CreateStartInfo("s282_mech_wheels_connect");
        Check(info.FileName == local.Executable && info.WorkingDirectory == local.ToolDirectory, "Exporter launch directory differs from bundled location");
        Check(info.Arguments.Contains("\"" + local.Resources + "\"") && info.Arguments.Contains("\"" + local.ExportDirectory + "\""), "Paths with spaces are not quoted");
        Check(!info.UseShellExecute && info.CreateNoWindow, "Unexpected shell/window launch");
        Check(Directory.Exists(local.ExportDirectory), "Export directory not created");
        try { local.CreateStartInfo("bad\" --anything"); throw new Exception("Invalid mesh name accepted"); }
        catch (ArgumentException) { checks++; }
        Console.WriteLine(checks + " exporter path and incomplete-install checks passed");
    }
}
