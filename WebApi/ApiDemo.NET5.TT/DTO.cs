using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApiDemo.NET5.Models.DTO
{
	/// <summary>
	/// ����/�ʾ�����
	/// </summary>
	public class PersonalAnswerModel
    {

		/// <summary>
		/// ���
		/// </summary>
		[Display(Name = "���")]
		public int Id { get; set; }
    
		/// <summary>
		/// ���
		/// </summary>
		[Display(Name = "���")]
		public int Year { get; set; }
    
		/// <summary>
		/// ��1�⣦�Ա�
		/// </summary>
		[Display(Name = "��1�⣦�Ա�")]
		[Required(ErrorMessage = "{0} Ϊ������")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} ����")]
		public string Sex { get; set; }
    
		/// <summary>
		/// ��2�⣦����
		/// </summary>
		[Display(Name = "��2�⣦����")]
		[Required(ErrorMessage = "{0} Ϊ������")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} ����")]
		public string Age { get; set; }
    
		/// <summary>
		/// ��3�⣦����
		/// </summary>
		[Display(Name = "��3�⣦����")]
		[Required(ErrorMessage = "{0} Ϊ������")]
        [StringLength(200, MinimumLength = 0, ErrorMessage = "{0} ����")]
		public string City { get; set; }
    
		/// <summary>
		/// ��4�⣦��Դ��
		/// </summary>
		[Display(Name = "��4�⣦��Դ��")]
		[Required(ErrorMessage = "{0} Ϊ������")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} ����")]
		public string Province { get; set; }
    
		/// <summary>
		/// ��5�⣦ѧ��
		/// </summary>
		[Display(Name = "��5�⣦ѧ��")]
		[Required(ErrorMessage = "{0} Ϊ������")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} ����")]
		public string Education { get; set; }
    
		/// <summary>
		/// ��6�⣦��ҵԺУ
		/// </summary>
		[Display(Name = "��6�⣦��ҵԺУ")]
		[Required(ErrorMessage = "{0} Ϊ������")]
        [StringLength(200, MinimumLength = 0, ErrorMessage = "{0} ����")]
		public string University { get; set; }
    
		/// <summary>
		/// ��7�⣦ְ��
		/// </summary>
		[Display(Name = "��7�⣦ְ��")]
		[Required(ErrorMessage = "{0} Ϊ������")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} ����")]
		public string Title { get; set; }
    
		/// <summary>
		/// ��8�⣦������ò
		/// </summary>
		[Display(Name = "��8�⣦������ò")]
		[Required(ErrorMessage = "{0} Ϊ������")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} ����")]
		public string Political { get; set; }
    
		/// <summary>
		/// ��9�⣦��ҵ����
		/// </summary>
		[Display(Name = "��9�⣦��ҵ����")]
		[Required(ErrorMessage = "{0} Ϊ������")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} ����")]
		public string WorkAge { get; set; }
    
		/// <summary>
		/// ��10�⣦��λ����
		/// </summary>
		[Display(Name = "��10�⣦��λ����")]
		[Required(ErrorMessage = "{0} Ϊ������")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} ����")]
		public string WorkNature { get; set; }
    
		/// <summary>
		/// ��11�⣦�������
		/// </summary>
		[Display(Name = "��11�⣦�������")]
		[Required(ErrorMessage = "{0} Ϊ������")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} ����")]
		public string WorkCategory { get; set; }
    
		/// <summary>
		/// ��12�⣦ӵ���������
		/// </summary>
		[Display(Name = "��12�⣦ӵ���������")]
		[Required(ErrorMessage = "{0} Ϊ������")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} ����")]
		public string HasCertificate { get; set; }
    
		/// <summary>
		/// ��13�⣦��λÿ����ѵ��ѧϰ����
		/// </summary>
		[Display(Name = "��13�⣦��λÿ����ѵ��ѧϰ����")]
		[Required(ErrorMessage = "{0} Ϊ������")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} ����")]
		public string WorkTraining { get; set; }
    
		/// <summary>
		/// ��14�⣦����������ѵ��ѧϰ����
		/// </summary>
		[Display(Name = "��14�⣦����������ѵ��ѧϰ����")]
		[Required(ErrorMessage = "{0} Ϊ������")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} ����")]
		public string PersonTraining { get; set; }
    
		/// <summary>
		/// ��15�⣦ÿ�²�����ѵ��ѧϰʱ��
		/// </summary>
		[Display(Name = "��15�⣦ÿ�²�����ѵ��ѧϰʱ��")]
		[Required(ErrorMessage = "{0} Ϊ������")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} ����")]
		public string MonthTrainingTime { get; set; }
    
		/// <summary>
		/// ��16�⣦ÿ�����ּ����������
		/// </summary>
		[Display(Name = "��16�⣦ÿ�����ּ����������")]
		[Required(ErrorMessage = "{0} Ϊ������")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} ����")]
		public string YearTrainingTime { get; set; }
    
		/// <summary>
		/// ��17�⣦ÿ�����ּ����������
		/// </summary>
		[Display(Name = "��17�⣦ÿ�����ּ����������")]
		[Required(ErrorMessage = "{0} Ϊ������")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} ����")]
		public string YearTrainingMoney { get; set; }
    
		/// <summary>
		/// ��18�⣦��н��ˮƽ
		/// </summary>
		[Display(Name = "��18�⣦��н��ˮƽ")]
		[Required(ErrorMessage = "{0} Ϊ������")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} ����")]
		public string YearSalary { get; set; }
    
		/// <summary>
		/// ��19�⣦н���Ƿ�����
		/// </summary>
		[Display(Name = "��19�⣦н���Ƿ�����")]
		[Required(ErrorMessage = "{0} Ϊ������")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} ����")]
		public string LikeSalary { get; set; }
    
		/// <summary>
		/// ��20�⣦�Ƿ��˽��Ż�����
		/// </summary>
		[Display(Name = "��20�⣦�Ƿ��˽��Ż�����")]
		[Required(ErrorMessage = "{0} Ϊ������")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} ����")]
		public string KnowPoliciesMeasures { get; set; }
    
		/// <summary>
		/// ��21�⣦��Ϊ����֧���������
		/// </summary>
		[Display(Name = "��21�⣦��Ϊ����֧���������")]
		[Required(ErrorMessage = "{0} Ϊ������")]
        [StringLength(1, MinimumLength = 0, ErrorMessage = "{0} ����")]
		public string LikePoliciesMeasures { get; set; }
    
		/// <summary>
		/// ��22�⣦��ע��Щ����֧��
		/// </summary>
		[Display(Name = "��22�⣦��ע��Щ����֧��")]
		[Required(ErrorMessage = "{0} Ϊ������")]
        [StringLength(4, MinimumLength = 0, ErrorMessage = "{0} ����")]
		public string FollowAreas { get; set; }
    
		/// <summary>
		/// ��22�⣦����
		/// </summary>
		[Display(Name = "��22�⣦����")]
        [StringLength(200, ErrorMessage = "{0} ����")]
		public string FollowAreaOther { get; set; }
    
		/// <summary>
		/// �޸Ĵ���
		/// </summary>
		[Display(Name = "�޸Ĵ���")]
		[Required(ErrorMessage = "{0} Ϊ������")]
		public int HistoryRecords { get; set; }
    }

	/// <summary>
	/// AutoMapper Profiles
	/// </summary>
	public class AutoMapperProfiles : AutoMapper.Profile
	{
		/// <summary>
		/// AutoMapper Profiles
		/// </summary>
		public AutoMapperProfiles()
		{
			CreateMap<PersonalAnswerModel, Entities.PersonalAnswer>().ReverseMap();
		}
	}
}
