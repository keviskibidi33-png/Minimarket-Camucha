using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Minimarket.Application.Common.Models;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.EmailTemplates.Commands;

public class UpdateEmailTemplateSettingsCommandHandler : IRequestHandler<UpdateEmailTemplateSettingsCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UpdateEmailTemplateSettingsCommandHandler> _logger;

    public UpdateEmailTemplateSettingsCommandHandler(
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<UpdateEmailTemplateSettingsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Result<bool>> Handle(UpdateEmailTemplateSettingsCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Guardar en SystemSettings para persistencia
            var logoSetting = await _unitOfWork.SystemSettings.FirstOrDefaultAsync(
                s => s.Key == "email_template_logo_url",
                cancellationToken);

            var promotionSetting = await _unitOfWork.SystemSettings.FirstOrDefaultAsync(
                s => s.Key == "email_template_promotion_url",
                cancellationToken);

            if (logoSetting == null)
            {
                logoSetting = new Minimarket.Domain.Entities.SystemSettings
                {
                    Key = "email_template_logo_url",
                    Value = request.LogoUrl,
                    Category = "EmailTemplates"
                };
                await _unitOfWork.SystemSettings.AddAsync(logoSetting, cancellationToken);
            }
            else
            {
                logoSetting.Value = request.LogoUrl;
                // Entity Framework trackea los cambios automáticamente
            }

            if (promotionSetting == null)
            {
                promotionSetting = new Minimarket.Domain.Entities.SystemSettings
                {
                    Key = "email_template_promotion_url",
                    Value = request.PromotionImageUrl,
                    Category = "EmailTemplates"
                };
                await _unitOfWork.SystemSettings.AddAsync(promotionSetting, cancellationToken);
            }
            else
            {
                promotionSetting.Value = request.PromotionImageUrl;
                // Entity Framework trackea los cambios automáticamente
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Email template settings updated. LogoUrl: {LogoUrl}, PromotionUrl: {PromotionUrl}",
                request.LogoUrl, request.PromotionImageUrl);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating email template settings");
            throw;
        }
    }
}

