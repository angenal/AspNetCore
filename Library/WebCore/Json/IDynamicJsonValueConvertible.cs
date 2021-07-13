using WebCore.Json.Parsing;

namespace WebCore.Json
{
    public interface IDynamicJsonValueConvertible
    {
        DynamicJsonValue ToJson();
    }
}
