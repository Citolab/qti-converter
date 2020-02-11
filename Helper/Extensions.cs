using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using QtiPackageConverter.Model;
using static System.Char;

namespace QtiPackageConverter.Helper
{
    public static class Extensions
    {
        public static IEnumerable<XElement> FindElementsByName(this XDocument doc, string name)
        {
            return doc.Descendants().Where(d => d.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public static IEnumerable<XElement> FindElementsByLastPartOfName(this XDocument doc, string name)
        {
            return doc.Descendants().Where(d => d.Name.LocalName.EndsWith(name, StringComparison.OrdinalIgnoreCase));
        }
        public static IEnumerable<XElement> FindElementsByLastPartOfName(this XElement el, string name)
        {
            return el.Descendants().Where(d => d.Name.LocalName.EndsWith(name, StringComparison.OrdinalIgnoreCase));
        }
        public static IEnumerable<XElement> FindElementsByName(this XElement el, string name)
        {
            return el.Descendants().Where(d => d.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public static XElement FindElementByName(this XDocument doc, string name)
        {
            return doc.FindElementsByName(name).FirstOrDefault();
        }

        public static string GetAttributeValue(this XElement el, string name)
        {
            return el.GetAttribute(name)?.Value ?? String.Empty;
        }
        public static XAttribute GetAttribute(this XElement el, string name)
        {
            return el.Attributes()
                .FirstOrDefault(a => a.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        public static IEnumerable<XAttribute> GetAttributes(this XDocument doc, string name)
        {
            var s = doc.Descendants().SelectMany(d => d.Attributes()
                .Where(a => a.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase)));
            return s;
        }

        public static bool Validate(this XDocument xDoc, QtiVersion version)
        {
            var result = true;
            // Set the validation settings.
            using var sr = new StringReader(xDoc.ToString());
            var reader = version == QtiVersion.Qti22
                ?
                XmlReader.Create(sr, XsdHelper.GetXmlReaderSettingsQti22(ValidationEventHandler))
                : version == QtiVersion.Qti30
                    ? XmlReader.Create(sr, XsdHelper.GetXmlReaderSettingsQti30(ValidationEventHandler))
                    :
                    XmlReader.Create(sr, XsdHelper.GetXmlReaderSettingsQti21(ValidationEventHandler));

            void ValidationEventHandler(object sender, ValidationEventArgs e)
            {
                result = false;
                var orgColor = Console.ForegroundColor;
                var type = e.Severity;
                if (type == XmlSeverityType.Warning)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(e.Message);
                    Console.ForegroundColor = orgColor;
                }
                if (type == XmlSeverityType.Error)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e.Message);
                    Console.ForegroundColor = orgColor;
                }
            }
            // Parse the file. 
            while (reader.Read()) ;
            return result;
        }

        public static XmlSchema GetSchemaByName(this XmlReaderSettings settings, string targetNamespace)
        {
            foreach (XmlSchema schema in settings.Schemas.Schemas())
            {
                if (schema.TargetNamespace == targetNamespace)
                {
                    return schema;
                }
            }
            return null;
        }


        public static IEnumerable<XElement> FindElementsByElementAndAttributeValue(this XElement element, string elementName, string attributeName, string attributeValue)
        {
            return element.FindElementsByName(elementName)
                .Where(d => d.Attributes()
                    .Any(a => a.Name.LocalName.Equals(attributeName, StringComparison.OrdinalIgnoreCase) &&
                              a.Value.Equals(attributeValue, StringComparison.OrdinalIgnoreCase)));
        }

        public static IEnumerable<XElement> FindElementsByElementAndAttributeValue(this XDocument doc, string elementName, string attributeName, string attributeValue)
        {
            return doc.FindElementsByName(elementName)
                .Where(element => element.Attributes()
                    .Any(a => a.Name.LocalName.Equals(attributeName, StringComparison.OrdinalIgnoreCase) &&
                              a.Value.Equals(attributeValue, StringComparison.OrdinalIgnoreCase)));
        }

        public static IEnumerable<XElement> FindElementsByElementAndAttributeStartValue(this XDocument doc, string elementName, string attributeName, string attributeValue)
        {
            return doc.FindElementsByName(elementName)
                .Where(element => element.Attributes()
                    .Any(a => a.Name.LocalName.Equals(attributeName, StringComparison.OrdinalIgnoreCase) &&
                              a.Value.ToLower().StartsWith(attributeValue.ToLower())));
        }

        public static string ReplaceRunsTabsAndLineBraks(this string text)
        {
            return text
                .Replace("\n", " ")
                .Replace("\t", " ")
                .Replace("\r", " ");
        }

        private static string GetBaseSchema(QtiResourceType resourceType, QtiVersion version)
            => XsdHelper.BaseSchemas[$"{resourceType.ToString()}-{version.ToString()}"];
        
        public static string GetBaseSchemaLocation(QtiResourceType resourceType, QtiVersion version)
            => XsdHelper.BaseSchemaLocations[$"{resourceType.ToString()}-{version.ToString()}"];

        public static string ToLocalSchemas(this string xml, string xsdFolder, QtiVersion newVersion)
        {
            var itemXsdLocation = GetBaseSchemaLocation(QtiResourceType.AssessmentItem, newVersion);
            var manifestXsdLocation = GetBaseSchemaLocation(QtiResourceType.Manifest, newVersion);
            xml = xml.Replace($"{itemXsdLocation}", $"{xsdFolder}/{Path.GetFileName(itemXsdLocation)}");
            xml = xml.Replace($"{manifestXsdLocation}", $"{xsdFolder}/{Path.GetFileName(manifestXsdLocation)}");
            return xml;
        }

        public static string ReplaceSchemas(this string xml, QtiResourceType resourceType, QtiVersion newVersion, QtiVersion oldVersion, bool localSchema)
        {
            var tagName = resourceType == QtiResourceType.AssessmentItem ? "assessmentItem" :
                resourceType == QtiResourceType.AssessmentTest ? "assessmentTest" :
                "manifest";
            var baseScheme = GetBaseSchema(resourceType, newVersion);
            var baseSchemeLocation = GetBaseSchemaLocation(resourceType, newVersion);
            var qtiParentTag = Regex.Match(xml, $"<{tagName}(.*?)>").Value;
            var qtiParentOrg = qtiParentTag;
            var schemaPrefix = Regex.Match(qtiParentTag, " (.*?)schemaLocation")
                .Value.Replace(":schemaLocation", "").Trim();
            schemaPrefix = schemaPrefix.Substring(schemaPrefix.LastIndexOf(" ", StringComparison.Ordinal), schemaPrefix.Length - schemaPrefix.LastIndexOf(" ", StringComparison.Ordinal)).Trim();

            var schemaLocations = Regex.Match(qtiParentTag, @$"{schemaPrefix}:schemaLocation=""(.*?)""").Value;
            var extensionSchemas = RemoveSchemaFromLocation(schemaLocations, GetBaseSchema(resourceType, oldVersion));
            if (resourceType == QtiResourceType.Manifest)
            {
                extensionSchemas = RemoveSchemaFromLocation(extensionSchemas, GetBaseSchema(QtiResourceType.AssessmentItem, oldVersion));
                extensionSchemas = extensionSchemas +
                                   $" {GetBaseSchema(QtiResourceType.AssessmentItem, newVersion)} {GetBaseSchemaLocation(QtiResourceType.AssessmentItem, newVersion)}";
            }

            qtiParentTag = Regex.Replace(qtiParentTag, @$"{schemaPrefix}:schemaLocation=""(.*?)""", "");
            qtiParentTag = Regex.Replace(qtiParentTag, @$"xmlns:{schemaPrefix}=""(.*?)""", "");
            qtiParentTag = Regex.Replace(qtiParentTag, @$"xmlns:xsi=""(.*?)""", "");
            qtiParentTag = Regex.Replace(qtiParentTag, @"xmlns=""(.*?)""", "");

            if (localSchema)
            {
                var prefix = resourceType == QtiResourceType.Manifest ? "/" : "../";
                baseSchemeLocation = $"{prefix}controlxsds/{Path.GetFileName(baseSchemeLocation)}";
            }
            var schemaLocation = $"xsi:schemaLocation=\"{baseScheme}  {baseSchemeLocation} ";

            qtiParentTag = qtiParentTag.Replace($"{tagName} ",
                $@"{tagName} xmlns=""{baseScheme}"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" " +
                schemaLocation + "  " + extensionSchemas + @""" "
            );

            xml = xml.Replace(qtiParentOrg, qtiParentTag);
            if (schemaPrefix != "xsi")
            {
                xml = xml.Replace($":{schemaPrefix}", ":xsi");
                xml = xml.Replace($"{schemaPrefix}:", "xsi:");
            }
            return xml;
        }

        private static string RemoveSchemaFromLocation(string schemaLocations, string schemaToRemove)
        {
            var schemas = schemaLocations.Split(" ")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
            var schemaIndex = schemas.FindIndex(s => s.IndexOf(schemaToRemove, StringComparison.Ordinal) != -1);
            var extensionSchemas = schemaIndex == -1
                ? schemaLocations
                : string.Join(" ", schemas.Where((schema) =>
                        schemas.IndexOf(schema) != schemaIndex &&
                        schemas.IndexOf(schema) != 1 + schemaIndex)
                    .ToArray());
            return extensionSchemas.Trim('"');
        }

        public static string GetElementValue(this XElement el, string name)
        {
            return el.FindElementsByName(name).FirstOrDefault()?.Value ?? String.Empty;
        }
        //xmlns

        public static IEnumerable<XElement> GetInteractions(this XDocument doc)
        {
            return doc.Document?.Root.GetInteractions();
        }

        public static bool IsAlphaNumeric(this char strToCheck)
        {
            var rg = new Regex(@"^[a-zA-Z0-9\s,]*$");
            return rg.IsMatch(strToCheck.ToString());
        }

        public static void DeleteLocalImsSchemasToPackage(this Manifest manifest, string packageLocation)
        {
            var controlResourceList = manifest
                .FindElementsByElementAndAttributeStartValue("resource", "type", "controlfile/")
                .ToList();
            var existingXsdReference = new List<string>();
            if (controlResourceList.Any())
            {
                controlResourceList.ForEach(controlResource =>
                {
                    var xsdFiles = controlResource?.FindElementsByName("file").ToList();
                    xsdFiles.ForEach(xsd =>
                    {
                        // trying to keep custom/extension schema's
                        var file = Path.Combine(packageLocation, xsd.GetAttributeValue("href"));
                        if (file.ToLower().Contains("ims") && !file.ToLower().Contains("ext"))
                        {
                            xsd.Remove();
                        }
                        else
                        {
                            existingXsdReference.Add(Path.GetFileName(file));
                        }
                    });
                });
                // delete all xsd files that are not in the manifest.
                foreach (var file in Directory.GetFiles(packageLocation, "*.xsd", SearchOption.AllDirectories))
                {
                    if (!existingXsdReference.Contains(Path.GetFileName(file)))
                    {
                        File.Delete(file);
                    }
                }
            }
        }

        public static void AddLocalSchemasToPackage(this Manifest manifest, string packageLocation, QtiVersion newVersion)
        {
            var controlResources = manifest
                .FindElementsByElementAndAttributeStartValue("resource", "type", "controlfile/xmlv1p0")
                .FirstOrDefault();
            if (controlResources == null)
            {
                var resources = manifest.FindElementByName("resources");
                controlResources = XElement.Parse("<resource identifier=\"I_00001_CF\" type=\"controlfile/xmlv1p0\"></resource>");
                resources?.Add(controlResources);
            }
            var xsdFolder = Path.Combine(packageLocation, "controlxsds");
            if (!Directory.Exists(xsdFolder))
            {
                Directory.CreateDirectory(xsdFolder);
            }
            var applicationPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
            var version = newVersion == QtiVersion.Qti21 ? "21" : newVersion == QtiVersion.Qti22 ? "22" : "30";
            var schemaLocation = $"Xsds/schema_qti{version}.zip";
            ZipFile.ExtractToDirectory(Path.Combine(applicationPath, schemaLocation), xsdFolder, Encoding.Default, true);
            Directory.GetFiles(xsdFolder, "*.xsd").ToList().ForEach(xsdFile =>
            {
                controlResources?.Add(XElement.Parse($"<file href=\"controlxsds/{Path.GetFileName(xsdFile)}\" />"));
            });
        }



        /// <summary>
        /// Force tags to be non-selfclosing
        /// </summary>
        /// <param name="document"></param>
        public static void ForceTags(this XDocument document)
        {
            var allowedSelfClosingTags = new HashSet<string>
            {
                "br", "img", "qti-stylesheet", "qti-text-entry-interaction"
            };
            foreach (var childElement in
                from x in document.DescendantNodes().OfType<XElement>()
                where x.IsEmpty && !allowedSelfClosingTags.Contains(x.Name.LocalName.ToLower())
                select x)
            {
                childElement.Value = string.Empty;
            }
        }


        public static string ReplaceAllOccurrenceExceptFirst(this string source, string find, string replace)
        {
            var result = source;
            while (result.CountStringOccurrences(find) > 1)
            {
                var place = result.LastIndexOf(find, StringComparison.Ordinal);
                if (place == -1)
                    return result;
                result = result.Remove(place, find.Length).Insert(place, replace);
            }

            return result;
        }

        public static int CountStringOccurrences(this string text, string pattern)
        {
            // Loop through all instances of the string 'text'.
            var count = 0;
            var i = 0;
            while ((i = text.IndexOf(pattern, i, StringComparison.Ordinal)) != -1)
            {
                i += pattern.Length;
                count++;
            }
            return count;
        }

        public static IEnumerable<XElement> GetInteractions(this XElement el)
        {
            var qti2Elements = el.FindElementsByLastPartOfName("interaction")
                .Where(d => d.Name.LocalName.IndexOf("audio", StringComparison.OrdinalIgnoreCase) == -1)
                .Where(d => d.Attributes()
                    .Any(a => a.Name.LocalName.Equals("responseIdentifier", StringComparison.OrdinalIgnoreCase) &&
                              a.Value.Equals("RESPONSE", StringComparison.OrdinalIgnoreCase)));
            var qti3Elements = el.FindElementsByLastPartOfName("Interaction")
                .Where(d => d.Name.LocalName.IndexOf("audio", StringComparison.OrdinalIgnoreCase) == -1)
                .Where(d => d.Attributes()
                    .Any(a => a.Name.LocalName.Equals("response-identifier", StringComparison.OrdinalIgnoreCase) &&
                              a.Value.Equals("RESPONSE", StringComparison.OrdinalIgnoreCase)));
            return qti2Elements.Concat(qti3Elements);
        }
        public static XElement GetInteraction(this XElement element)
        {
            return element.GetInteractions().FirstOrDefault();
        }
        public static XElement GetInteraction(this XDocument doc)
        {
            return doc.GetInteractions().FirstOrDefault();
        }

        public static void SetAttributeValue(this XElement el, string name, string value)
        {
            el.GetAttribute(name)?.SetValue(value);
        }

        public static string ToKebabCase(this string source)
        {
            if (source is null) return null;

            if (source.Length == 0) return string.Empty;

            var builder = new StringBuilder();

            for (var i = 0; i < source.Length; i++)
            {
                if (IsLower(source[i])) // if current char is already lowercase
                {
                    builder.Append(source[i]);
                }
                else if (i == 0) // if current char is the first char
                {
                    builder.Append(ToLower(source[i]));
                }
                else if (IsLower(source[i - 1])) // if current char is upper and previous char is lower
                {
                    builder.Append("-");
                    builder.Append(ToLower(source[i]));
                }
                else if (i + 1 == source.Length || IsUpper(source[i + 1])) // if current char is upper and next char doesn't exist or is upper
                {
                    builder.Append(ToLower(source[i]));
                }
                else // if current char is upper and next char is lower
                {
                    builder.Append("-");
                    builder.Append(ToLower(source[i]));
                }
            }

            return builder.ToString();
        }
    }
}
