using FluentValidation;

namespace WebControllers.Models.DTO
{
    /// <summary>
    /// 模型验证
    /// </summary>
    public partial class EncodeTextInputDto_Validator : AbstractValidator<EncodeTextInputDto>
    {
        /// <summary></summary>
        public EncodeTextInputDto_Validator()
        {
            RuleFor(t => t.Text)
                .NotEmpty()
                .WithMessage("文本为必填项!");
            RuleFor(t => t.Text)
                .MinimumLength(1)
                .WithMessage("文本需至少输入1个字符!")
                .MaximumLength(100)
                .WithMessage("文本最多输入100个字符!");
        }
    }
}
