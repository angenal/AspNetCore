using Microsoft.AspNetCore.Mvc;
using Senparc.NeuChar.MessageHandlers;
using System;
using System.Text;
using System.Threading.Tasks;

namespace WebFramework.Weixins.MessageHandlers
{
    /// <summary>这个类型只用于特殊阶段：目前IOS版本微信有换行的bug，\r\n会识别为2行</summary>
    public class FixWeixinBugWeixinResult : ContentResult
    {
        private readonly IMessageHandlerDocument _messageHandlerDocument;

        /// <summary></summary>
        public FixWeixinBugWeixinResult(IMessageHandlerDocument messageHandlerDocument)
        {
            _messageHandlerDocument = messageHandlerDocument;
        }
        /// <summary></summary>
        public FixWeixinBugWeixinResult(string content)
        {
            base.Content = content;
        }
        /// <summary></summary>
        public new string Content
        {
            get
            {
                return base.Content ?? (_messageHandlerDocument != null && _messageHandlerDocument.TextResponseMessage != null
                    ? _messageHandlerDocument.TextResponseMessage.Replace("\r\n", "\n")
                    : null);
            }
            set { base.Content = value; }
        }
        /// <summary></summary>
        public override async Task ExecuteResultAsync(ActionContext context)
        {
            var content = Content;
            if (content == null)
            {
                if (_messageHandlerDocument == null)
                    throw new Senparc.Weixin.Exceptions.WeixinException("执行WeixinResult时提供的MessageHandler不能为空值null", null);

                var finalResponseDocument = _messageHandlerDocument.FinalResponseDocument;
                if (finalResponseDocument != null) content = finalResponseDocument.ToString();
            }

            content = (content ?? "").Replace("\r\n", "\n");
            var bytes = Encoding.UTF8.GetBytes(content);
            context.HttpContext.Response.ContentType = "text/xml";
            await context.HttpContext.Response.Body.WriteAsync(bytes.AsMemory(0, bytes.Length));
        }
    }
}
