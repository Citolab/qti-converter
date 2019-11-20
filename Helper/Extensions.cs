using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

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

        public static bool Validate(this XDocument xDoc)
        {
            var result = true;
            // Set the validation settings.
            using var sr = new StringReader(xDoc.ToString());
            var reader = XmlReader.Create(sr, XsdHelper.GetXmlReaderSettings(ValidationEventHandler));

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
            while (reader.Read());
            return result;
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

            if (source.Length == 0) return String.Empty;

            StringBuilder builder = new StringBuilder();

            for (var i = 0; i < source.Length; i++)
            {
                if (Char.IsLower(source[i])) // if current char is already lowercase
                {
                    builder.Append(source[i]);
                }
                else if (i == 0) // if current char is the first char
                {
                    builder.Append(Char.ToLower(source[i]));
                }
                else if (Char.IsLower(source[i - 1])) // if current char is upper and previous char is lower
                {
                    builder.Append("-");
                    builder.Append(Char.ToLower(source[i]));
                }
                else if (i + 1 == source.Length || Char.IsUpper(source[i + 1])) // if current char is upper and next char doesn't exist or is upper
                {
                    builder.Append(Char.ToLower(source[i]));
                }
                else // if current char is upper and next char is lower
                {
                    builder.Append("-");
                    builder.Append(Char.ToLower(source[i]));
                }
            }

            return builder.ToString();
        }
    }
}
