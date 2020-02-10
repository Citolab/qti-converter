using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Xml.Schema;
using QtiPackageConverter.Model;

namespace QtiPackageConverter.Helper
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Xml;

    public static class XsdHelper
    {
        public static Dictionary<string, string> BaseSchemaLocations = new Dictionary<string, string>
        {
            {$"{QtiResourceType.AssessmentItem.ToString()}-{QtiVersion.Qti21.ToString()}", "http://www.imsglobal.org/xsd/qti/qtiv2p1/imsqti_v2p1.xsd"},
            {$"{QtiResourceType.AssessmentItem.ToString()}-{QtiVersion.Qti22.ToString()}", "http://www.imsglobal.org/xsd/qti/qtiv2p2/imsqti_v2p2p2.xsd"},
            {$"{QtiResourceType.AssessmentItem.ToString()}-{QtiVersion.Qti30.ToString()}", "https://purl.imsglobal.org/spec/qti/v3p0/schema/xsd/imsqti_asiv3p0_v1p0.xsd"},
            {$"{QtiResourceType.AssessmentTest.ToString()}-{QtiVersion.Qti21.ToString()}",  "http://www.imsglobal.org/xsd/qti/qtiv2p1/imsqti_v2p1.xsd"},
            {$"{QtiResourceType.AssessmentTest.ToString()}-{QtiVersion.Qti22.ToString()}", "http://www.imsglobal.org/xsd/qti/qtiv2p2/imsqti_v2p2p2.xsd"},
            {$"{QtiResourceType.AssessmentTest.ToString()}-{QtiVersion.Qti30.ToString()}", "https://purl.imsglobal.org/spec/qti/v3p0/schema/xsd/imsqti_asiv3p0_v1p0.xsd"},
            {$"{QtiResourceType.Manifest.ToString()}-{QtiVersion.Qti21.ToString()}", "http://www.imsglobal.org/xsd/imscp_v1p2.xsd"},
            {$"{QtiResourceType.Manifest.ToString()}-{QtiVersion.Qti22.ToString()}", " http://www.imsglobal.org/xsd/qti/qtiv2p2/qtiv2p2_imscpv1p2_v1p0.xsd"},
            {$"{QtiResourceType.Manifest.ToString()}-{QtiVersion.Qti30.ToString()}", "https://purl.imsglobal.org/spec/qti/v3p0/schema/xsd/imsqti_asiv3p0_v1p0.xsd"},
        };

        public static Dictionary<string, string> BaseSchemas = new Dictionary<string, string>
        {
            {$"{QtiResourceType.AssessmentItem.ToString()}-{QtiVersion.Qti21.ToString()}", "http://www.imsglobal.org/xsd/imsqti_v2p1"},
            {$"{QtiResourceType.AssessmentItem.ToString()}-{QtiVersion.Qti22.ToString()}", "http://www.imsglobal.org/xsd/imsqti_v2p2"},
            {$"{QtiResourceType.AssessmentItem.ToString()}-{QtiVersion.Qti30.ToString()}", "http://www.imsglobal.org/xsd/imsqtiasi_v3p0"},
            {$"{QtiResourceType.AssessmentTest.ToString()}-{QtiVersion.Qti21.ToString()}", "http://www.imsglobal.org/xsd/imsqti_v2p1"},
            {$"{QtiResourceType.AssessmentTest.ToString()}-{QtiVersion.Qti22.ToString()}", "http://www.imsglobal.org/xsd/imsqti_v2p2"},
            {$"{QtiResourceType.AssessmentTest.ToString()}-{QtiVersion.Qti30.ToString()}", "http://www.imsglobal.org/xsd/imsqtiasi_v3p0"},
            {$"{QtiResourceType.Manifest.ToString()}-{QtiVersion.Qti21.ToString()}", "http://www.imsglobal.org/xsd/imscp_v1p1"},
            {$"{QtiResourceType.Manifest.ToString()}-{QtiVersion.Qti22.ToString()}", "http://www.imsglobal.org/xsd/imscp_v1p1"},
            {$"{QtiResourceType.Manifest.ToString()}-{QtiVersion.Qti30.ToString()}", "http://www.imsglobal.org/xsd/qti/qtiv3p0/imscp_v1p1"},
        };

        private static XmlReaderSettings Qti22ReaderSettings = null;
        private static XmlReaderSettings Qti30ReaderSettings = null;

        private static XmlReaderSettings GetXmlReaderSettings(ValidationEventHandler eventHandler)
        {
            var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema };
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            var tempXsdsPath = Path.Combine(Path.GetTempPath(), "qti-xsds");
            var applicationPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            ZipFile.ExtractToDirectory(Path.Combine(applicationPath, "Xsds/schemas.zip"), tempXsdsPath, Encoding.Default, true);
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.Schemas.XmlResolver = new PreventLoadingExternalXsdXmlResolver();

            settings.Schemas.Add("http://www.w3.org/XML/1998/namespace", XmlReader.Create(Path.Combine(tempXsdsPath, @"xml\xml.xsd")));
            settings.Schemas.Add("http://www.w3.org/2001/XInclude", XmlReader.Create(Path.Combine(tempXsdsPath, @"xinclude\XInclude.xsd")));
            settings.Schemas.Add("http://www.w3.org/1998/Math/MathML", XmlReader.Create(Path.Combine(tempXsdsPath, @"mathml2\mathml2.xsd")));
            settings.Schemas.Add("http://www.imsglobal.org/xsd/imslip_v1p0", XmlReader.Create(Path.Combine(tempXsdsPath, @"imslip_v1p0\imslip_v1p0.xsd")));
            settings.Schemas.Add("http://www.imsglobal.org/xsd/apip/apipv1p0/imsapip_qtiv1p0", XmlReader.Create(Path.Combine(tempXsdsPath, @"apip\apipv1p0\imsapip_qtiv1p0\apipv1p0_qtiextv2p1_v1p0.xsd")));
            settings.Schemas.Add("http://www.w3.org/1999/xhtml", XmlReader.Create(Path.Combine(tempXsdsPath, @"xhtml\xhtml11.xsd")));
            settings.Schemas.Add("http://www.w3.org/2010/Math/MathML", XmlReader.Create(Path.Combine(tempXsdsPath, @"mathml3\mathml3.xsd")));
            settings.Schemas.Add("http://www.imsglobal.org/xsd/imsqtiv2p2_html5_v1p0", XmlReader.Create(Path.Combine(tempXsdsPath, "imsqtiv2p2p1_html5_v1p0.xsd")));
            settings.Schemas.Add("http://www.w3.org/2010/10/synthesis", XmlReader.Create(Path.Combine(tempXsdsPath, "ssmlv1p1-core.xsd")));
            settings.ValidationEventHandler += eventHandler;
            return settings;
        }
        public static XmlReaderSettings GetXmlReaderSettingsQti22(ValidationEventHandler eventHandler, bool force = false)
        {
            if (Qti22ReaderSettings == null)
            {
                var tempXsdsPath = Path.Combine(Path.GetTempPath(), "qti-xsds-22");
                if (!Directory.Exists(tempXsdsPath) || force)
                {
                    var applicationPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                    ZipFile.ExtractToDirectory(Path.Combine(applicationPath, "Xsds/schema_qti22.zip"), tempXsdsPath,
                        Encoding.UTF8, true);
                }
                var settings = GetXmlReaderSettings(eventHandler);
                settings.Schemas.Add("http://www.imsglobal.org/xsd/imsqti_v2p2",

                XmlReader.Create(Path.Combine(tempXsdsPath, "imsqti_v2p2p1.xsd")));
                settings.Schemas.Add("http://www.imsglobal.org/xsd/imscp_v1p1",
                    XmlReader.Create(Path.Combine(tempXsdsPath, "imscp_v1p2.xsd")));
                settings.Schemas.Add("http://www.imsglobal.org/xsd/imsqti_metadata_v2p2",
                    XmlReader.Create(Path.Combine(tempXsdsPath, "imsqti_metadata_v2p2.xsd")));
                Qti22ReaderSettings = settings;
            }
            return Qti22ReaderSettings;
        }

        public static XmlReaderSettings GetXmlReaderSettingsQti30(ValidationEventHandler eventHandler, bool force = false)
        {
            if (Qti30ReaderSettings == null)
            {
                var tempXsdsPath = Path.Combine(Path.GetTempPath(), "qti-xsds-30");
                if (!Directory.Exists(tempXsdsPath) || force)
                {
                    var applicationPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                    ZipFile.ExtractToDirectory(Path.Combine(applicationPath, "Xsds/schema_qti30.zip"), tempXsdsPath,
                        Encoding.UTF8, true);
                }
                var settings = GetXmlReaderSettings(eventHandler);
                settings.Schemas.Add("http://www.imsglobal.org/xsd/imsqtiasi_v3p0",
                    XmlReader.Create(Path.Combine(tempXsdsPath, "imsqti_asiv3p0_v1p0.xsd")));
                settings.Schemas.Add("http://ltsc.ieee.org/xsd/LOM",
                    XmlReader.Create(Path.Combine(tempXsdsPath, "imsmd_loose_v1p3p2.xsd")));
                settings.Schemas.Add("http://www.imsglobal.org/xsd/qti/qtiv3p0/imscp_v1p1",
                    XmlReader.Create(Path.Combine(tempXsdsPath, "imsqtiv3p0_imscpv1p2_v1p0.xsd")));
                settings.Schemas.Add("http://www.imsglobal.org/xsd/imsqti_metadata_v3p0",
                    XmlReader.Create(Path.Combine(tempXsdsPath, "imsqti_metadatav3p0_v1p0.xsd")));
                settings.Schemas.Add("http://www.imsglobal.org/xsd/qti/qtiv3p0/imscp_extensionv1p2",
                    XmlReader.Create(Path.Combine(tempXsdsPath, "imsqtiv3p0_cpextv1p2_v1p0.xsd")));
                Qti30ReaderSettings = settings;
            }
            return Qti30ReaderSettings;
        }
    }


    public class PreventLoadingExternalXsdXmlResolver : XmlUrlResolver
    {
        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            if (absoluteUri.Scheme != "http" && absoluteUri.Scheme != "https")
                return base.GetEntity(absoluteUri, role, ofObjectToReturn);
            else
                return null;
        }

    }

}
