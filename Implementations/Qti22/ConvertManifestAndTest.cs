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

namespace QtiPackageConverter.Implementations.Qti22
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
                manifest.AddLocalSchemasToPackage(packageLocation, QtiVersion.Qti22);
            }

            manifest.FindElementByName("schema")?.SetValue("QTIv2.2 Package");
            manifest.FindElementByName("schemaversion")?.SetValue("1.0.0");
            manifest.FindElementsByName("resource").ToList().ForEach(resource =>
                {
                    switch (resource.GetAttribute("type")?.Value)
                    {
                        case "controlfile/xmlv1p0":
                            resource.GetAttribute("type").SetValue("webcontent");
                            break;
                        case "imsqti_item_xmlv2p1":
                            resource.GetAttribute("type").SetValue("imsqti_item_xmlv2p2");
                            break;
                        case "imsqti_test_xmlv2p1":
                            resource.GetAttribute("type").SetValue("imsqti_test_xmlv2p2");
                            break;
                        case "associatedcontent/xmlv1p0/learning-application-resource":
                            resource.GetAttribute("type").SetValue("webcontent");
                            break;
                    }
                });
        }
    }
}
