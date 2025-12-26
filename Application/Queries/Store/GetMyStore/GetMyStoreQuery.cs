using Application.DTOs;
using MediatR;

namespace Application.Queries.Store.GetMyStore;

public sealed record GetMyStoreQuery(Guid UserId) : IRequest<ServiceResponse<MyStoreDto?>>;

public sealed record MyStoreDto(
	Guid Id,
	string Name,
	string Slug,
	string? Description,
	bool IsVerified,
	bool IsSuspended,
	DateTime CreatedAt,
	int ProductCount
);
