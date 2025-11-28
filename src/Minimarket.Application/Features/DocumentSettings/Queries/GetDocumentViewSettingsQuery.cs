using MediatR;
using Minimarket.Application.Common.Models;
using Minimarket.Application.Features.DocumentSettings.DTOs;

namespace Minimarket.Application.Features.DocumentSettings.Queries;

public class GetDocumentViewSettingsQuery : IRequest<Result<DocumentViewSettingsDto>>
{
}

