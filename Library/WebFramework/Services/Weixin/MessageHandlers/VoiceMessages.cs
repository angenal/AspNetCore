using Senparc.NeuChar.Entities;
using Senparc.Weixin.MP;
using Senparc.Weixin.MP.AdvancedAPIs;
using Senparc.Weixin.MP.Entities;
using WebFramework.Services.Weixin.MessageHandlers.CustomMessageHandlers;

namespace WebFramework.Services.Weixin.MessageHandlers
{
    public class VoiceMessages
    {
        public IResponseMessageBase GetResponseMessage(RequestMessageVoice requestMessage, CustomMessageHandler handler)
        {
            var responseMessage = handler.CreateResponseMessage<ResponseMessageMusic>();
            //上传缩略图
            //var accessToken = Containers.AccessTokenContainer.TryGetAccessToken(appId, appSecret);
            var uploadResult = MediaApi.UploadTemporaryMedia(handler.appId, UploadMediaFileType.image,
                                                         Server.GetMapPath("~/UpLoadFiles/web/20180910/logo.png"));

            //设置音乐信息
            responseMessage.Music.Title = "天籁之音";
            responseMessage.Music.Description = "播放您上传的语音";
            responseMessage.Music.MusicUrl = "https://sdk.weixin.senparc.com/Media/GetVoice?mediaId=" + requestMessage.MediaId;
            responseMessage.Music.HQMusicUrl = "https://sdk.weixin.senparc.com/Media/GetVoice?mediaId=" + requestMessage.MediaId;
            responseMessage.Music.ThumbMediaId = uploadResult.media_id;

            //推送一条客服消息
            try
            {
                CustomApi.SendText(handler.appId, handler.OpenId, "本次上传的音频MediaId：" + requestMessage.MediaId);
            }
            catch
            {
            }

            return responseMessage;
        }
    }
}
