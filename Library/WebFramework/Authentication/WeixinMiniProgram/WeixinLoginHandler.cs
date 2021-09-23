using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebFramework.Authentication.WeixinMiniProgram
{
    /// <summary></summary>
    public class WeixinLoginHandler : RemoteAuthenticationHandler<WeixinLoginOptions>
    {
        /// <summary></summary>
        public WeixinLoginHandler(IOptionsMonitor<WeixinLoginOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock) { }
        /// <summary></summary>
        protected override async Task<HandleRequestResult> HandleRemoteAuthenticateAsync()
        {
            var code = Request.Query[Options.JsQuery].ToString();
            if (string.IsNullOrEmpty(code))
                return HandleRequestResult.Fail("没有找到客户端所提供的code供微信服务器进行验证。");

            using var tokens = await ExchangeCodeAsync(code);
            if (tokens.Error != null)
                return HandleRequestResult.Fail(tokens.Error);

            var completedContext = new WeixinServerResultContext(Context, Scheme, Options, tokens.SessionKey, tokens.OpenId, tokens.UnionId, tokens.ErrCode, tokens.ErrMsg);
            await Options.Events?.OnWeixinServerCompleted(completedContext);

            if (string.IsNullOrEmpty(tokens.OpenId) || string.IsNullOrEmpty(tokens.SessionKey))
                return HandleRequestResult.Fail("没有接收到微信服务器所返回的OpenID和SessionKey。");

            // 根据微信服务器返回的会话密匙执行登录操作, 比如颁发JWT, 缓存OpenId, 重定向Action等.
            if (Options.CustomerLoginState == null)
            {
                Logger.LogWarning("当前没有提供根据微信服务器返回的会话密匙执行登录操作的逻辑。");
                return HandleRequestResult.Handle();
            }
            try
            {
                string sessionInfoKey = null;
                var state = Context.RequestServices.GetService<IWeixinLoginStateInfoStore>();
                if (state != null) sessionInfoKey = await state.StoreAsync(new WeixinLoginSessionInfo(tokens.OpenId, tokens.SessionKey), Options);
                var customerLoginStateContext = new WeixinLoginStateContext(Context, Scheme, Options, tokens.SessionKey, tokens.OpenId, tokens.UnionId, tokens.ErrCode, tokens.ErrMsg, sessionInfoKey);
                await Options.CustomerLoginState.Invoke(customerLoginStateContext);
            }
            catch (Exception ex)
            {
                return HandleRequestResult.Fail(ex);
            }
            return HandleRequestResult.Handle();
        }
        /// <summary></summary>
        protected virtual async Task<WeixinPostResponse> ExchangeCodeAsync(string clientJsCode)
        {
            var queryString = new StringBuilder();
            queryString.Append($"?appid={Options.AppId}");
            queryString.Append($"&secret={Options.Secret}");
            queryString.Append($"&js_code={clientJsCode}");
            queryString.Append($"&grant_type={Options.GrantType}");

            var requestUri = $"{WeixinLoginDefaults.AuthorizationEndpoint}{queryString}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = await Options.Backchannel.SendAsync(request, Context.RequestAborted);
            var json = await response.Content?.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                var payload = JsonDocument.Parse(json);
                return WeixinPostResponse.Success(payload);
            }
            return WeixinPostResponse.Failed(new Exception($"请求微信服务端失败！{Environment.NewLine}{json}"));
        }
    }
}
