using Senparc.Weixin.Entities.TemplateMessage;
using Senparc.Weixin.MP.AdvancedAPIs.TemplateMessage;
using System;

namespace WebFramework.Services.Weixin.MessageTemplate
{
    /// <summary>
    /// 系统异常
    /// </summary>
    public class WeixinTemplate_ExceptionAlert : TemplateMessageBase
    {
        public TemplateDataItem first { get; set; }
        public TemplateDataItem keyword1 { get; set; }
        public TemplateDataItem keyword2 { get; set; }
        public TemplateDataItem keyword3 { get; set; }
        public TemplateDataItem keyword4 { get; set; }
        public TemplateDataItem keyword5 { get; set; }
        public TemplateDataItem remark { get; set; }

        public WeixinTemplate_ExceptionAlert(string _first, string host, string service, string status, string message, string _remark, string url = null,
            //根据实际的“模板ID”进行修改
            string templateId = "H2r4EHjGyUT5d846CEEscgaTaHD_MMObOZOPG3RhSJg")
            : base(templateId, url, "系统异常")
        {
            /*
            {{first.DATA}}
            Time：{{keyword1.DATA}}
            Host：{{keyword2.DATA}}
            Service：{{keyword3.DATA}}
            Status：{{keyword4.DATA}}
            Message：{{keyword5.DATA}}
            {{remark.DATA}}
            */

            first = new TemplateDataItem(_first);
            keyword1 = new TemplateDataItem(DateTime.Now.ToString());
            keyword2 = new TemplateDataItem(host);
            keyword3 = new TemplateDataItem(service);
            keyword4 = new TemplateDataItem(status);
            keyword5 = new TemplateDataItem(message);
            remark = new TemplateDataItem(_remark);
        }
    }
}
