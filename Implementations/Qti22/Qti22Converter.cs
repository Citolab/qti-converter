using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using QtiPackageConverter.Helper;
using QtiPackageConverter.Base;
using QtiPackageConverter.Model;

namespace QtiPackageConverter.Implementations.Qti22
{
    public class Qti22Converter : BaseConverter
    {
        private readonly DirectoryInfo _extractedPackageLocation;
        private readonly bool _localSchema;

        public Qti22Converter(DirectoryInfo extractedPackageLocation, bool localSchema) : base(
            extractedPackageLocation, xml => xml
                .ReplaceRunsTabsAndLineBraks()
                .ReplaceSchemas(QtiResourceType.AssessmentItem, QtiVersion.Qti22, localSchema), 
            
            testXml => testXml
                .ReplaceRunsTabsAndLineBraks()
                .ReplaceSchemas(QtiResourceType.AssessmentTest, QtiVersion.Qti22, localSchema),
            manifestXml => manifestXml
                .ReplaceRunsTabsAndLineBraks()
                .ReplaceSchemas(QtiResourceType.Manifest, QtiVersion.Qti22, localSchema)
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
                var itemIndex = 0;
                
                foreach (var item in Items)
                {
                    Console.WriteLine($"Converting item: {item.Identifier}");
                    var c = new ConvertItem(item, Manifest);
                    c.Convert();
                    itemIndex++;
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
