using Application.DTOs;
using MediatR;

namespace Application.Commands.Store.DeleteStore;

public sealed record DeleteStoreCommand(Guid UserId) : IRequest<ServiceResponse>;
