using Application.DTOs;
using MediatR;

namespace Application.Queries.Store.GetAllStores;

public sealed record GetAllStoresQuery(bool IncludeUnverified = true) : IRequest<ServiceResponse<IReadOnlyList<StoreAdminDto>>>;

public sealed record StoreAdminDto(
	Guid Id,
	string Name,
	string Slug,
	string? Description,
	Guid UserId,
	string? OwnerName,
	string? OwnerEmail,
	bool IsVerified,
	bool IsSuspended,
	DateTime CreatedAt,
	int ProductCount
);
