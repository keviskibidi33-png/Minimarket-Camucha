using MediatR;
using Minimarket.Application.Common.Models;

namespace Minimarket.Application.Features.EmailTemplates.Commands;

public class UpdateEmailTemplateSettingsCommand : IRequest<Result<bool>>
{
    public string LogoUrl { get; set; } = string.Empty;
    public string PromotionImageUrl { get; set; } = string.Empty;
}

