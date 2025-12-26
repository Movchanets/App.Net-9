using Application.DTOs;
using MediatR;

namespace Application.Commands.Store.SuspendStore;

public sealed record UnsuspendStoreCommand(Guid StoreId) : IRequest<ServiceResponse>;
