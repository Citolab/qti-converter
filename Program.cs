using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using QtiPackageConverter.Implementations.Qti21;
using QtiPackageConverter.Implementations.Qti22;
using QtiPackageConverter.Implementations.Qti30;
using QtiPackageConverter.Implementations.ValidatePackage;
using QtiPackageConverter.Interface;
using QtiPackageConverter.Model;

namespace QtiPackageConverter
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 2 && args[1].ToLower().IndexOf("help", StringComparison.Ordinal) >= 0)
            {
                // give help
                Console.WriteLine(@"dotnet run QtiPackageConverter package.zip task:(30)");
            }

            {
                var storeNewPackage = true;
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
                    var packageFolder = ExtractPackage(packageRef);

                    IConvertPackage converter = null;
                    switch (args[1].ToLower())
                    {
                        case "validate30":
                            {
                                storeNewPackage = false; converter = new Validate(packageFolder, QtiVersion.Qti30);
                                break;
                            }
                        case "validate22":
                            {
                                storeNewPackage = false;
                                converter = new Validate(packageFolder, QtiVersion.Qti22);
                                break;
                            }
                        case "validate21":
                        {
                            storeNewPackage = false;
                            converter = new Validate(packageFolder, QtiVersion.Qti21);
                            break;
                        }
                        case "30":
                            {
                                var local = args.ToList().Contains("--local");
                                converter = new Qti30Converter(packageFolder, local);
                                break;
                            }
                        case "22":
                            {

                                var local = args.ToList().Contains("--local");
                                converter = new Qti22Converter(packageFolder, local);
                                break;
                            }
                        case "21":
                        {

                            var local = args.ToList().Contains("--local");
                            converter = new Qti21Converter(packageFolder, local);
                            break;
                        }
                        default:
                            throw new Exception($"ConvertManifest method '{args[1]}' not found. Options: dynamischeleestoets");
                    }
                    // call convert function
                    converter.Convert();
                    // save new package
                    if (storeNewPackage)
                    {
                        var newPackage = Path.Combine(Path.GetDirectoryName(packageRef),
                            $"{Path.GetFileNameWithoutExtension(packageRef)}-{DateTime.Now:yyyy-MM-dd_hh-mm-ss}.zip");
                        ZipFile.CreateFromDirectory(packageFolder.FullName, newPackage, CompressionLevel.Optimal, false);
                        Console.WriteLine($"package successfully converted: {newPackage}");
                        if (args.ToList().Contains("--validate"))
                        {
                            Console.WriteLine($"validating converted package.");
                            var newpackageFolder = ExtractPackage(newPackage);
                            switch (args[1].ToLower())
                            {
                                case "30":
                                {
                                    converter = new Validate(newpackageFolder, QtiVersion.Qti30);
                                    break;
                                }
                                case "22":
                                {
                                    converter = new Validate(newpackageFolder, QtiVersion.Qti22);
                                    break;
                                }
                                case "21":
                                {
                                    converter = new Validate(newpackageFolder, QtiVersion.Qti21);
                                    break;
                                }
                            }
                            converter?.Convert();
                            Console.WriteLine($"Done.");
                        };
 
                    }
                    else
                    {
                        Console.WriteLine($"Done.");
                    }

                }
                Console.WriteLine($"All done");
            }

        }

        private static DirectoryInfo ExtractPackage(string packageRef)
        {
            // extract package
            var extractPath = new DirectoryInfo(Path.Combine(Path.Combine(Path.GetTempPath(), "_temp"), Path.GetFileNameWithoutExtension(Path.GetRandomFileName())));
            if (!extractPath.Exists) extractPath.Create();
            var packageFolder = new DirectoryInfo(Path.Combine(extractPath.FullName, "package"));

            if (!packageFolder.Exists) packageFolder.Create();
            using var packageStream = new FileStream(packageRef, FileMode.Open);
            using var packageZip = new ZipArchive(packageStream);
            packageZip.ExtractToDirectory(packageFolder.FullName);
            return packageFolder;
        }
    }
}