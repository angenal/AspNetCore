﻿using System;

namespace WebCore.Json
{
    /// <summary>
    /// Instructs the <see cref="JsonDeserializationBase"/> not to serialize the public field or public read/write property value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class JsonDeserializationIgnoreAttribute : Attribute
    {
    }
}
