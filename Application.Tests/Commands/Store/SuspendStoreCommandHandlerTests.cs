using Application.Commands.Store.SuspendStore;
using Application.Interfaces;
using Domain.Interfaces.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Application.Tests.Commands.Store;

public class SuspendStoreCommandHandlerTests
{
	private readonly Mock<IStoreRepository> _storeRepository = new();
	private readonly Mock<IUnitOfWork> _unitOfWork = new();
	private readonly Mock<ILogger<SuspendStoreCommandHandler>> _logger = new();

	private SuspendStoreCommandHandler CreateSut()
		=> new(_storeRepository.Object, _unitOfWork.Object, _logger.Object);

	[Fact]
	public async Task Handle_WhenStoreNotFound_ReturnsFailure()
	{
		// Arrange
		var storeId = Guid.NewGuid();
		_storeRepository
			.Setup(x => x.GetByIdAsync(storeId))
			.ReturnsAsync((Domain.Entities.Store?)null);

		var sut = CreateSut();
		var cmd = new SuspendStoreCommand(storeId);

		// Act
		var res = await sut.Handle(cmd, CancellationToken.None);

		// Assert
		res.IsSuccess.Should().BeFalse();
		res.Message.Should().Be("Store not found");
		_storeRepository.Verify(x => x.Update(It.IsAny<Domain.Entities.Store>()), Times.Never);
	}

	[Fact]
	public async Task Handle_WhenValidRequest_SuspendsStore()
	{
		// Arrange
		var store = Domain.Entities.Store.Create(Guid.NewGuid(), "My Store", null);
		_storeRepository
			.Setup(x => x.GetByIdAsync(store.Id))
			.ReturnsAsync(store);

		_unitOfWork
			.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()))
			.ReturnsAsync(1);

		var sut = CreateSut();
		var cmd = new SuspendStoreCommand(store.Id);

		// Act
		var res = await sut.Handle(cmd, CancellationToken.None);

		// Assert
		res.IsSuccess.Should().BeTrue();
		res.Message.Should().Be("Store suspended successfully");
		store.IsSuspended.Should().BeTrue();
		_storeRepository.Verify(x => x.Update(store), Times.Once);
		_unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
	}
}
