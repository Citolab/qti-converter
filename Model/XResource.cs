using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using QtiPackageConverter.Helper;

namespace QtiPackageConverter.Model
{
    public class XResource
    {
        public string Identifier { get; set; }
        public XDocument Content { get; set; }
        private string Path { get; set; }
        public XResource(string path, string identifier, Func<string, string> replaceBeforeReading)
        {
            Identifier = identifier;
            var content = replaceBeforeReading(File.ReadAllText(path));
            Content = XDocument.Parse(content);
            Path = path;
        }

        public void Save(bool replaceMark = true)
        {
            Content.ForceTags(); // no self-closing tags
            var content = Content.ToString().Replace(@"xmlns=""""", string.Empty);
            if (replaceMark) content = content.Replace("UserSRMarker", "UserSRMarker_hide");
            File.WriteAllText(Path, content);
        }
    }

    public class XItem : XResource
    {
        public InteractionType Type { get; set; }

        public XItem(string path, string identifier, Func<string, string> replaceBeforeReading) : base(path, identifier, replaceBeforeReading)
        {
            Type = GetInteractionTypeFromXitem(Content);
        }

        private InteractionType GetInteractionTypeFromXitem(XDocument itemContent)
        {
            var interactions = itemContent.GetInteractions().ToList();
            if (interactions.Count > 1) return InteractionType.Combined;
            if (!interactions.Any()) return InteractionType.Info;
            switch (interactions.First().Name.LocalName)
            {
                case "textEntryInteraction":
                    return InteractionType.TextEntry;
                case "choiceInteraction":
                    return InteractionType.Choice;
                case "gapMatchInteraction":
                    return InteractionType.GapMatch;
                case "inlineChoiceInteraction":
                    return InteractionType.InlineChoice;
                case "extendedTextInteraction":
                    return InteractionType.ExtendedText;
                case "hottextInteraction":
                    return InteractionType.HotText;
                case "selectPointInteraction":
                    return InteractionType.SelectPoint;
                case "graphicAssociateInteraction":
                    return InteractionType.GraphicAssociate;
                case "graphicGapMatchInteraction":
                    return InteractionType.GraphicGapMatch;
                case "sliderInteraction":
                    return InteractionType.Slider;
                case "matchInteraction":
                    return InteractionType.MatchInteraction;
                case "orderInteraction":
                    return InteractionType.Order;
                default: return InteractionType.NotDetermined;
            }
        }
    }
}
