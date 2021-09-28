using Senparc.NeuChar.Entities;
using Senparc.NeuChar.Entities.Request;
using Senparc.NeuChar.Helpers;
using Senparc.Weixin.MP.Entities;
using WebFramework.Services.Weixin.MessageHandlers.CustomMessageHandlers;

namespace WebFramework.Services.Weixin.MessageHandlers
{
    public class TextMessages
    {
        //下面的Url和Token可以用其他平台的消息，或者到www.weiweihi.com注册微信用户，将自动在“微信营销工具”下得到
        //string agentUrl = Config.SenparcWeixinSetting.AgentUrl;//这里使用了www.weiweihi.com微信自动托管平台
        //string agentToken = Config.SenparcWeixinSetting.AgentToken;//Token
        //string wiweihiKey = Config.SenparcWeixinSetting.SenparcWechatAgentKey;//WeiweihiKey专门用于对接www.Weiweihi.com平台，获取方式见：http://www.weiweihi.com/ApiDocuments/Item/25#51

        public IResponseMessageBase GetResponseMessage(RequestMessageText requestMessage, CustomMessageHandler handler)
        {
            //return ResponseMessageBase.CreateFromRequestMessage<ResponseMessageText>(requestMessage);

            //var defaultResponseMessage = handler.CreateResponseMessage<ResponseMessageText>();

            var h = requestMessage.StartHandler().Default(() =>
            {
                //var result = new StringBuilder();
                //result.AppendFormat("您刚才发送了文字信息：{0}\r\n\r\n", requestMessage.Content);

                //if (CurrentMessageContext.RequestMessages.Count > 1)
                //{
                //    result.AppendFormat("您刚才还发送了如下消息（{0}/{1}）：\r\n", CurrentMessageContext.RequestMessages.Count,
                //        CurrentMessageContext.StorageData);
                //    for (int i = CurrentMessageContext.RequestMessages.Count - 2; i >= 0; i--)
                //    {
                //        var historyMessage = CurrentMessageContext.RequestMessages[i];
                //        result.AppendFormat("{0} 【{1}】{2}\r\n",
                //            historyMessage.CreateTime.ToString("HH:mm:ss"),
                //            historyMessage.MsgType.ToString(),
                //            (historyMessage is RequestMessageText)
                //                ? (historyMessage as RequestMessageText).Content
                //                : "[非文字类型]"
                //            );
                //    }
                //    result.AppendLine("\r\n");
                //}

                //result.AppendFormat("如果您在{0}分钟内连续发送消息，记录将被自动保留（当前设置：最多记录{1}条）。过期后记录将会自动清除。\r\n", GlobalMessageContext.ExpireMinutes, GlobalMessageContext.MaxRecordCount);
                //result.AppendLine("\r\n");
                //result.AppendLine("您还可以发送【位置】【图片】【语音】【视频】等类型的信息（注意是这几种类型，不是这几个文字），查看不同格式的回复。\r\nSDK官方地址：http://sdk.weixin.senparc.com");

                //defaultResponseMessage.Content = result.ToString();
                //return defaultResponseMessage;

                handler.TextResponseMessage = "success";
                return null;
            });

            foreach (string keyword in WeixinData.MP.TextRequestReply.Keys)
            {
                if (string.IsNullOrEmpty(keyword)) continue;
                DTO.RequestReplyDTO model = WeixinData.MP.TextRequestReply[keyword];
                h.Keyword(keyword, () =>
                {
                    if (string.IsNullOrEmpty(model.media_id)) return null;
                    if (model.type == "news")
                    {
                        var dto = MP.Material.Get<DTO.NewsDTO>(model.media_id);
                        if (dto == null) return null;
                        var msg = requestMessage.CreateResponseMessage<ResponseMessageNews>();
                        msg.Articles.Add(new Article()
                        {
                            PicUrl = dto.thumb_url,
                            Title = dto.title,
                            Description = dto.digest,
                            Url = dto.url
                        });
                        return msg;
                    }
                    else if (model.type == "image")
                    {
                        var dto = MP.Material.Get<DTO.ImageDTO>(model.media_id);
                        if (dto == null) return null;
                        var msg = requestMessage.CreateResponseMessage<ResponseMessageImage>();
                        msg.Image = new Image() { MediaId = dto.media_id };
                        return msg;
                    }
                    return null;
                });
            }

            return h
            //关键字不区分大小写，按照顺序匹配成功后将不再运行下面的逻辑
            //.Keyword("约束", () =>
            //{
            //    defaultResponseMessage.Content =
            //    @"您正在进行微信内置浏览器约束判断测试。您可以：
            //    <a href=""http://sdk.weixin.senparc.com/FilterTest/"">点击这里</a>进行客户端约束测试（地址：http://sdk.weixin.senparc.com/FilterTest/），如果在微信外打开将直接返回文字。
            //    或：
            //    <a href=""http://sdk.weixin.senparc.com/FilterTest/Redirect"">点击这里</a>进行客户端约束测试（地址：http://sdk.weixin.senparc.com/FilterTest/Redirect），如果在微信外打开将重定向一次URL。";
            //    return defaultResponseMessage;
            //})
            ////匹配任一关键字
            //.Keywords(new[] { "托管", "代理" }, () =>
            //{
            //    //开始用代理托管，把请求转到其他服务器上去，然后拿回结果
            //    //甚至也可以将所有请求在DefaultResponseMessage()中托管到外部。

            //    DateTime dt1 = DateTime.Now; //计时开始

            //    var agentXml = handler.RequestDocument.ToString();

            //    #region 暂时转发到SDK线上Demo

            //    agentUrl = "http://sdk.weixin.senparc.com/weixin";
            //    //agentToken = WebConfigurationManager.AppSettings["WeixinToken"];//Token

            //    //修改内容，防止死循环
            //    var agentDoc = XDocument.Parse(agentXml);
            //    agentDoc.Root.Element("Content").SetValue("代理转发文字：" + requestMessage.Content);
            //    agentDoc.Root.Element("CreateTime").SetValue(DateTimeHelper.GetWeixinDateTime(DateTime.Now));//修改时间，防止去重
            //    agentDoc.Root.Element("MsgId").SetValue("123");//防止去重
            //    agentXml = agentDoc.ToString();

            //    #endregion

            //    var responseXml = MessageAgent.RequestXml(handler, agentUrl, agentToken, agentXml);
            //    //获取返回的XML
            //    //上面的方法也可以使用扩展方法：this.RequestResponseMessage(this,agentUrl, agentToken, RequestDocument.ToString());

            //    /* 如果有WeiweihiKey，可以直接使用下面的这个MessageAgent.RequestWeiweihiXml()方法。
            //    * WeiweihiKey专门用于对接www.weiweihi.com平台，获取方式见：https://www.weiweihi.com/ApiDocuments/Item/25#51
            //    */
            //    //var responseXml = MessageAgent.RequestWeiweihiXml(weiweihiKey, RequestDocument.ToString());//获取Weiweihi返回的XML

            //    DateTime dt2 = DateTime.Now; //计时结束

            //    //转成实体。
            //    /* 如果要写成一行，可以直接用：
            //    * responseMessage = MessageAgent.RequestResponseMessage(agentUrl, agentToken, RequestDocument.ToString());
            //    * 或
            //    *
            //    */
            //    var msg = string.Format("\r\n\r\n代理过程总耗时：{0}毫秒", (dt2 - dt1).Milliseconds);
            //    var agentResponseMessage = responseXml.CreateResponseMessage(handler.MessageEntityEnlightener);
            //    if (agentResponseMessage is ResponseMessageText)
            //    {
            //        (agentResponseMessage as ResponseMessageText).Content += msg;
            //    }
            //    else if (agentResponseMessage is ResponseMessageNews)
            //    {
            //        (agentResponseMessage as ResponseMessageNews).Articles[0].Description += msg;
            //    }
            //    return agentResponseMessage;//可能出现多种类型，直接在这里返回
            //})
            //.Keywords(new[] { "测试", "退出" }, () =>
            //{
            //    /*
            //     * 这是一个特殊的过程，此请求通常来自于微微嗨（http://www.weiweihi.com）的“盛派网络小助手”应用请求（https://www.weiweihi.com/User/App/Detail/1），
            //     * 用于演示微微嗨应用商店的处理过程，由于微微嗨的应用内部可以单独设置对话过期时间，所以这里通常不需要考虑对话状态，只要做最简单的响应。
            //     */
            //    if (defaultResponseMessage.Content == "测试")
            //    {
            //        //进入APP测试
            //        defaultResponseMessage.Content = "您已经进入（公众平台）的测试程序，请发送任意信息进行测试。发送文字【退出】退出测试对话。10分钟内无任何交互将自动退出应用对话状态。";
            //    }
            //    else
            //    {
            //        //退出APP测试
            //        defaultResponseMessage.Content = "您已经退出（公众平台）的测试程序。";
            //    }
            //    return defaultResponseMessage;
            //})
            //.Keyword("OPEN", () =>
            //{
            //    var openResponseMessage = requestMessage.CreateResponseMessage<ResponseMessageNews>();
            //    openResponseMessage.Articles.Add(new Article()
            //    {
            //        Title = "开放平台微信授权测试",
            //        Description = @"点击进入Open授权页面。
            //        授权之后，您的微信所收到的消息将转发到第三方（公众平台）的服务器上，并获得对应的回复。
            //        测试完成后，您可以登陆公众号后台取消授权。",
            //        Url = Configs.GetValue("CallbackURL") +"/OpenOAuth/JumpToMpOAuth"
            //    });
            //    return openResponseMessage;
            //})
            //.Keyword("错误", () =>
            //{
            //    var errorResponseMessage = requestMessage.CreateResponseMessage<ResponseMessageText>();
            //    //因为没有设置errorResponseMessage.Content，所以这小消息将无法正确返回。
            //    return errorResponseMessage;
            //})
            //.Keyword("容错", () =>
            //{
            //    Thread.Sleep(1500);//故意延时1.5秒，让微信多次发送消息过来，观察返回结果
            //    var faultTolerantResponseMessage = requestMessage.CreateResponseMessage<ResponseMessageText>();
            //    faultTolerantResponseMessage.Content = string.Format("测试容错，MsgId：{0}，Ticks：{1}", requestMessage.MsgId, DateTime.Now.Ticks);
            //    return faultTolerantResponseMessage;
            //})
            //.Keyword("TM", () =>
            //{
            //    var openId = requestMessage.FromUserName;
            //    var checkCode = Guid.NewGuid().ToString("n").Substring(0, 3);//为了防止openId泄露造成骚扰，这里启用验证码
            //    CustomMessageHandler.TemplateMessageCollection[checkCode] = openId;
            //    defaultResponseMessage.Content = string.Format(@"新的验证码为：{0}，请在网页上输入。网址：https://sdk.weixin.senparc.com/AsyncMethods", checkCode);
            //    return defaultResponseMessage;
            //})
            //.Keyword("OPENID", () =>
            //{
            //    var openId = requestMessage.FromUserName;//获取OpenId
            //    var userInfo = UserApi.Info(handler.appId, openId, Language.zh_CN);

            //    defaultResponseMessage.Content = string.Format(
            //        "您的OpenID为：{0}\r\n昵称：{1}\r\n性别：{2}\r\n地区（国家/省/市）：{3}/{4}/{5}\r\n关注时间：{6}\r\n关注状态：{7}",
            //        requestMessage.FromUserName, userInfo.nickname, (WeixinSex)userInfo.sex, userInfo.country, userInfo.province, userInfo.city, DateTimeHelper.GetDateTimeFromXml(userInfo.subscribe_time), userInfo.subscribe);
            //    return defaultResponseMessage;
            //})
            //.Keyword("EX", () =>
            //{
            //    var ex = new WeixinException("openid:" + requestMessage.FromUserName + ":这是一条测试异常信息");//回调过程在global的ConfigWeixinTraceLog()方法中
            //    defaultResponseMessage.Content = "请等待异步模板消息发送到此界面上（自动延时数秒）。\r\n当前时间：" + System.DateTime.Now.ToString();
            //    return defaultResponseMessage;
            //})
            //.Keyword("MUTE", () => //不回复任何消息
            //{
            //    handler.TextResponseMessage = "success";
            //    return null;
            //    //return null;//在 Action 中结合使用 return new FixWeixinBugWeixinResult(messageHandler);
            //})
            //.Keywords(new string[] { "JS", "JSSDK" }, () =>
            //{
            //    defaultResponseMessage.Content = "点击打开：http://sdk.weixin.senparc.com/WeixinJsSdk";
            //    return defaultResponseMessage;
            //})
            //“一次订阅消息”接口测试
            //.Keyword("订阅", () =>
            //{
            //    defaultResponseMessage.Content = "点击打开：https://sdk.weixin.senparc.com/SubscribeMsg";
            //    return defaultResponseMessage;
            //})
            //正则表达式
            //.Regex(@"^\d+#\d+$", () =>
            //{
            //    defaultResponseMessage.Content = string.Format("您输入了：{0}，符合正则表达式：^\\d+#\\d+$", requestMessage.Content);
            //    return defaultResponseMessage;
            //})
            .GetResponseMessage();
        }
    }
}
