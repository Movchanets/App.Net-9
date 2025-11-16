using Application.DTOs;
using Application.ViewModels;
using MediatR;
using Application.Interfaces;
using Domain.Interfaces.Repositories;
using AutoMapper;

namespace Application.Queries.User.GetProfile;

public class GetProfileHandler : IRequestHandler<GetProfileQuery, ServiceResponse<UserDto>>
{
	private readonly IUserService _identity;
	private readonly IUserRepository _userRepository;
	private readonly IMapper _mapper;

	public GetProfileHandler(IUserService identity, IUserRepository userRepository, IMapper mapper)
	{
		_identity = identity;
		_userRepository = userRepository;
		_mapper = mapper;
	}

	public async Task<ServiceResponse<UserDto>> Handle(GetProfileQuery request, CancellationToken cancellationToken)
	{
		var dto = await _identity.GetIdentityInfoByIdAsync(request.UserId);
		if (dto == null) return new ServiceResponse<UserDto>(false, "User not found");
		return new ServiceResponse<UserDto>(true, "Profile retrieved", dto);
	}
}
