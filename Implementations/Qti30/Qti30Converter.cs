using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using QtiPackageConverter.Helper;
using QtiPackageConverter.Base;
using QtiPackageConverter.Model;

namespace QtiPackageConverter.Implementations.Qti30
{
    public class Qti30Converter : BaseConverter
    {
        public Qti30Converter(DirectoryInfo extractedPackageLocation, bool localSchema) : base(
            extractedPackageLocation, xml => xml
                .ReplaceRunsTabsAndLineBraks()
                .ReplaceSchemas(QtiResourceType.AssessmentItem, QtiVersion.Qti30, localSchema),

            testXml => testXml
                .ReplaceRunsTabsAndLineBraks()
                .ReplaceSchemas(QtiResourceType.AssessmentTest, QtiVersion.Qti30, localSchema),
            manifestXml => manifestXml
                .ReplaceRunsTabsAndLineBraks()
                .ReplaceSchemas(QtiResourceType.Manifest, QtiVersion.Qti30, localSchema)
        )
        {
        }
        public override void Convert()
        {
            try
            {
                ConvertManifestAndTest.ConvertManifest(Manifest);
                ConvertManifestAndTest.ConvertTest(Test);
                var tagNamesWithoutQtiPrefix =
                        JsonSerializer.Deserialize<List<string>>(File.ReadAllText("nonqtiTags.json")).ToHashSet();
                    foreach (var item in Items)
                {
                    Console.WriteLine($"Converting item: {item.Identifier}");
                    var c = new ConvertItem(item, Manifest, tagNamesWithoutQtiPrefix);
                    c.Convert();
                    if (item.Content.ToString().IndexOf("questify", StringComparison.Ordinal) != -1)
                    {
                        var cs = new ConvertStyling(item);
                        cs.Convert();
                    }
                }
                Manifest?.Save();
                Console.WriteLine($"Successfully converted package");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error while converting package: {e.Message}");
                throw;
            }
        }
    }
}
