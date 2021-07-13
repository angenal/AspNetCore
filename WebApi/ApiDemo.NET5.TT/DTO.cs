using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApiDemo.NET5.Models.DTO
{
	/// <summary>
	/// 个人/问卷调查表
	/// </summary>
	public class PersonalAnswerModel
    {

		/// <summary>
		/// 编号
		/// </summary>
		[Display(Name = "编号")]
		public int Id { get; set; }
    
		/// <summary>
		/// 年份
		/// </summary>
		[Display(Name = "年份")]
		public int Year { get; set; }
    
		/// <summary>
		/// 第1题＆性别
		/// </summary>
		[Display(Name = "第1题＆性别")]
		[Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} 错误")]
		public string Sex { get; set; }
    
		/// <summary>
		/// 第2题＆年龄
		/// </summary>
		[Display(Name = "第2题＆年龄")]
		[Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} 错误")]
		public string Age { get; set; }
    
		/// <summary>
		/// 第3题＆城市
		/// </summary>
		[Display(Name = "第3题＆城市")]
		[Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(200, MinimumLength = 0, ErrorMessage = "{0} 错误")]
		public string City { get; set; }
    
		/// <summary>
		/// 第4题＆生源地
		/// </summary>
		[Display(Name = "第4题＆生源地")]
		[Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} 错误")]
		public string Province { get; set; }
    
		/// <summary>
		/// 第5题＆学历
		/// </summary>
		[Display(Name = "第5题＆学历")]
		[Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} 错误")]
		public string Education { get; set; }
    
		/// <summary>
		/// 第6题＆毕业院校
		/// </summary>
		[Display(Name = "第6题＆毕业院校")]
		[Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(200, MinimumLength = 0, ErrorMessage = "{0} 错误")]
		public string University { get; set; }
    
		/// <summary>
		/// 第7题＆职称
		/// </summary>
		[Display(Name = "第7题＆职称")]
		[Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} 错误")]
		public string Title { get; set; }
    
		/// <summary>
		/// 第8题＆政治面貌
		/// </summary>
		[Display(Name = "第8题＆政治面貌")]
		[Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} 错误")]
		public string Political { get; set; }
    
		/// <summary>
		/// 第9题＆从业年限
		/// </summary>
		[Display(Name = "第9题＆从业年限")]
		[Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} 错误")]
		public string WorkAge { get; set; }
    
		/// <summary>
		/// 第10题＆单位性质
		/// </summary>
		[Display(Name = "第10题＆单位性质")]
		[Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} 错误")]
		public string WorkNature { get; set; }
    
		/// <summary>
		/// 第11题＆工作类别
		/// </summary>
		[Display(Name = "第11题＆工作类别")]
		[Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} 错误")]
		public string WorkCategory { get; set; }
    
		/// <summary>
		/// 第12题＆拥有相关资质
		/// </summary>
		[Display(Name = "第12题＆拥有相关资质")]
		[Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} 错误")]
		public string HasCertificate { get; set; }
    
		/// <summary>
		/// 第13题＆单位每年培训或学习次数
		/// </summary>
		[Display(Name = "第13题＆单位每年培训或学习次数")]
		[Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} 错误")]
		public string WorkTraining { get; set; }
    
		/// <summary>
		/// 第14题＆自主报名培训或学习次数
		/// </summary>
		[Display(Name = "第14题＆自主报名培训或学习次数")]
		[Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} 错误")]
		public string PersonTraining { get; set; }
    
		/// <summary>
		/// 第15题＆每月参与培训或学习时长
		/// </summary>
		[Display(Name = "第15题＆每月参与培训或学习时长")]
		[Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} 错误")]
		public string MonthTrainingTime { get; set; }
    
		/// <summary>
		/// 第16题＆每年主持及参与课题数
		/// </summary>
		[Display(Name = "第16题＆每年主持及参与课题数")]
		[Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} 错误")]
		public string YearTrainingTime { get; set; }
    
		/// <summary>
		/// 第17题＆每年主持及参与课题金额
		/// </summary>
		[Display(Name = "第17题＆每年主持及参与课题金额")]
		[Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} 错误")]
		public string YearTrainingMoney { get; set; }
    
		/// <summary>
		/// 第18题＆年薪资水平
		/// </summary>
		[Display(Name = "第18题＆年薪资水平")]
		[Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} 错误")]
		public string YearSalary { get; set; }
    
		/// <summary>
		/// 第19题＆薪酬是否满意
		/// </summary>
		[Display(Name = "第19题＆薪酬是否满意")]
		[Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} 错误")]
		public string LikeSalary { get; set; }
    
		/// <summary>
		/// 第20题＆是否了解优惠政策
		/// </summary>
		[Display(Name = "第20题＆是否了解优惠政策")]
		[Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} 错误")]
		public string KnowPoliciesMeasures { get; set; }
    
		/// <summary>
		/// 第21题＆认为政策支持力度如何
		/// </summary>
		[Display(Name = "第21题＆认为政策支持力度如何")]
		[Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} 错误")]
		public string LikePoliciesMeasures { get; set; }
    
		/// <summary>
		/// 第22题＆关注哪些政策支持
		/// </summary>
		[Display(Name = "第22题＆关注哪些政策支持")]
		[Required(ErrorMessage = "{0} 为必填项")]
        [StringLength(4, MinimumLength = 0, ErrorMessage = "{0} 错误")]
		public string FollowAreas { get; set; }
    
		/// <summary>
		/// 第22题＆其它
		/// </summary>
		[Display(Name = "第22题＆其它")]
        [StringLength(200, ErrorMessage = "{0} 错误")]
		public string FollowAreaOther { get; set; }
    
		/// <summary>
		/// 修改次数
		/// </summary>
		[Display(Name = "修改次数")]
		[Required(ErrorMessage = "{0} 为必填项")]
		public int HistoryRecords { get; set; }
    }
}
