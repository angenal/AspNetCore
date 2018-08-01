using System;
using System.Collections.Generic;
using System.Text;

namespace WebCore.Attributes
{
    /// <summary>
    /// 文件大小 [默认最大1M]
    /// </summary>
    public class FileSizeAttribute : Attribute
    {
        public FileSizeAttribute(long maximumSize = 1, SizeUnit sizeUnit = SizeUnit.Megabytes)
        {
            MaximumSize = maximumSize;
            SizeUnit = sizeUnit;
        }

        /// <summary>
        /// 范围[小,大]
        /// </summary>
        /// <returns></returns>
        public Size[] Sizes()
        {
            return new Size[]
            {
                new Size(MinimumSize, SizeUnit),
                new Size(MaximumSize, SizeUnit)
            };
        }

        public long MaximumSize { get; }
        public long MinimumSize { get; set; }
        public SizeUnit SizeUnit { get; }
    }
}
