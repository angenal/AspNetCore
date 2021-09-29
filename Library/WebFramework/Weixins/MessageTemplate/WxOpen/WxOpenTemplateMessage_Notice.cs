using Senparc.Weixin.Entities.TemplateMessage;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using System;
using WebCore;

namespace WebFramework.Weixins.MessageTemplate.WxOpen
{
    /// <summary>
    /// 系统通知
    /// </summary>
    public class WxOpenTemplateMessage_Notice : TemplateMessageData
    {
        public string TemplateId { get; set; }

        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem keyword3 { get; set; }

        public WxOpenTemplateMessage_Notice(string templateId,//根据实际的“模板ID”进行修改
            string title, string content, DateTime time)
        {
            /*
            标题 {{keyword1.DATA}}
            时间 {{keyword2.DATA}}
            事件 {{keyword3.DATA}}
            */

            TemplateId = templateId;

            keyword1 = new TemplateDataItem(title);
            keyword2 = new TemplateDataItem(time.ToDateTimeString());
            keyword3 = new TemplateDataItem(content);

            Add("标题", new TemplateMessageDataValue(keyword1));
            Add("时间", new TemplateMessageDataValue(keyword2));
            Add("事件", new TemplateMessageDataValue(keyword3));
        }
    }
}
