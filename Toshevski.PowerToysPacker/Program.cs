// See https://aka.ms/new-console-template for more information
using System.IO;
using System.Linq;
using System.IO.Compression;

Console.WriteLine("Hello, World!");

var finalPluginsDirectoryName = "PluginBinaries";
var zipDirectoryName = "PluginReleases";
var version = "Debug";
var type = "net6.0-windows";

var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());

var parentDirectory = currentDirectory.Parent;

var powerToysDllsPath = Path.Combine(parentDirectory.FullName, "PowerToysDlls");
var powerToysDlls = new DirectoryInfo(powerToysDllsPath).GetFiles().Select(x => x.Name).ToList();

Console.WriteLine("=== Current PowerToys dll's.");

foreach (var dll in powerToysDlls) {
    Console.WriteLine(dll);
}

var finalPluginsDirectoryPath = Path.Combine(parentDirectory.FullName, finalPluginsDirectoryName);

if (Directory.Exists(finalPluginsDirectoryPath))
    Directory.Delete(finalPluginsDirectoryPath, true);
    
Directory.CreateDirectory(finalPluginsDirectoryPath);

var zipDirectoryPath = Path.Combine(parentDirectory.FullName, zipDirectoryName);

if (Directory.Exists(zipDirectoryPath))
    Directory.Delete(zipDirectoryPath, true);
    
Directory.CreateDirectory(zipDirectoryPath);

Console.WriteLine("=== Copy plugin dll's and zip them.");

foreach (var pluginDirectory in parentDirectory.EnumerateDirectories().Where(x => x.Name.Contains(".Plugins."))) {
    Console.WriteLine($"Now copying: {pluginDirectory.Name}");

    var binPath = Path.Combine(pluginDirectory.FullName, "bin", version, type);

    var path = Path.Combine(finalPluginsDirectoryPath, pluginDirectory.Name);

    var releaseDirectory = Directory.CreateDirectory(path);

    var pluginFiles = new DirectoryInfo(binPath).GetFiles().Where(x => !powerToysDlls.Contains(x.Name));

    foreach (var pluginFile in pluginFiles) {
        File.Copy(pluginFile.FullName, Path.Combine(releaseDirectory.FullName, pluginFile.Name));
    }

    ZipFile.CreateFromDirectory(releaseDirectory.FullName, Path.Combine(zipDirectoryPath, pluginDirectory.Name + ".zip"));
}