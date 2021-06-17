using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using QtiPackageConverter.Helper;
using QtiPackageConverter.Base;
using QtiPackageConverter.Model;

namespace QtiPackageConverter.Implementations.Qti21
{
    public class Qti21Converter : BaseConverter
    {
        private readonly DirectoryInfo _extractedPackageLocation;
        private readonly bool _localSchema;
        public override QtiVersion ConvertsTo { get => QtiVersion.Qti21; }
        public Qti21Converter(DirectoryInfo extractedPackageLocation, bool localSchema, QtiVersion fromVersion) : base(
            extractedPackageLocation, QtiVersion.Qti22, xml => xml
                .ReplaceRunsTabsAndLineBraks()
                .ReplaceSchemas(QtiResourceType.AssessmentItem, QtiVersion.Qti21, fromVersion, localSchema),

            testXml => testXml
                .ReplaceRunsTabsAndLineBraks()
                .ReplaceSchemas(QtiResourceType.AssessmentTest, QtiVersion.Qti21, fromVersion, localSchema),
            manifestXml => manifestXml
                .ReplaceRunsTabsAndLineBraks()
                .ReplaceSchemas(QtiResourceType.Manifest, QtiVersion.Qti21, fromVersion, localSchema)
            )
        {
            _extractedPackageLocation = extractedPackageLocation;
            _localSchema = localSchema;
        }
        public override void Convert()
        {
            try
            {
                ConvertManifestAndTest.ConvertManifest(Manifest, _extractedPackageLocation.FullName, _localSchema);
                ConvertManifestAndTest.ConvertTest(Test);
                foreach (var item in Items)
                {
                    Console.WriteLine($"Converting item: {item.Identifier}");
                    var c = new ConvertItem(item, Manifest);
                    c.Convert();
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
