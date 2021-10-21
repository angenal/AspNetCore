using Microsoft.AspNetCore.Authentication.QQ;
using Microsoft.AspNetCore.Authentication.WeChat;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Threading.Tasks;
using WebCore;
using WebFramework;

namespace ApiDemo.NET5.Controllers
{
    public partial class UserController
    {

        // api/User/QQLogin

        /// <summary>
        /// QQ登陆
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <param name="remoteError"></param>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Produces(Produces.JSON)]
        [ProducesResponseType(typeof(Session), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> QQLogin(string returnUrl = null, string remoteError = null)
        {
            if (!string.IsNullOrEmpty(remoteError)) return BadRequest(remoteError);

            // get information from HttpContext using Microsoft.AspNetCore.Authentication.QQ
            var loginInfo = await HttpContext.GetExternalQQLoginInfoAsync();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                //cache.Set();
            }

            return Ok(loginInfo);
        }

        // api/User/WxLogin

        /// <summary>
        /// 微信登陆
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <param name="remoteError"></param>
        [HttpGet]
        [AllowAnonymous]
        [Produces(Produces.JSON)]
        [ProducesResponseType(typeof(Session), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> WxLogin(string returnUrl = null, string remoteError = null)
        {
            if (!string.IsNullOrEmpty(remoteError)) return BadRequest(remoteError);

            // get information from HttpContext using Microsoft.AspNetCore.Authentication.Weixin
            var loginInfo = await HttpContext.GetExternalWeChatLoginInfoAsync();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                //cache.Set();
            }

            return Ok(loginInfo);
        }

        // api/User/Info

        /// <summary>
        /// 获取登陆状态
        /// </summary>
        [HttpGet]
        [Produces(Produces.JSON)]
        [ProducesResponseType(typeof(Session), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.Unauthorized)]
        public ActionResult Info()
        {
            return Ok(user);
        }

    }
}
