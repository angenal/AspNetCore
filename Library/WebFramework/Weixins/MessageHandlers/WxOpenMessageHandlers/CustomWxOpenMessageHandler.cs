using Senparc.NeuChar.Entities;
using Senparc.Weixin.WxOpen.Entities;
using Senparc.Weixin.WxOpen.Entities.Request;
using Senparc.Weixin.WxOpen.MessageHandlers;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace WebFramework.Weixins.MessageHandlers.WxOpenMessageHandlers
{
    public class CustomWxOpenMessageHandler : WxOpenMessageHandler<CustomWxOpenMessageContext>
    {
        public string appId = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppId;
        //private string appSecret = Senparc.Weixin.Config.SenparcWeixinSetting.WxOpenAppSecret;

        public CustomWxOpenMessageHandler(Stream inputStream, PostModel postModel, int maxRecordCount = 0)
            : base(inputStream, postModel, maxRecordCount)
        {
            //这里设置仅用于测试，实际开发可以在外部更全局的地方设置，
            //比如MessageHandler<MessageContext>.GlobalGlobalMessageContext.ExpireMinutes = 3。
            GlobalMessageContext.ExpireMinutes = 3;

            if (!string.IsNullOrEmpty(postModel.AppId))
            {
                appId = postModel.AppId;//通过第三方开放平台发送过来的请求
            }

            //在指定条件下，不使用消息去重
            OmitRepeatedMessageFunc = requestMessage =>
            {
                var textRequestMessage = requestMessage as RequestMessageText;
                if (textRequestMessage != null && textRequestMessage.Content == "容错")
                {
                    return false;
                }
                return true;
            };
        }

        public override XDocument ResponseDocument
        {
            get { return new XDocument(); }//暂时没有需要输出的XML格式内容
        }

        public override XDocument FinalResponseDocument
        {
            get { return new XDocument(); }//暂时没有需要输出的XML格式内容
        }

        //public override void OnExecuting()
        //{
        //    //测试MessageContext.StorageData
        //    if (CurrentMessageContext.StorageData == null)
        //    {
        //        CurrentMessageContext.StorageData = 0;
        //    }
        //    base.OnExecuting();
        //}

        //public override void OnExecuted()
        //{
        //    base.OnExecuted();
        //    CurrentMessageContext.StorageData = ((int)CurrentMessageContext.StorageData) + 1;
        //}


        /// <summary>
        /// 处理文字请求
        /// </summary>
        /// <returns></returns>
        public override async Task<IResponseMessageBase> OnTextRequestAsync(RequestMessageText requestMessage)
        {
            //这里可以进行数据库记录或处理

            //发送一条客服消息回复用户

            var contentUpper = requestMessage.Content.ToUpper();
            if (contentUpper == "LINK")
            {
                //发送客服消息
                await Senparc.Weixin.WxOpen.AdvancedAPIs.CustomApi.SendLinkAsync(appId, OpenId, "欢迎使用 Senparc.Weixin SDK", "感谢大家的支持！\r\n\r\n盛派永远在你身边！",
                    "https://weixin.senparc.com", "https://sdk.weixin.senparc.com/images/book-cover-front-small-3d-transparent.png");
            }
            else if (contentUpper == "CARD")
            {
                //上传封面临时素材
                var uploadResult = Senparc.Weixin.MP.AdvancedAPIs.MediaApi.UploadTemporaryMedia(appId, Senparc.Weixin.MP.UploadMediaFileType.image, Server.GetMapPath("~/UpLoadFiles/web/20180910/logo.png"));

                //发送客服消息
                await Senparc.Weixin.WxOpen.AdvancedAPIs.CustomApi.SendMiniProgramPageAsync(appId, OpenId, "欢迎使用 Senparc.Weixin SDK", "pages/web/index", uploadResult.media_id);
            }
            else
            {

                var result = new StringBuilder();
                result.AppendFormat("您刚才发送了文字信息：{0}\r\n\r\n", requestMessage.Content);

                var context = GetCurrentMessageContext().Result;
                if (context.RequestMessages.Count > 1)
                {
                    result.AppendFormat("您刚才还发送了如下消息（{0}/{1}）：\r\n", context.RequestMessages.Count,
                        context.StorageData);
                    for (int i = context.RequestMessages.Count - 2; i >= 0; i--)
                    {
                        var historyMessage = context.RequestMessages[i];
                        string content;
                        if (historyMessage is RequestMessageText)
                        {
                            content = (historyMessage as RequestMessageText).Content;
                        }
                        else if (historyMessage is RequestMessageEvent_UserEnterTempSession)
                        {
                            content = "[进入客服]";
                        }
                        else
                        {
                            content = string.Format("[非文字信息:{0}]", historyMessage.GetType().Name);
                        }

                        result.AppendFormat("{0} 【{1}】{2}\r\n",
                            historyMessage.CreateTime.ToString("HH:mm:ss"),
                            historyMessage.MsgType.ToString(),
                            content
                            );
                    }
                    result.AppendLine("\r\n");
                }

                //处理微信换行符识别问题
                var msg = result.ToString().Replace("\r\n", "\n");

                //发送客服消息
                Senparc.Weixin.WxOpen.AdvancedAPIs.CustomApi.SendText(appId, OpenId, msg);

                //也可以使用微信公众号的接口，完美兼容：
                //Senparc.Weixin.MP.AdvancedAPIs.CustomApi.SendText(appId, WeixinOpenId, msg);
            }

            return new SuccessResponseMessage();
        }

        public override async Task<IResponseMessageBase> OnImageRequestAsync(RequestMessageImage requestMessage)
        {
            //发来图片，进行处理
            await Senparc.Weixin.WxOpen.AdvancedAPIs.CustomApi.SendTextAsync(appId, OpenId, "刚才您发送了这张图片：");
            await Senparc.Weixin.WxOpen.AdvancedAPIs.CustomApi.SendImageAsync(appId, OpenId, requestMessage.MediaId);
            return DefaultResponseMessage(requestMessage);
        }

        public override IResponseMessageBase OnEvent_UserEnterTempSessionRequest(RequestMessageEvent_UserEnterTempSession requestMessage)
        {
            //进入客服
            var msg = @"欢迎您！这条消息来自 Senparc.Weixin 进入客服事件。

您可以进行以下测试：
1、发送任意文字，返回上下文消息记录
2、发送图片，返回同样的图片
3、发送文字“link”,返回图文链接
4、发送文字“card”，发送小程序卡片";
            Senparc.Weixin.WxOpen.AdvancedAPIs.CustomApi.SendText(appId, OpenId, msg);

            return DefaultResponseMessage(requestMessage);
        }

        public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {
            //所有没有被处理的消息会默认返回这里的结果

            return new SuccessResponseMessage();

            //return new SuccessResponseMessage();等效于：
            //base.TextResponseMessage = "success";
            //return null;
        }
    }
}
