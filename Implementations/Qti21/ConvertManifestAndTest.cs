using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using QtiPackageConverter.Helper;
using QtiPackageConverter.Model;

namespace QtiPackageConverter.Implementations.Qti21
{
    public static class ConvertManifestAndTest
    {
        public static void ConvertTest(XResource test)
        {
            test.Save();
        }

        public static void ConvertManifest(Manifest manifest, string packageLocation, bool localSchemas)
        {
            manifest.DeleteLocalImsSchemasToPackage(packageLocation);
            if (localSchemas)
            {
                manifest.AddLocalSchemasToPackage(packageLocation, QtiVersion.Qti21);
            }

            manifest.FindElementByName("schema")?.SetValue("IMS Content");
            manifest.FindElementByName("schemaversion")?.SetValue("2.1");
            manifest.FindElementsByName("resource").ToList().ForEach(resource =>
                {
                    switch (resource.GetAttribute("type")?.Value)
                    {
                        case "imsqti_item_xmlv2p2":
                            resource.GetAttribute("type").SetValue("imsqti_item_xmlv2p1");
                            break;
                        case "imsqti_test_xmlv2p2":
                            resource.GetAttribute("type").SetValue("imsqti_test_xmlv2p1");
                            break;
                        case "webcontent":
                            resource.GetAttribute("type")
                                .SetValue(resource.ToString().IndexOf(".xsd", StringComparison.Ordinal) != -1
                                    ? "controlfile/xmlv1p0"
                                    : "associatedcontent/xmlv1p0/learning-application-resource");
                            break;
                    }
                });
        }
    }
}
