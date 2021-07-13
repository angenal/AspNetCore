using System;
using System.Collections.Generic;
using System.Text;

namespace WebCore.Data.DTO
{
    public class ResInfo<T> : Result where T : class
    {
        public T info { get; set; }
    }
    public class ResData<TData, TPage> : Result where TData : class
    {
        public TData data { get; set; }
        public TPage page { get; set; }
    }
}
