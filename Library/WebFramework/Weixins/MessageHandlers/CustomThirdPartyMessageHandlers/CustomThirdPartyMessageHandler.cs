using Senparc.Weixin.Open;
using Senparc.Weixin.Open.Entities.Request;
using Senparc.Weixin.Open.MessageHandlers;
using System.IO;

namespace WebFramework.Weixins.MessageHandlers.CustomThirdPartyMessageHandlers
{
    public class CustomThirdPartyMessageHandler : ThirdPartyMessageHandler
    {
        public CustomThirdPartyMessageHandler(Stream inputStream, PostModel encryptPostModel)
            : base(inputStream, encryptPostModel) { }

        /// <summary>
        /// 请求授权
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override string OnComponentVerifyTicketRequest(RequestMessageComponentVerifyTicket requestMessage)
        {
            var openTicketPath = Server.GetMapPath("~/App_Data/OpenTicket");
            if (!Directory.Exists(openTicketPath))
            {
                Directory.CreateDirectory(openTicketPath);
            }

            //记录ComponentVerifyTicket（也可以存入数据库或其他可以持久化的地方）
            using (FileStream fs = new FileStream(Path.Combine(openTicketPath, string.Format("{0}.txt", RequestMessage.AppId)), FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (TextWriter tw = new StreamWriter(fs))
                {
                    tw.Write(requestMessage.ComponentVerifyTicket);
                    tw.Flush();
                }
            }
            return base.OnComponentVerifyTicketRequest(requestMessage);
        }

        /// <summary>
        /// 取消授权
        /// </summary>
        /// <param name="requestMessage"></param>
        /// <returns></returns>
        public override string OnUnauthorizedRequest(RequestMessageUnauthorized requestMessage)
        {
            return base.OnUnauthorizedRequest(requestMessage);
        }
    }
}
