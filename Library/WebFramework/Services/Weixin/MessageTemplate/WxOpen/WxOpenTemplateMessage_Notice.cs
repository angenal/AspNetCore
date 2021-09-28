using Senparc.Weixin.Entities.TemplateMessage;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using System;
using WebCore;

namespace WebFramework.Services.Weixin.MessageTemplate.WxOpen
{
    /// <summary>
    /// 系统通知
    /// </summary>
    public class WxOpenTemplateMessage_Notice : TemplateMessageBase
    {
        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem keyword3 { get; set; }

        public WxOpenTemplateMessage_Notice(string title, DateTime time, string content, string url = null,
            //根据实际的“模板ID”进行修改
            string templateId = "TmZ5Vz1jO3u0YzYsfCS2zy6eD21HJUVd3Frgq8F0cbo")
            : base(templateId, url, "系统通知")
        {
            /*
            标题 {{keyword1.DATA}}
            时间 {{keyword2.DATA}}
            事件 {{keyword3.DATA}}
            */

            keyword1 = new TemplateDataItem(title);
            keyword2 = new TemplateDataItem(time.ToDateTimeString());
            keyword3 = new TemplateDataItem(content);
        }
    }
}
