using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using QtiPackageConverter.Base;
using QtiPackageConverter.Helper;
using QtiPackageConverter.Model;

namespace QtiPackageConverter.Implementations.ValidatePackage
{

    public class Validate : BaseConverter
    {
        private readonly string _extractedPackageLocation;
        private readonly QtiVersion _qtiVersion;
        public override QtiVersion ConvertsTo { get => _qtiVersion; }
        public override void Convert()
        {
            Manifest.AddLocalSchemasToPackage(_extractedPackageLocation, QtiVersion.Qti21);
            Items.ForEach(i =>
            {
                Console.WriteLine($"Validing item: {i.Identifier}");
                i.Content.Validate(Version);
            });
            Console.WriteLine($"Validing assessmentTest");
            Test.Content.Validate(Version);
            Console.WriteLine($"Validing manifest");
            Manifest.Validate(Version);
        }


        public Validate(DirectoryInfo extractedPackageLocation, QtiVersion version) : base(
            extractedPackageLocation, version, xml => xml
                .ReplaceRunsTabsAndLineBraks()
                .ToLocalSchemas("../controlxsds", version),

            testXml => testXml
                .ReplaceRunsTabsAndLineBraks()
                .ToLocalSchemas("../controlxsds", version),
            manifestXml => manifestXml
                .ReplaceRunsTabsAndLineBraks()
                .ToLocalSchemas("/controlxsds", version)
        )
        {
            _qtiVersion = version;
            _extractedPackageLocation = extractedPackageLocation.FullName;
        }
    }
}
