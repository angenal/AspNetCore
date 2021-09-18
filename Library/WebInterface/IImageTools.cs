using System;

namespace WebInterface
{
    public interface IImageTools
    {
        /// <summary>
        /// 获取验证码(缓存)
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        string GetCaptchaCode(ulong key);

        /// <summary>
        /// 生成验证码(缓存)返回key
        /// </summary>
        /// <param name="time">到期时间</param>
        /// <returns></returns>
        ulong NewCaptchaCode(DateTime time);

        /// <summary>
        /// 生成验证码图片
        /// </summary>
        /// <param name="captchaCode">验证码参数</param>
        /// <param name="width">图片宽度</param>
        /// <param name="height">图片高度</param>
        /// <param name="fontSize">字体大小</param>
        /// <param name="degree">难度系数 1.低 2.高</param>
        /// <returns></returns>
        byte[] NewCaptchaCode(string captchaCode, int width = 90, int height = 36, int fontSize = 20, int degree = 1);
    }
}
