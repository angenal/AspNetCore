using Senparc.NeuChar.Entities;
using Senparc.Weixin;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.Entities;
using System.Threading.Tasks;
using WebFramework.Weixins.MessageHandlers.CustomMessageHandlers;

namespace WebFramework.Weixins.MessageHandlers
{
    public class VideoMessages
    {
        public IResponseMessageBase GetResponseMessage(RequestMessageVideo requestMessage, CustomMessageHandler handler)
        {
            var responseMessage = handler.CreateResponseMessage<ResponseMessageText>();
            responseMessage.Content = "您发送了一条视频信息，ID：" + requestMessage.MediaId;

            // 上传素材并推送到客户端
            Task.Factory.StartNew(async () =>
            {
                var dir = Server.GetMapPath("~/temp/");
                var file = await MediaApi.GetAsync(handler.appId, requestMessage.MediaId, dir);
                var uploadResult = await MediaApi.UploadTemporaryMediaAsync(handler.appId, UploadMediaFileType.video, file, 50000);
                await CustomApi.SendVideoAsync(handler.appId, handler.OpenId, uploadResult.media_id, "这是您刚才发送的视频", "这是一条视频消息");
            }).ContinueWith(async task =>
            {
                if (task.Exception != null)
                {
                    WeixinTrace.Log("OnVideoRequest()储存Video过程发生错误：", task.Exception.Message);

                    var msg = string.Format("上传素材出错：{0}\r\n{1}",
                               task.Exception.Message,
                               task.Exception.InnerException != null
                                   ? task.Exception.InnerException.Message
                                   : null);
                    await CustomApi.SendTextAsync(handler.appId, handler.OpenId, msg);
                }
            });

            return responseMessage;
        }
    }
}
