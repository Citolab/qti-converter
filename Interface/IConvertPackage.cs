using QtiPackageConverter.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace QtiPackageConverter.Interface
{
    public interface IConvertPackage
    {
        QtiVersion ConvertsTo { get; }
        void Convert();
    }
}
