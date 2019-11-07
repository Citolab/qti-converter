using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using QtiPackageConverter.Helper;
using QtiPackageConverter.Model;

namespace QtiPackageConverter.Implementations.Qti30
{
    public static class ConvertManifestAndTest
    {
        public static void ConvertTest(XResource test)
        {
            XNamespace xNamespace = "http://www.imsglobal.org/xsd/imsqtiasi_v3p0";
            foreach (var element in test.Content.Descendants())
            {
                var tagName = element.Name.LocalName;
                var kebabTagName = tagName.ToKebabCase();
                element.Name = xNamespace + $"qti-{kebabTagName}";
            }

            // fix attributes
            foreach (var element in test.Content.Descendants())
            {
                var attributesToRemove = new List<XAttribute>();
                var attributesToAdd = new List<XAttribute>();
                foreach (var attribute in element.Attributes()
                    .Where(attr => !attr.IsNamespaceDeclaration && string.IsNullOrEmpty(attr.Name.NamespaceName)))
                {
                    var attributeName = attribute.Name.LocalName;
                    var kebabAttributeName = attributeName.ToKebabCase();
                    if (attributeName != kebabAttributeName)
                    {
                        var newAttr = new XAttribute($"{kebabAttributeName}", attribute.Value);
                        attributesToRemove.Add(attribute);
                        attributesToAdd.Add(newAttr);
                    }
                }
                attributesToRemove.ForEach(a => a.Remove());
                attributesToAdd.ForEach(a => element.Add(a));
            }
            test.Save();
        }
        public static void ConvertManifest(Manifest manifest)
        {
            manifest.FindElementByName("schema")?.SetValue("QTI Package");
            manifest.FindElementByName("schemaversion")?.SetValue("3.0.0");
            manifest.FindElementsByName("resource").ToList().ForEach(resource =>
            {
                switch (resource.GetAttribute("type")?.Value)
                {
                    case "controlfile/xmlv1p0":
                        resource.GetAttribute("type").SetValue("controlfile");
                        break;
                    case "imsqti_item_xmlv2p1":
                        resource.GetAttribute("type").SetValue("imsqti_item_xmlv3p0");
                        break;
                    case "imsqti_test_xmlv2p1":
                        resource.GetAttribute("type").SetValue("imsqti_test_xmlv3p0");
                        break;
                    case "associatedcontent/xmlv1p0/learning-application-resource":
                        resource.GetAttribute("type").SetValue("associatedcontent/learning-application-resource");
                        break;
                    default:
                        resource.GetAttribute("type").SetValue("extension");
                        break;
                };
            });
        }
    }
}
