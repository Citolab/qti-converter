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
    public class ConvertStyling : IConvertItemType
    {
        private readonly XItem _item;

        public ConvertStyling(XItem item)
        {
            _item = item;
        }

        public InteractionType InteractionType { get; }

        public void Convert()
        {
            XNamespace xNamespace = "http://www.imsglobal.org/xsd/imsqtiasi_v3p0";
            var body = _item.Content.FindElementByName("qti-item-body");

            var sb = new StringBuilder();
            if (_item.Type == InteractionType.TextEntry)
            {
                sb.Append($@"<div xmlns=""{xNamespace.NamespaceName}"" class=""qti-layout-row"">");
                sb.Append(@"<div id=""column-left"" class=""qti-layout-col6"">");
                sb.Append(@"</div>");
                sb.Append(@"<div id=""column-right"" class=""qti-layout-col6"">");
                sb.Append(@"</div>");
                sb.Append(@"</div>");
            }
            else
            {
                sb.Append($@"<div xmlns=""{xNamespace.NamespaceName}"" class=""qti-layout-row"">");
                sb.Append(@"<div id=""column-center"" class=""qti-layout-col12"">");
                sb.Append(@"</div>");
                sb.Append(@"</div>");
            }

            // create a two column layout and current elements in the new columns
            var newElement = XElement.Parse(sb.ToString());


            var firstColumn = newElement.FindElementsByElementAndAttributeValue("div", "id", "column-left")
                                  .FirstOrDefault() ??
                              newElement.FindElementsByElementAndAttributeValue("div", "id", "column-center")
                                  .FirstOrDefault();

            var bodyElements = body.FindElementsByElementAndAttributeValue("div", "class", "questify_bodyWrapper");
            foreach (var xElement in bodyElements.Elements())
            {
                var added = false;
                if (xElement.Name.LocalName == "p")
                {
                    if (string.IsNullOrWhiteSpace(xElement.Value))
                    {
                        xElement.Elements().ToList().ForEach(e => firstColumn?.Add(new XElement(e)));
                        added = true;
                    }
                }
                if (!added)
                {
                    firstColumn?.Add(new XElement(xElement));
                }
 
            }

            var currentPrompt =
                body.FindElementsByElementAndAttributeValue("div", "class", "questify_questionWrapper")
                    .FirstOrDefault()?
                    .Elements()
                    .Select(el => el.ToString())
                    .ToArray();
            var promptString = currentPrompt != null ? string.Join(' ', currentPrompt) : string.Empty;

            var interaction = body.GetInteraction();
            if (interaction != null && _item.Type == InteractionType.Choice)
            {
                var classAttribute = interaction.GetAttribute("class");
                if (classAttribute != null)
                {
                    classAttribute.Value = classAttribute.Value
                        .Replace("horizontal", "qti-orientation-horizontal");
                }
                var promptElement = XElement.Parse($@"<qti-prompt xmlns=""{xNamespace.NamespaceName}"">{promptString}</qti-prompt>");
                interaction.AddFirst(promptElement);
                firstColumn?.Add(new XElement(interaction));
            }
            else if (interaction != null)
            {
                var attr = interaction.GetAttribute("class");
                attr.Value = "type:numpad " + attr.Value;
                var paragraphWithInteraction =
                    body.FindElementsByElementAndAttributeValue("div", "id", "questify_textAndInteractionsWrapper")
                        .FirstOrDefault()?
                        .Elements()
                        .FirstOrDefault(); // this is a paragraph;
                if (paragraphWithInteraction != null)
                {
                    paragraphWithInteraction.Name = "div";
                }
                //var divWithInteraction = XElement.Parse(paragraphWithInteraction.
                var promptElement = XElement.Parse($@"<div class=""prompt"" xmlns=""{xNamespace.NamespaceName}"">{promptString}</div>");
                firstColumn?.Add(promptElement);
                interaction?.Elements().Remove();
                firstColumn?.Add(new XElement(paragraphWithInteraction ?? interaction));
            }
            foreach (var bodyElement in body.Elements().ToList())
            {
                bodyElement.Remove();
            }
            body.Add(newElement);
            _item.Content = XDocument.Parse(_item.Content.ToString().ReplaceAllOccurrenceExceptFirst($@"xmlns=""{xNamespace.NamespaceName}""", string.Empty));
            _item.Save();

        }
    }
}
