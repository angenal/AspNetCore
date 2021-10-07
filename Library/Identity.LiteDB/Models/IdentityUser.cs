using Microsoft.AspNetCore.Identity;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Identity.LiteDB.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    /// <summary>
    /// User Account to Represents a user in the identity system.
    /// The Id property is initialized to form a new GUID string value.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public partial class IdentityUser : IdentityUser<string>
    {
        /// <summary>
        /// Initializes a new instance of IdentityUser.
        /// </summary>
        public IdentityUser()
        {
            Initialize();
        }
        /// <summary>
        /// Initializes a new instance of IdentityUser.
        /// </summary>
        /// <param name="userName">The user login name.</param>
        public IdentityUser(string userName)
        {
            UserName = userName;
            Initialize();
        }
        /// <summary>
        /// Initializes a new instance of IdentityUser.
        /// </summary>
        /// <param name="init">Want to initialize?</param>
        public IdentityUser(bool init)
        {
            if (init) Initialize();
        }
        void Initialize()
        {
            Id = Guid.NewGuid().ToString();
            CreationTime = DateTime.Now;
            LastModificationTime = CreationTime;
        }

        /// <summary>
        /// the password salt for this user.
        /// </summary>
        public virtual string PasswordSalt { get; set; }

        /// <summary>
        /// 邮箱激活时间
        /// </summary>
        public virtual DateTime? EmailConfirmedTime { get; set; }
        /// <summary>
        /// 手机激活时间
        /// </summary>
        public virtual DateTime? PhoneNumberConfirmedTime { get; set; }

        /// <summary>
        /// 最近登录IP
        /// </summary>
        public virtual string LastLoginIP { get; set; }
        /// <summary>
        /// 最近登录时间
        /// </summary>
        public virtual DateTime? LastLoginTime { get; set; }
        /// <summary>
        /// 最新认证密钥
        /// </summary>
        public string AuthenticationKey { get; set; }
        /// <summary>
        /// 账号是否激活(用户启用状态)
        /// </summary>
        public virtual bool IsActive { get; set; }

        /// <summary>
        /// 身份证号码(身份唯一标识)
        /// </summary>
        public virtual string IdCard { get; set; }
        /// <summary>
        /// 微信openid(身份唯一标识)
        /// </summary>
        public virtual string WeChatOpenId { get; set; }
        /// <summary>
        /// 人脸识别id(身份唯一标识)
        /// </summary>
        public virtual string FaceImgCode { get; set; }

        /// <summary>
        /// 用户类型
        /// </summary>
        public virtual int Type { get; set; }
        /// <summary>
        /// 用户角色
        /// </summary>
        public virtual int Role { get; set; }
        /// <summary>
        /// 用户菜单
        /// </summary>
        public virtual string Menu { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        public virtual string Name { get; set; }
        /// <summary>
        /// 性别
        /// </summary>
        public virtual int Sex { get; set; }
        /// <summary>
        /// 头像
        /// </summary>
        public virtual string Avatar { get; set; }
        /// <summary>
        /// 昵称
        /// </summary>
        public virtual string Nickname { get; set; }
        /// <summary>
        /// 生日
        /// </summary>
        public virtual string Birthday { get; set; }
        /// <summary>
        /// 民族
        /// </summary>
        public virtual string Nation { get; set; }
        /// <summary>
        /// 籍贯
        /// </summary>
        public virtual string NativePlace { get; set; }
        /// <summary>
        /// 现居住地
        /// </summary>
        public virtual string LivePlace { get; set; }
        /// <summary>
        /// 户籍所在地
        /// </summary>
        public virtual string DomicileAddress { get; set; }
        /// <summary>
        /// 家庭住址
        /// </summary>
        public virtual string FamilyAddress { get; set; }
        /// <summary>
        /// 行政区划
        /// </summary>
        public virtual string AreaPath { get; set; }

        /// <summary>
        /// 学历
        /// </summary>
        public virtual string Education { get; set; }
        /// <summary>
        /// 学位
        /// </summary>
        public virtual string Degree { get; set; }
        /// <summary>
        /// 专业技术级别
        /// </summary>
        public virtual string TechnicalLevel { get; set; }
        /// <summary>
        /// 取得专业技术职务日期
        /// </summary>
        public virtual DateTime? TechnicalLevelDate { get; set; }

        /// <summary>
        /// 工作单位
        /// </summary>
        public virtual string WorkUnit { get; set; }
        /// <summary>
        /// 工作岗位
        /// </summary>
        public virtual string WorkPost { get; set; }
        /// <summary>
        /// 工作职务
        /// </summary>
        public virtual string JobTitle { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public virtual string Remark { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public virtual DateTime CreationTime { get; internal set; }
        /// <summary>
        /// 最近修改时间
        /// </summary>
        public virtual DateTime LastModificationTime { get; set; }
    }
}
