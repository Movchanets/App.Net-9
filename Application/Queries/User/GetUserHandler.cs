using Application.DTOs;
using AutoMapper;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Application.Queries.User;

public class GetUserHandler : IRequestHandler<GetUserQuery, UserDto?>
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _repository;

    public GetUserHandler(IUserRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UserDto?> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetByIdAsync(request.Id);
        return user == null ? null : _mapper.Map<UserDto>(user);
    }
}