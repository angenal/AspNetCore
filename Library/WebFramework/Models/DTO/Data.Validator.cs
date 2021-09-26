using FluentValidation;

namespace WebFramework.Models.DTO
{
    /// <summary>
    /// 模型验证
    /// </summary>
    public partial class EncodeTextInputDto_Validator : AbstractValidator<EncodeTextInputDto>
    {
        public EncodeTextInputDto_Validator()
        {
            RuleFor(t => t.Text)
                .NotEmpty()
                .WithMessage("文本为必填项!");
            RuleFor(t => t.Text)
                .MinimumLength(1)
                .MaximumLength(255)
                .WithMessage("文本编码最多255个字符!");
        }
    }
}
