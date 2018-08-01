using System;
using System.Collections.Generic;
using System.Text;

namespace WebCore.Data.DTO
{
    /// <summary>
    /// 错误时返回错误码等信息 JSON数据包
    /// </summary>
    public class Result
    {
        public Result() { }

        /// <summary>
        /// 返回码
        /// </summary>
        public int errcode { get; set; }
        /// <summary>
        /// 返回信息[英文或中文]
        /// </summary>
        public string errmsg { get; set; }
    }
    public static class Results
    {
        public static readonly Result Ok = new Result() { errcode = 0, errmsg = "ok" };
        public static readonly Result SystemBusy = new Result() { errcode = -1, errmsg = "system busy" };
        public static Result Res(int code, string msg)
        {
            return new Result() { errcode = code, errmsg = msg };
        }
    }
}
