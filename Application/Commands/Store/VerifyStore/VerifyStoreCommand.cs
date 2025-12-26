using Application.DTOs;
using MediatR;

namespace Application.Commands.Store.VerifyStore;

public sealed record VerifyStoreCommand(Guid StoreId) : IRequest<ServiceResponse>;
