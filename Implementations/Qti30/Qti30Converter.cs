using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using QtiPackageConverter.Helper;
using QtiPackageConverter.Base;

namespace QtiPackageConverter.Implementations.Qti30
{
    public class Qti30Converter : BaseConverter
    {
        public Qti30Converter(DirectoryInfo extractedPackageLocation) : base(
            extractedPackageLocation, xml =>
            {
                // Replacing the default namespace is easier here than form and xDocument
                 xml = xml
                     .Replace("\n", " ")
                     .Replace("\t", " ")
                     .Replace("\r", " ");
                var assessmentItemTag = Regex.Match(xml, "<assessmentItem(.*?)>").Value;
                var assessmentItemTagOrg = assessmentItemTag;

                assessmentItemTag = Regex.Replace(assessmentItemTag, @"xsi:schemaLocation=""(.*?)""","");
                assessmentItemTag = Regex.Replace(assessmentItemTag, @"xmlns:xsi=""(.*?)""", "");
                assessmentItemTag = Regex.Replace(assessmentItemTag, @"xmlns=""(.*?)""", "");

                assessmentItemTag = assessmentItemTag.Replace("assessmentItem ",
                    "assessmentItem xmlns=\"http://www.imsglobal.org/xsd/imsqtiasi_v3p0\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\"http://www.imsglobal.org/xsd/imsqtiasi_v3p0 https://purl.imsglobal.org/spec/qti/v3p0/schema/xsd/imsqti_asiv3p0_v1p0.xsd\" ");

                xml = xml.Replace(assessmentItemTagOrg, assessmentItemTag);
                return xml;
            }, 
            
            testXml =>
            {
                testXml = testXml
                    .Replace("\n", " ")
                    .Replace("\t", " ")
                    .Replace("\r", " ");
                var assessmentTag = Regex.Match(testXml, "<assessmentTest(.*?)>").Value;
                var assessmentTagOrg = assessmentTag;
                assessmentTag = Regex.Replace(assessmentTag, @"xsi:schemaLocation=""(.*?)""", "");
                assessmentTag = Regex.Replace(assessmentTag, @"xmlns:xsi=""(.*?)""", "");
                assessmentTag = Regex.Replace(assessmentTag, @"xmlns=""(.*?)""", "");

                assessmentTag = assessmentTag.Replace("assessmentTest ",
                    "assessmentTest xmlns=\"http://www.imsglobal.org/xsd/imsqtiasi_v3p0\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xsi:schemaLocation=\" http://www.imsglobal.org/xsd/imsqtiasi_v3p0 https://purl.imsglobal.org/spec/qti/v3p0/schema/xsd/imsqti_asiv3p0_v1p0.xsd\" ");
                testXml = testXml.Replace(assessmentTagOrg, assessmentTag);
                return testXml;
             
            },
            manifestXml =>
            {
    
                manifestXml = manifestXml
                    .Replace("\n", " ")
                    .Replace("\t", " ")
                    .Replace("\r", " ");
                var manifestTag = Regex.Match(manifestXml, "<manifest(.*?)>").Value;
                var schemaPrefix = Regex.Match(manifestTag, " (.*?)schemaLocation").Value.Replace(":schemaLocation", "").Trim();
                schemaPrefix = schemaPrefix.Substring(schemaPrefix.LastIndexOf(" ", StringComparison.Ordinal), schemaPrefix.Length - schemaPrefix.LastIndexOf(" ", StringComparison.Ordinal));
                var manifestTagOrg = manifestTag;
                manifestTag = Regex.Replace(manifestTag, $@"{schemaPrefix}:schemaLocation=""(.*?)""", "");
                manifestTag = Regex.Replace(manifestTag, $@"xmlns:{schemaPrefix}=""(.*?)""", "");
                manifestTag = Regex.Replace(manifestTag, @"xmlns=""(.*?)""", "");

                manifestTag = manifestTag.Replace("manifest ",
                    "manifest xmlns=\"http://www.imsglobal.org/xsd/qti/qtiv3p0/imscp_v1p1\" xsi:schemaLocation=\"http://www.imsglobal.org/xsd/qti/qtiv3p0/imscp_v1p1 https://purl.imsglobal.org/spec/qti/v3p0/schema/xsd/imsqtiv3p0_imscpv1p2_v1p0.xsd\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" ");
                manifestXml = manifestXml.Replace(manifestTagOrg, manifestTag);
                return manifestXml;
            }
            )
        {

        }
        public override void Convert()
        {
            var applicationPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            var listOfElementsFile = Path.Combine(applicationPath, "nonqtiTags.json");

            // Get the schema to find all the element names that don't need to be qti prefixed.
            // Ones the schema is retrieved, write the output to a .json text file with can
            // be read next time instead of downloading the xsd.
            HashSet<string> tagNamesWithoutQtiPrefix;
            if (File.Exists(listOfElementsFile))
            {
                tagNamesWithoutQtiPrefix =
                    JsonSerializer.Deserialize<List<string>>(File.ReadAllText(listOfElementsFile)).ToHashSet();
            }
            else
            {
                using var client = new HttpClient();
                var response = client.GetAsync("https://purl.imsglobal.org/spec/qti/v3p0/schema/xsd/imsqti_asiv3p0_v1p0.xsd").Result;
                using var stream = response.Content.ReadAsStreamAsync().Result;
                using var reader = new StreamReader(stream);
                var xsd = reader.ReadToEnd();
                var xsdDoc = XDocument.Parse(xsd);
                var elements = xsdDoc.Document.Descendants()
                    .Where(d => d.Name.LocalName == "element")
                    .Select(d => d.GetAttributeValue("name"))
                    .ToList();
                var elementsWithoutQti = elements
                    .Where(elementName => !elementName.StartsWith("qti-")
                    && !string.IsNullOrWhiteSpace(elementName)).ToList();
                File.WriteAllText(listOfElementsFile, JsonSerializer.Serialize(elementsWithoutQti));
                tagNamesWithoutQtiPrefix = elementsWithoutQti.ToHashSet();
            }
            try
            {
                ConvertManifestAndTest.ConvertManifest(Manifest);
                ConvertManifestAndTest.ConvertTest(Test);
                foreach (var item in Items)
                {

                    Console.WriteLine($"Converting item: {item.Identifier}");
                    var c = new ConvertItem(item, Manifest, tagNamesWithoutQtiPrefix);
                    c.Convert();
                    if (item.ToString().IndexOf("questify", StringComparison.Ordinal) != -1)
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
