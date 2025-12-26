using Application.DTOs;
using MediatR;

namespace Application.Commands.Store.SuspendStore;

public sealed record SuspendStoreCommand(Guid StoreId) : IRequest<ServiceResponse>;
