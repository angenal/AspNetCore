using Senparc.NeuChar.Entities;
using Senparc.Weixin.MP.Entities;
using System.Linq;
using WebFramework.Weixins.MessageHandlers.CustomMessageHandlers;

namespace WebFramework.Weixins.MessageHandlers
{
    public class ImageMessages
    {
        public IResponseMessageBase GetResponseMessage(RequestMessageImage requestMessage, CustomMessageHandler handler)
        {
            //一隔一返回News或Image格式
            if (handler.GlobalMessageContext.GetMessageContext(requestMessage).RequestMessages.Count() % 2 == 0)
            {
                var responseMessage = handler.CreateResponseMessage<ResponseMessageNews>();

                responseMessage.Articles.Add(new Article()
                {
                    Title = "您刚才发送了图片信息",
                    Description = "您发送的图片将会显示在边上",
                    PicUrl = requestMessage.PicUrl,
                    Url = "http://sdk.weixin.senparc.com"
                });
                responseMessage.Articles.Add(new Article()
                {
                    Title = "第二条",
                    Description = "第二条带连接的内容",
                    PicUrl = requestMessage.PicUrl,
                    Url = "http://sdk.weixin.senparc.com"
                });

                return responseMessage;
            }
            else
            {
                var responseMessage = handler.CreateResponseMessage<ResponseMessageImage>();
                responseMessage.Image.MediaId = requestMessage.MediaId;
                return responseMessage;
            }
        }
    }
}
