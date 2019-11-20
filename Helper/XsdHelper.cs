using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;
using System.Xml.Schema;

namespace QtiPackageConverter.Helper
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml;
    using Microsoft.VisualBasic;

    public static class XsdHelper
    {
        public static XmlReaderSettings GetXmlReaderSettings(ValidationEventHandler eventHandler)
        {
            var settings = new XmlReaderSettings { ValidationType = ValidationType.Schema };
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            var tempXsdsPath = Path.Combine(Path.GetTempPath(), "qti-xsds");
            if (!Directory.Exists(tempXsdsPath))
            {
                var applicationPath = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                ZipFile.ExtractToDirectory(Path.Combine(applicationPath, "Xsds/schemas.zip"), tempXsdsPath);
                ZipFile.ExtractToDirectory(Path.Combine(applicationPath, "Xsds/schema_30.zip"), tempXsdsPath);
            }
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.Schemas.XmlResolver = new PreventLoadingExternalXsdXmlResolver(tempXsdsPath);
            
            //settings.Schemas.Add("http://www.w3.org/XML/1998/namespace", XmlReader.Create(Path.Combine(tempXsdsPath, @"xml\xml.xsd")));
            //settings.Schemas.Add("http://www.w3.org/2001/XInclude", XmlReader.Create(Path.Combine(tempXsdsPath, @"xinclude\XInclude.xsd")));
            //settings.Schemas.Add("http://www.w3.org/1998/Math/MathML", XmlReader.Create(Path.Combine(tempXsdsPath, @"mathml2\mathml2.xsd")));
            //settings.Schemas.Add("http://www.imsglobal.org/xsd/imslip_v1p0", XmlReader.Create(Path.Combine(tempXsdsPath, @"imslip_v1p0\imslip_v1p0.xsd")));
            //settings.Schemas.Add("http://www.imsglobal.org/xsd/apip/apipv1p0/imsapip_qtiv1p0", XmlReader.Create(Path.Combine(tempXsdsPath, @"apip\apipv1p0\imsapip_qtiv1p0\apipv1p0_qtiextv2p1_v1p0.xsd")));
            //settings.Schemas.Add("http://www.w3.org/1999/xhtml", XmlReader.Create(Path.Combine(tempXsdsPath, @"xhtml\xhtml11.xsd")));
            //settings.Schemas.Add("http://www.w3.org/2010/Math/MathML", XmlReader.Create(Path.Combine(tempXsdsPath, @"mathml3\mathml3.xsd")));
            //settings.Schemas.Add("http://www.imsglobal.org/xsd/imsqtiv2p2_html5_v1p0", XmlReader.Create(Path.Combine(tempXsdsPath, "imsqtiv2p2p1_html5_v1p0.xsd")));
            //settings.Schemas.Add("http://www.w3.org/2010/10/synthesis", XmlReader.Create(Path.Combine(tempXsdsPath, "ssmlv1p1-core.xsd")));

            AddQtiSchemasToValidate(ref settings, tempXsdsPath);

            settings.ValidationEventHandler += eventHandler;
            return settings;
        }


        private static void AddQtiSchemasToValidate(ref XmlReaderSettings settings, string controlxsdsPath)
        {
            settings.Schemas.Add("http://www.imsglobal.org/xsd/imsqtiasi_v3p0", XmlReader.Create(Path.Combine(controlxsdsPath, "imsqti_asiv3p0_v1p0.xsd")));
            //settings.Schemas.Add("http://www.imsglobal.org/xsd/qti/qtiv3p0/imscp_v1p1", XmlReader.Create(Path.Combine(controlxsdsPath, "imsqtiv3p0_imscpv1p2_v1p0.xsd")));
            //settings.Schemas.Add("http://www.imsglobal.org/xsd/imsqti_metadata_v3p0", XmlReader.Create(Path.Combine(controlxsdsPath, "imsqti_metadatav3p0_v1p0.xsd")));
        }
    }

    public class PreventLoadingExternalXsdXmlResolver : XmlUrlResolver
    {
        private readonly DirectoryInfo _localXsdPath;
        private readonly HashSet<string> _exceptionList;
        public PreventLoadingExternalXsdXmlResolver(string localXsdPath)
        {
            _localXsdPath = new DirectoryInfo(localXsdPath);
            _exceptionList = new HashSet<string>()
            {
                //"ssmlv1p1-core.xsd"
            };
        }
        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            if (!absoluteUri.Scheme.StartsWith("http"))
            {
                return base.GetEntity(absoluteUri, role, ofObjectToReturn);
            }
            var filename = Path.GetFileName(absoluteUri.ToString());
            var localFile = _localXsdPath.GetFiles(filename, SearchOption.AllDirectories).FirstOrDefault();
            if (localFile != null && !_exceptionList.Contains(filename))
            {
                var localUrl = new Uri(localFile.FullName);
                return base.GetEntity(localUrl, role, ofObjectToReturn);
            };
            return null;
            //    base.GetEntity(absoluteUri, role, ofObjectToReturn) : null;

        }
    }

}
