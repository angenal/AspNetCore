using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApiDemo.NET5.Models.Entities
{
	/// <summary>
	/// ����/�ʾ�����
	/// </summary>
	public partial class PersonalAnswer
    {

		/// <summary>
		/// ���
		/// </summary>
		[Display(Name = "���")]
        [Key()][SqlSugar.SugarColumn(IsPrimaryKey = true)]
		[Required]
        public int Id { get; set; }
    
		/// <summary>
		/// �ͻ���IP
		/// </summary>
		[Display(Name = "�ͻ���IP")]
		[Required]
		[MaxLength(40)]
        public string Ip { get; set; }
    
		/// <summary>
		/// ���
		/// </summary>
		[Display(Name = "���")]
		[Required]
        public int Year { get; set; }
    
		/// <summary>
		/// ��1�⣦�Ա�
		/// </summary>
		[Display(Name = "��1�⣦�Ա�")]
		[Required]
		[MaxLength(1)]
        public string Sex { get; set; }
    
		/// <summary>
		/// ��2�⣦����
		/// </summary>
		[Display(Name = "��2�⣦����")]
		[Required]
		[MaxLength(1)]
        public string Age { get; set; }
    
		/// <summary>
		/// ��3�⣦����
		/// </summary>
		[Display(Name = "��3�⣦����")]
		[Required]
		[MaxLength(200)]
        public string City { get; set; }
    
		/// <summary>
		/// ��4�⣦��Դ��
		/// </summary>
		[Display(Name = "��4�⣦��Դ��")]
		[Required]
		[MaxLength(1)]
        public string Province { get; set; }
    
		/// <summary>
		/// ��5�⣦ѧ��
		/// </summary>
		[Display(Name = "��5�⣦ѧ��")]
		[Required]
		[MaxLength(1)]
        public string Education { get; set; }
    
		/// <summary>
		/// ��6�⣦��ҵԺУ
		/// </summary>
		[Display(Name = "��6�⣦��ҵԺУ")]
		[Required]
		[MaxLength(200)]
        public string University { get; set; }
    
		/// <summary>
		/// ��7�⣦ְ��
		/// </summary>
		[Display(Name = "��7�⣦ְ��")]
		[Required]
		[MaxLength(1)]
        public string Title { get; set; }
    
		/// <summary>
		/// ��8�⣦������ò
		/// </summary>
		[Display(Name = "��8�⣦������ò")]
		[Required]
		[MaxLength(1)]
        public string Political { get; set; }
    
		/// <summary>
		/// ��9�⣦��ҵ����
		/// </summary>
		[Display(Name = "��9�⣦��ҵ����")]
		[Required]
		[MaxLength(1)]
        public string WorkAge { get; set; }
    
		/// <summary>
		/// ��10�⣦��λ����
		/// </summary>
		[Display(Name = "��10�⣦��λ����")]
		[Required]
		[MaxLength(1)]
        public string WorkNature { get; set; }
    
		/// <summary>
		/// ��11�⣦�������
		/// </summary>
		[Display(Name = "��11�⣦�������")]
		[Required]
		[MaxLength(1)]
        public string WorkCategory { get; set; }
    
		/// <summary>
		/// ��12�⣦ӵ���������
		/// </summary>
		[Display(Name = "��12�⣦ӵ���������")]
		[Required]
		[MaxLength(1)]
        public string HasCertificate { get; set; }
    
		/// <summary>
		/// ��13�⣦��λÿ����ѵ��ѧϰ����
		/// </summary>
		[Display(Name = "��13�⣦��λÿ����ѵ��ѧϰ����")]
		[Required]
		[MaxLength(1)]
        public string WorkTraining { get; set; }
    
		/// <summary>
		/// ��14�⣦����������ѵ��ѧϰ����
		/// </summary>
		[Display(Name = "��14�⣦����������ѵ��ѧϰ����")]
		[Required]
		[MaxLength(1)]
        public string PersonTraining { get; set; }
    
		/// <summary>
		/// ��15�⣦ÿ�²�����ѵ��ѧϰʱ��
		/// </summary>
		[Display(Name = "��15�⣦ÿ�²�����ѵ��ѧϰʱ��")]
		[Required]
		[MaxLength(1)]
        public string MonthTrainingTime { get; set; }
    
		/// <summary>
		/// ��16�⣦ÿ�����ּ����������
		/// </summary>
		[Display(Name = "��16�⣦ÿ�����ּ����������")]
		[Required]
		[MaxLength(1)]
        public string YearTrainingTime { get; set; }
    
		/// <summary>
		/// ��17�⣦ÿ�����ּ����������
		/// </summary>
		[Display(Name = "��17�⣦ÿ�����ּ����������")]
		[Required]
		[MaxLength(1)]
        public string YearTrainingMoney { get; set; }
    
		/// <summary>
		/// ��18�⣦��н��ˮƽ
		/// </summary>
		[Display(Name = "��18�⣦��н��ˮƽ")]
		[Required]
		[MaxLength(1)]
        public string YearSalary { get; set; }
    
		/// <summary>
		/// ��19�⣦н���Ƿ�����
		/// </summary>
		[Display(Name = "��19�⣦н���Ƿ�����")]
		[Required]
		[MaxLength(1)]
        public string LikeSalary { get; set; }
    
		/// <summary>
		/// ��20�⣦�Ƿ��˽��Ż�����
		/// </summary>
		[Display(Name = "��20�⣦�Ƿ��˽��Ż�����")]
		[Required]
		[MaxLength(1)]
        public string KnowPoliciesMeasures { get; set; }
    
		/// <summary>
		/// ��21�⣦��Ϊ����֧���������
		/// </summary>
		[Display(Name = "��21�⣦��Ϊ����֧���������")]
		[Required]
		[MaxLength(1)]
        public string LikePoliciesMeasures { get; set; }
    
		/// <summary>
		/// ��22�⣦��ע��Щ����֧��
		/// </summary>
		[Display(Name = "��22�⣦��ע��Щ����֧��")]
		[Required]
		[MaxLength(4)]
        public string FollowAreas { get; set; }
    
		/// <summary>
		/// ��22�⣦����
		/// </summary>
		[Display(Name = "��22�⣦����")]
		[MaxLength(200)]
        public string FollowAreaOther { get; set; }
    
		/// <summary>
		/// �޸Ĵ���
		/// </summary>
		[Display(Name = "�޸Ĵ���")]
		[Required]
        public int HistoryRecords { get; set; }
    }
}
