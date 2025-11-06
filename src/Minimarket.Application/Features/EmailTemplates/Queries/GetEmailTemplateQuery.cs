using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.EmailTemplates.DTOs;

namespace Minimarket.Application.Features.EmailTemplates.Queries;

public class GetEmailTemplateQuery : IRequest<Result<EmailTemplateDto>>
{
    public string TemplateType { get; set; } = string.Empty;
}

