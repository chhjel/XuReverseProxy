using XuReverseProxy.Core.Attributes;

namespace XuReverseProxy.Core.Models.Enums;


[GenerateFrontendModel]
[Flags]
public enum DayOfWeekFlags
{
    None,
    Monday = 1,
    Tuesday = 2,
    Wednesday = 4,
    Thursday = 8,
    Friday = 16,
    Saturday = 32,
    Sunday = 64
}
