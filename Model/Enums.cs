using System;
using System.Collections.Generic;
using System.Text;

namespace QtiPackageConverter.Model
{
    public enum QtiVersion
    {
        Qti21,
        Qti22,
        Qti30
    }

    public enum QtiResourceType
    {
        AssessmentItem,
        AssessmentTest,
        Manifest
    }
    public enum InteractionType
    {
        Choice,
        TextEntry,
        GapMatch,
        InlineChoice,
        ExtendedText,
        HotText,
        Info,
        SelectPoint,
        GraphicAssociate,
        GraphicGapMatch,
        Slider,
        MatchInteraction,
        Order,
        Combined,
        NotDetermined
    }
}
