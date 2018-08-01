using System;

namespace WebCore.Annotations
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public sealed class NoEnumerationAttribute : Attribute
    {
    }
}
