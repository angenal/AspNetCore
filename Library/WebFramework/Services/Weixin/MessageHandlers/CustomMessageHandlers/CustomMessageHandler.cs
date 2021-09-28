using Senparc.NeuChar.Entities;
using Senparc.Weixin;
using Senparc.Weixin.MP.Entities;
using Senparc.Weixin.MP.Entities.Request;
using Senparc.Weixin.MP.MessageHandlers;
using System.Collections.Generic;
using System.Xml.Linq;

namespace WebFramework.Services.Weixin.MessageHandlers.CustomMessageHandlers
{
    /// <summary>
    /// 自定义 微信公众号消息接收处理方法 MessageHandler
    /// </summary>
    public partial class CustomMessageHandler : MessageHandler<CustomMessageContext>
    {
        /*
         * 重要提示：v1.5起，MessageHandler提供了一个DefaultResponseMessage的抽象方法，
         * DefaultResponseMessage必须在子类中重写，用于返回没有处理过的消息类型（也可以用于默认消息，如帮助信息等）；
         * 其中所有原OnXX的抽象方法已经都改为虚方法，可以不必每个都重写。若不重写，默认返回DefaultResponseMessage方法中的结果。
         */

        //下面的Url和Token可以用其他平台的消息，或者到www.weiweihi.com注册微信用户，将自动在“微信营销工具”下得到
        private string agentUrl = Config.SenparcWeixinSetting.AgentUrl;//这里使用了www.weiweihi.com微信自动托管平台
        private string agentToken = Config.SenparcWeixinSetting.AgentToken;//Token
        //private string wiweihiKey = Config.SenparcWeixinSetting.SenparcWechatAgentKey;//WeiweihiKey专门用于对接www.Weiweihi.com平台，获取方式见：http://www.weiweihi.com/ApiDocuments/Item/25#51

        public string appId = Config.SenparcWeixinSetting.WeixinAppId;
        private string appSecret = Config.SenparcWeixinSetting.WeixinAppSecret;

        /// <summary>
        /// 模板消息集合（Key：checkCode，Value：OpenId）
        /// </summary>
        public static Dictionary<string, string> TemplateMessageCollection = new Dictionary<string, string>();

        public CustomMessageHandler(System.IO.Stream inputStream, PostModel postModel, int maxRecordCount = 0)
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
                //var textRequestMessage = requestMessage as RequestMessageText;
                //if (textRequestMessage != null && textRequestMessage.Content == "容错")
                //{
                //    return false;
                //}
                return true;
            };
        }

        public CustomMessageHandler(XDocument requestDoc, PostModel postModel = null, int maxRecordCount = 0)
            : base(requestDoc, postModel, maxRecordCount)
        {
        }

        public CustomMessageHandler(RequestMessageBase requestMessage, PostModel postModel)
            : base(requestMessage, postModel)
        {
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
        public override IResponseMessageBase OnTextRequest(RequestMessageText requestMessage)
        {
            //return CurrentMessageHandlerNode.Execute(requestMessage, this, appId);
            var service = new TextMessages();
            var responseMessage = service.GetResponseMessage(requestMessage, this);
            return responseMessage;
        }

        /// <summary>
        /// 处理位置请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnLocationRequest(RequestMessageLocation requestMessage)
        {
            var service = new LocationMessages();
            var responseMessage = service.GetResponseMessage(requestMessage, this);
            return responseMessage;
        }

        ///// <summary>
        ///// 处理小视频请求
        ///// </summary>
        ///// <param name="requestMessage"></param>
        ///// <returns></returns>
        //public override IResponseMessageBase OnShortVideoRequest(RequestMessageShortVideo requestMessage)
        //{
        //    var responseMessage = CreateResponseMessage<ResponseMessageText>();
        //    responseMessage.Content = "不支持小视频的处理";
        //    return responseMessage;
        //}

        /// <summary>
        /// 处理图片请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnImageRequest(RequestMessageImage requestMessage)
        {
            var service = new ImageMessages();
            var responseMessage = service.GetResponseMessage(requestMessage, this);
            return responseMessage;
        }

        /// <summary>
        /// 处理语音请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnVoiceRequest(RequestMessageVoice requestMessage)
        {
            var service = new VoiceMessages();
            var responseMessage = service.GetResponseMessage(requestMessage, this);
            return responseMessage;
        }

        /// <summary>
        /// 处理视频请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override IResponseMessageBase OnVideoRequest(RequestMessageVideo requestMessage)
        {
            var service = new VideoMessages();
            var responseMessage = service.GetResponseMessage(requestMessage, this);
            return responseMessage;
        }


        /// <summary>
        /// 处理链接消息请求
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        //        public override IResponseMessageBase OnLinkRequest(RequestMessageLink requestMessage)
        //        {
        //            var responseMessage = ResponseMessageBase.CreateFromRequestMessage<ResponseMessageText>(requestMessage);
        //            responseMessage.Content = string.Format(@"您发送了一条连接信息：
        //Title：{0}
        //Description:{1}
        //Url:{2}", requestMessage.Title, requestMessage.Description, requestMessage.Url);
        //            return responseMessage;
        //        }

        //        public override IResponseMessageBase OnFileRequest(RequestMessageFile requestMessage)
        //        {
        //            var responseMessage = requestMessage.CreateResponseMessage<ResponseMessageText>();
        //            responseMessage.Content = string.Format(@"您发送了一个文件：
        //文件名：{0}
        //说明:{1}
        //大小：{2}
        //MD5:{3}", requestMessage.Title, requestMessage.Description, requestMessage.FileTotalLen, requestMessage.FileMd5);
        //            return responseMessage;
        //        }

        ///// <summary>
        ///// 处理事件请求（这个方法一般不用重写，这里仅作为示例出现。除非需要在判断具体Event类型以外对Event信息进行统一操作
        ///// </summary>
        ///// <param name="requestMessage"></param>
        ///// <returns></returns>
        //public override IResponseMessageBase OnEventRequest(IRequestMessageEventBase requestMessage)
        //{
        //    var eventResponseMessage = base.OnEventRequest(requestMessage);//对于Event下属分类的重写方法，见：CustomerMessageHandler_Events.cs
        //    //TODO: 对Event信息进行统一操作
        //    return eventResponseMessage;
        //}

        public override IResponseMessageBase DefaultResponseMessage(IRequestMessageBase requestMessage)
        {
            /* 所有没有被处理的消息会默认返回这里的结果，
            * 因此，如果想把整个微信请求委托出去（例如需要使用分布式或从其他服务器获取请求），
            * 只需要在这里统一发出委托请求，如：
            * var responseMessage = MessageAgent.RequestResponseMessage(agentUrl, agentToken, RequestDocument.ToString());
            * return responseMessage;
            */

            //var responseMessage = CreateResponseMessage<ResponseMessageText>();
            //responseMessage.Content = "默认返回这条消息";
            //return responseMessage;
            return new ResponseMessageNoResponse();
        }

        public override IResponseMessageBase OnUnknownTypeRequest(RequestMessageUnknownType requestMessage)
        {
            /*
             * 此方法用于应急处理SDK没有提供的消息类型，
             * 原始XML可以通过requestMessage.RequestDocument（或this.RequestDocument）获取到。
             * 如果不重写此方法，遇到未知的请求类型将会抛出异常（v14.8.3 之前的版本就是这么做的）
             */
            //var msgType = MsgTypeHelper.GetRequestMsgTypeString(requestMessage.RequestDocument);
            //var responseMessage = CreateResponseMessage<ResponseMessageText>();
            //responseMessage.Content = "未知消息类型：" + msgType;
            ////WeixinTrace.SendCustomLog("未知请求消息类型", requestMessage.RequestDocument.ToString());//记录到日志中
            //return responseMessage;
            return new ResponseMessageNoResponse();
        }
    }
}
