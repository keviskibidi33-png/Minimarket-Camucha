using MediatR;
using Microsoft.Extensions.Configuration;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.EmailTemplates.DTOs;
using Minimarket.Domain.Interfaces;

namespace Minimarket.Application.Features.EmailTemplates.Queries;

public class GetEmailTemplateQueryHandler : IRequestHandler<GetEmailTemplateQuery, Result<EmailTemplateDto>>
{
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;

    public GetEmailTemplateQueryHandler(IConfiguration configuration, IUnitOfWork unitOfWork)
    {
        _configuration = configuration;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<EmailTemplateDto>> Handle(GetEmailTemplateQuery request, CancellationToken cancellationToken)
    {
        var emailSettings = _configuration.GetSection("EmailSettings");
        var baseUrl = _configuration["BaseUrl"] ?? "http://localhost:5000";
        
        // Intentar obtener desde SystemSettings primero, luego desde appsettings
        var allSettings = await _unitOfWork.SystemSettings.GetAllAsync(cancellationToken);
        var logoSetting = allSettings.FirstOrDefault(s => s.Key == "email_template_logo_url");
        var promotionSetting = allSettings.FirstOrDefault(s => s.Key == "email_template_promotion_url");
        
        var logoUrl = logoSetting?.Value 
            ?? emailSettings["LogoUrl"] 
            ?? $"{baseUrl}/email-templates/logo.png";
            
        var promotionImageUrl = promotionSetting?.Value 
            ?? emailSettings["PromotionImageUrl"] 
            ?? $"{baseUrl}/email-templates/promotion.png";

        var dto = new EmailTemplateDto
        {
            TemplateType = request.TemplateType,
            LogoUrl = logoUrl,
            PromotionImageUrl = promotionImageUrl,
            Subject = "", // Se genera dinámicamente
            Body = "" // Se genera dinámicamente
        };

        return Result<EmailTemplateDto>.Success(dto);
    }
}

