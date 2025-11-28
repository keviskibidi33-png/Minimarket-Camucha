using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.DocumentSettings.DTOs;

namespace Minimarket.Application.Features.DocumentSettings.Commands;

public class UpdateDocumentViewSettingsCommand : IRequest<Result<DocumentViewSettingsDto>>
{
    public DocumentViewSettingsDto Settings { get; set; } = null!;
}

