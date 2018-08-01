using System;

namespace WebCore.Json.Parsing
{
    public interface IJsonParser : IDisposable
    {
        bool Read();
        void ValidateFloat();
        string GenerateErrorState();
    }
}