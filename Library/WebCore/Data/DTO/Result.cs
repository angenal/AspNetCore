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
        public int Code { get; set; }
        /// <summary>
        /// 返回信息[英文或中文]
        /// </summary>
        public string Message { get; set; }
    }
    public static class Results
    {
        public static readonly Result Ok = New(0, "ok");
        public static readonly Result Error = New(400, "error");
        public static readonly Result Unauthorized = New(401, "not authorized"); // 未经授权-未登录或登录过期
        public static readonly Result Forbid = New(403, "forbidden"); // 禁止访问-无权限或账号异常
        public static readonly Result NotFound = New(404, "not found");
        public static readonly Result Timeout = New(408, "timeout");
        public static readonly Result SystemBusy = New(-1, "system busy"); // 系统无响应
        public static readonly Result SystemException = New(500, "system exception"); // 系统内部异常
        public static Result New(int code, string msg)
        {
            return new Result() { Code = code, Message = msg };
        }
    }
}
