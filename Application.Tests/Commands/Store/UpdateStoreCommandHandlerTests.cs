using Application.Commands.Store.UpdateStore;
using Application.Interfaces;
using Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests.Commands.Store;

public class UpdateStoreCommandHandlerTests
{
	private readonly Mock<IStoreRepository> _storeRepository = new();
	private readonly Mock<IUnitOfWork> _unitOfWork = new();
	private readonly Mock<ILogger<UpdateStoreCommandHandler>> _logger = new();

	private UpdateStoreCommandHandler CreateSut()
		=> new(_storeRepository.Object, _unitOfWork.Object, _logger.Object);

	[Fact]
	public async Task Handle_WhenStoreNotFound_ReturnsFailure()
	{
		// Arrange
		var userId = Guid.NewGuid();
		_storeRepository
			.Setup(x => x.GetByUserIdAsync(userId))
			.ReturnsAsync((Domain.Entities.Store?)null);

		var sut = CreateSut();
		var cmd = new UpdateStoreCommand(userId, "New Name", "desc");

		// Act
		var res = await sut.Handle(cmd, CancellationToken.None);

		// Assert
		res.IsSuccess.Should().BeFalse();
		res.Message.Should().Be("Store not found");
		_storeRepository.Verify(x => x.Update(It.IsAny<Domain.Entities.Store>()), Times.Never);
		_unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_WhenSlugAlreadyExists_ReturnsFailure()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var store = Domain.Entities.Store.Create(userId, "Old", null);

		_storeRepository
			.Setup(x => x.GetByUserIdAsync(userId))
			.ReturnsAsync(store);

		_storeRepository
			.Setup(x => x.GetBySlugAsync(It.IsAny<string>()))
			.ReturnsAsync(Domain.Entities.Store.Create(Guid.NewGuid(), "New Name", null));

		var sut = CreateSut();
		var cmd = new UpdateStoreCommand(userId, "New Name", null);

		// Act
		var res = await sut.Handle(cmd, CancellationToken.None);

		// Assert
		res.IsSuccess.Should().BeFalse();
		res.Message.Should().Be("Store with same slug already exists");
		_storeRepository.Verify(x => x.Update(It.IsAny<Domain.Entities.Store>()), Times.Never);
		_unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
	}

	[Fact]
	public async Task Handle_WhenValidRequest_UpdatesStore()
	{
		// Arrange
		var userId = Guid.NewGuid();
		var store = Domain.Entities.Store.Create(userId, "Old", null);

		_storeRepository
			.Setup(x => x.GetByUserIdAsync(userId))
			.ReturnsAsync(store);

		_storeRepository
			.Setup(x => x.GetBySlugAsync(It.IsAny<string>()))
			.ReturnsAsync((Domain.Entities.Store?)null);

		_unitOfWork
			.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(1);

		var sut = CreateSut();
		var cmd = new UpdateStoreCommand(userId, "New Name", "desc");

		// Act
		var res = await sut.Handle(cmd, CancellationToken.None);

		// Assert
		res.IsSuccess.Should().BeTrue();
		res.Message.Should().Be("Store updated successfully");
		store.Name.Should().Be("New Name");
		store.Description.Should().Be("desc");

		_storeRepository.Verify(x => x.Update(store), Times.Once);
		_unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}
