using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using QtiPackageConverter.Helper;
using QtiPackageConverter.Interface;
using QtiPackageConverter.Model;

namespace QtiPackageConverter.Implementations.Qti30
{
    public class ConvertItem : IConvertItemType
    {
        private static int _idsGeneratedCount = 0;
        private readonly XItem _item;
        private readonly Manifest _manifest;
        private readonly HashSet<string> _tagNameWithoutQtiPrefix;

        public ConvertItem(XItem item, Manifest manifest, HashSet<string> tagNameWithoutQtiPrefix)
        {
            _item = item;
            _manifest = manifest;
            _tagNameWithoutQtiPrefix = tagNameWithoutQtiPrefix;
        }

        public void Convert()
        {
            XNamespace xNamespace = "http://www.imsglobal.org/xsd/imsqtiasi_v3p0";
            var assessmentItem = _item.Content
                .FindElementByName("assessmentItem");
            var id = assessmentItem?.GetAttribute("identifier");
            if (string.IsNullOrEmpty(id.Value) || !id.Value[0].IsAlphaNumeric())
            {
                id.SetValue(_item.Identifier);
            }
            assessmentItem?.GetAttribute("identifier")?.SetValue(_item.Identifier);
            // fix identifiers not starting with an alpanumeric 
            _item.Content.GetAttributes("identifier").ToList().ForEach(attr =>
            {
                if (string.IsNullOrEmpty(attr.Value) || !attr.Value[0].IsAlphaNumeric())
                {
                    attr.SetValue($"id{_idsGeneratedCount}-{attr.Value}");
                    _idsGeneratedCount++;
                }
            });
            var isChanged = false;


            // for now, remove rubricBlock and infoControl because there's much change in this part and we don't use it.
            foreach (var rubricBlock in _item.Content.FindElementsByName("rubricBlock"))
            {
                rubricBlock.Remove();
            }
            foreach (var infoControl in _item.Content.FindElementsByName("infoControl"))
            {
                infoControl.Remove();
            }
            foreach (var element in _item.Content.Descendants())
            {
                var tagName = element.Name.LocalName;
                var kebabTagName = tagName.ToKebabCase();
                if (!_tagNameWithoutQtiPrefix.Contains(tagName))
                {
                    isChanged = true;
                    element.Name = xNamespace + $"qti-{kebabTagName}";
                }
            }

            // fix attributes
            foreach (var element in _item.Content.Descendants())
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
                        isChanged = true;
                        var newAttr = new XAttribute($"{kebabAttributeName}", attribute.Value);
                        attributesToRemove.Add(attribute);
                        attributesToAdd.Add(newAttr);
                    }
                }
                attributesToRemove.ForEach(a => a.Remove());
                attributesToAdd.ForEach(a => element.Add(a));
            }
            if (isChanged)
            {
                _item.Save();

            }
        }
    }

    public class CustomValueChange
    {
        public string NewAttributeValue { get; }
        public string NewAttributeName { get; }

        public CustomValueChange(string newAttributeName, string newAttributeValue)
        {
            NewAttributeValue = newAttributeValue;
            NewAttributeName = newAttributeName;
        }
    }


}
