using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using System.Net.Http;

namespace Microsoft.AspNetCore.Authentication.WxOpen
{
    /// <summary></summary>
    public class WxOpenPostConfigureOptions : IPostConfigureOptions<WxOpenLoginOptions>
    {
        private readonly IDataProtectionProvider _dp;

        /// <summary></summary>
        public WxOpenPostConfigureOptions(IDataProtectionProvider dataProtection)
        {
            _dp = dataProtection;
        }

        /// <summary></summary>
        public void PostConfigure(string name, WxOpenLoginOptions options)
        {
            if (options.DataProtectionProvider == null) options.DataProtectionProvider = _dp;
            if (options.Backchannel != null) return;

            options.Backchannel = new HttpClient(options.BackchannelHttpHandler ?? new HttpClientHandler());
            options.Backchannel.Timeout = options.BackchannelTimeout;
            options.Backchannel.MaxResponseContentBufferSize = 1024 * 1024 * 10; // 10 MB
            options.Backchannel.DefaultRequestHeaders.Accept.ParseAdd("*/*");
            options.Backchannel.DefaultRequestHeaders.UserAgent.ParseAdd("Microsoft ASP.NET Core");
            options.Backchannel.DefaultRequestHeaders.ExpectContinue = false;
        }
    }
}
