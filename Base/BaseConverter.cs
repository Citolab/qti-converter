using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using QtiPackageConverter.Interface;
using QtiPackageConverter.Model;

namespace QtiPackageConverter.Base
{
    public abstract class BaseConverter : IConvertPackage
    {
        public QtiVersion Version;
        private DirectoryInfo _packageFolder;
        public Manifest Manifest { get; set; }
        public XResource Test { get; set; }
        public List<XItem> Items { get; set; }

        protected BaseConverter(DirectoryInfo extractedPackageLocation, 
            QtiVersion currentVersion,
            Func<string, string> beforeItem , 
            Func<string, string> beforeTest,
            Func<string, string> beforeManifest)
        {
            Version = currentVersion;
            _packageFolder = extractedPackageLocation;
            Init(beforeItem, beforeTest, beforeManifest);
        }

        protected BaseConverter(DirectoryInfo extractedPackageLocation,
            Func<string, string> beforeItem,
            Func<string, string> beforeTest,
            Func<string, string> beforeManifest): 
            this(extractedPackageLocation, QtiVersion.Qti21, beforeItem, beforeTest, beforeManifest)
        {
        }

        protected BaseConverter(DirectoryInfo extractedPackageLocation, QtiVersion currentVersion):  this(extractedPackageLocation, currentVersion, x => x, x => x, x => x)
        {
        }

        protected BaseConverter(DirectoryInfo extractedPackageLocation) : this(extractedPackageLocation, QtiVersion.Qti21, x => x, x => x, x => x)
        {
        }

        public abstract void Convert();

        private void Init(
                Func<string, string> beforeItem,
                Func<string, string> beforeTest,
                Func<string, string> beforeManifest)
        {
            var manifestFile = _packageFolder
                .GetFiles("imsmanifest.xml", SearchOption.AllDirectories)
                .FirstOrDefault();
            if (manifestFile != null)
            {
                _packageFolder = manifestFile.Directory;
                Manifest = new Manifest(manifestFile.Directory, Version, beforeManifest);
                Items = Manifest.Items.Select(itemRef => new XItem(Path.Combine(_packageFolder.FullName, itemRef.Href), itemRef.Identifier, beforeItem)).ToList();
                Test = new XResource(Path.Combine(_packageFolder.FullName, Manifest.Test.Href), Manifest.Test.Identifier, beforeTest);
            }
            else
            {
                Items = new List<XItem>();
              
                foreach (var filePath in Directory.GetFileSystemEntries(_packageFolder.FullName, "*.xml", SearchOption.AllDirectories))
                {
                    if (File.ReadAllText(filePath).IndexOf("assessmentItem", StringComparison.Ordinal) != -1)
                    {
                        Items.Add(new XItem(Path.Combine(filePath), $"ITM-{Path.GetFileNameWithoutExtension(filePath).Replace(".", "_")}", beforeItem));
                    }
                }
            }
        }
    }
}
