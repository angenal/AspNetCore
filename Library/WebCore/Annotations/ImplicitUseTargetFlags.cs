using System;

namespace WebCore.Annotations
{
    [Flags]
    public enum ImplicitUseTargetFlags
    {
        Itself = 0,

        Default = 1,

        Members = 2,

        WithMembers = 3
    }
}
