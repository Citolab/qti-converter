using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using QtiPackageConverter.Implementations.Qti30;
using QtiPackageConverter.Interface;

namespace QtiPackageConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 2 && args[1].ToLower().IndexOf("help", System.StringComparison.Ordinal) >= 0)
            {
                // give help
                Console.WriteLine(@"dotnet run QtiPackageConverter package.zip task:(30)");
            }
            {
                var appPath = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location ?? string.Empty);

                var packages = new List<string>();
                if (File.Exists(args[0]))
                {
                    packages.Add(args[0]);
                }
                else if (Directory.Exists(args[0]))
                {
                    packages.AddRange(Directory.GetFiles(args[0], "*.zip"));
                }

                foreach (var package in packages)
                {
                    var packageRef = package;
                    if (!Path.IsPathRooted(packageRef)) packageRef = Path.Combine(appPath, packageRef);
                    if (!File.Exists(packageRef)) throw new Exception($"cannot find package file: {packageRef}");
                    // extract package
                    var extractPath = new DirectoryInfo(Path.Combine(Path.Combine(Path.GetTempPath(), "_temp"), Path.GetFileNameWithoutExtension(Path.GetRandomFileName())));
                    if (!extractPath.Exists) extractPath.Create();
                    var packageFolder = new DirectoryInfo(Path.Combine(extractPath.FullName, "package"));

                    if (!packageFolder.Exists) packageFolder.Create();
                    using (var packageStream = new FileStream(packageRef, FileMode.Open))
                    {
                        using var packageZip = new ZipArchive(packageStream);
                        packageZip.ExtractToDirectory(packageFolder.FullName);
                    }
                    IConvertPackage converter = null;
                    switch (args[1].ToLower())
                    {
                        case "30":
                            {
                                converter = new Qti30Converter(packageFolder);
                                break;
                            }
                        default:
                            throw new Exception($"ConvertManifest method '{args[1]}' not found. Options: dynamischeleestoets");
                    }
                    // call convert function
                    converter.Convert();
                    // save new package
                    var newPackage = Path.Combine(Path.GetDirectoryName(packageRef),
                        $"{Path.GetFileNameWithoutExtension(packageRef)}-{DateTime.Now:yyyy-MM-dd_hh-mm-ss}.zip");
                    ZipFile.CreateFromDirectory(packageFolder.FullName, newPackage, CompressionLevel.Optimal, false);
                    Console.WriteLine($"package successfully converted: {newPackage}");
                }
                Console.WriteLine($"All done");
            }
        }
    }
}