using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using QtiPackageConverter.Helper;
using QtiPackageConverter.Interface;
using QtiPackageConverter.Model;

namespace QtiPackageConverter.Implementations.Qti22
{
    public class ConvertItem : IConvertItemType
    {
        private readonly XItem _item;
        private readonly Manifest _manifest;

        public ConvertItem(XItem item, Manifest manifest)
        {
            _item = item;
            _manifest = manifest;
        }

        public void Convert()
        {
            _item.Save();
        }
    }
}
