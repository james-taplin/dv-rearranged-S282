using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace RearrangedS282
{
    // Resolve from UMM and Unity's running game, never from a build-machine path
    // or the shell's current directory. Shared with the package validator.
    internal sealed class MeshExportPaths
    {
        internal const string ToolFolder = "AssetStudioModCLI_net472_win64_contained";
        internal static readonly string[] RuntimeFiles = {
            "AssetStudio.dll", "AssetStudioUtility.dll", "AssetStudioModCLI.exe", "AssetStudioModCLI.exe.config",
            "K4os.Compression.LZ4.dll", "Mono.Cecil.dll", "Newtonsoft.Json.dll", "System.Buffers.dll",
            "System.Memory.dll", "System.Numerics.Vectors.dll", "System.Runtime.CompilerServices.Unsafe.dll",
            "System.Text.Encoding.CodePages.dll", "System.ValueTuple.dll"
        };
        internal readonly string ModDirectory, ToolDirectory, Executable, Resources, ExportDirectory;

        internal MeshExportPaths(string modDirectory, string gameDataDirectory)
        {
            if (string.IsNullOrWhiteSpace(modDirectory) || string.IsNullOrWhiteSpace(gameDataDirectory))
                throw new ArgumentException("UMM mod path and Unity game data path are required for mesh export.");
            var data = Path.GetFullPath(gameDataDirectory).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            ModDirectory = Path.GetFullPath(Path.IsPathRooted(modDirectory) ? modDirectory :
                Path.Combine(Directory.GetParent(data).FullName, modDirectory));
            ToolDirectory = Path.Combine(ModDirectory, ToolFolder);
            Executable = Path.Combine(ToolDirectory, "AssetStudioModCLI.exe");
            Resources = Path.Combine(data, "resources.assets");
            ExportDirectory = Path.Combine(ModDirectory, "assets");
        }

        internal ProcessStartInfo CreateStartInfo(string meshName)
        {
            if (string.IsNullOrWhiteSpace(meshName) || meshName.Any(c => !(char.IsLetterOrDigit(c) || c == '_')))
                throw new ArgumentException("Invalid native mesh export name.", nameof(meshName));
            var missing = RuntimeFiles.Where(f => !File.Exists(Path.Combine(ToolDirectory, f))).ToArray();
            if (missing.Length != 0)
                throw new FileNotFoundException("Rearranged S282 mesh exporter is incomplete. Missing: " +
                    string.Join(", ", missing) + ". Reinstall the complete Nexus ZIP through UMM, including the " +
                    ToolFolder + " folder. If files disappear after extraction, check antivirus quarantine history.",
                    Path.Combine(ToolDirectory, missing[0]));
            if (!File.Exists(Resources))
                throw new FileNotFoundException("Cannot find the running Derail Valley installation's native mesh resources.", Resources);
            Directory.CreateDirectory(ExportDirectory);
            return new ProcessStartInfo {
                FileName = Executable, WorkingDirectory = ToolDirectory,
                Arguments = "\"" + Resources + "\" -o \"" + ExportDirectory + "\" -t mesh --filter-by-name \"" + meshName + "\"",
                CreateNoWindow = true, UseShellExecute = false
            };
        }
    }
}
