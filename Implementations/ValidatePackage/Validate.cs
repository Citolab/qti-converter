﻿using System;
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
        public override void Convert()
        {
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
        }
    }
}
