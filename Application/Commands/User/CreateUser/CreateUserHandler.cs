using Application.DTOs;
using AutoMapper;
using Infrastructure.Entities;
using Infrastructure.Repositories.Interfaces;
using MediatR;

namespace Application.Commands.User.CreateUser;

public class CreateUserHandler : IRequestHandler<CreateUserCommand, UserDto>
{
    private readonly IMapper _mapper;
    private readonly IUserRepository _repository;

    public CreateUserHandler(IUserRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var user = new UserEntity
        {
            UserName = request.Username,
            Email = request.Email
        };

        var createdUser = await _repository.AddAsync(user);
        return _mapper.Map<UserDto>(createdUser);
    }
}