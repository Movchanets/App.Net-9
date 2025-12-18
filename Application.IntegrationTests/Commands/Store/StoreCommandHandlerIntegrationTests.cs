using Application.Commands.Store.CreateStore;
using Application.Commands.Store.DeleteStore;
using Application.Commands.Store.SuspendStore;
using Application.Commands.Store.UpdateStore;
using Application.Commands.Store.VerifyStore;
using FluentAssertions;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Infrastructure.IntegrationTests.Commands.Store;

public class StoreCommandHandlerIntegrationTests : TestBase
{
	private static async Task<Domain.Entities.User> CreateDomainUserAsync(AppDbContext db)
	{
		var user = new Domain.Entities.User(Guid.NewGuid(), email: $"user_{Guid.NewGuid():N}@example.com");
		db.DomainUsers.Add(user);
		await db.SaveChangesAsync();
		return user;
	}

	[Fact]
	public async Task CreateStore_ShouldPersistStore_AndEnforceOneToOne()
	{
		// Arrange
		var user = await CreateDomainUserAsync(DbContext);
		var storeRepo = new StoreRepository(DbContext);
		var userRepo = new UserRepository(DbContext);
		var uow = new UnitOfWork(DbContext);
		var handler = new CreateStoreCommandHandler(storeRepo, userRepo, uow, NullLogger<CreateStoreCommandHandler>.Instance);

		var cmd = new CreateStoreCommand(user.Id, "My Store", "desc");

		// Act
		var res1 = await handler.Handle(cmd, CancellationToken.None);
		var res2 = await handler.Handle(cmd, CancellationToken.None);

		// Assert
		res1.IsSuccess.Should().BeTrue();
		res1.Payload.Should().NotBe(Guid.Empty);

		var store = await DbContext.Stores.FirstOrDefaultAsync(s => s.Id == res1.Payload);
		store.Should().NotBeNull();
		store!.UserId.Should().Be(user.Id);
		store.Name.Should().Be("My Store");
		store.Description.Should().Be("desc");

		res2.IsSuccess.Should().BeFalse();
		res2.Message.Should().Be("User already has a store");
	}

	[Fact]
	public async Task CreateStore_WhenUserMissing_ShouldFail()
	{
		// Arrange
		var storeRepo = new StoreRepository(DbContext);
		var userRepo = new UserRepository(DbContext);
		var uow = new UnitOfWork(DbContext);
		var handler = new CreateStoreCommandHandler(storeRepo, userRepo, uow, NullLogger<CreateStoreCommandHandler>.Instance);

		var cmd = new CreateStoreCommand(Guid.NewGuid(), "My Store", null);

		// Act
		var res = await handler.Handle(cmd, CancellationToken.None);

		// Assert
		res.IsSuccess.Should().BeFalse();
		res.Message.Should().Be("User not found");
		(await DbContext.Stores.CountAsync()).Should().Be(0);
	}

	[Fact]
	public async Task UpdateStore_ShouldUpdateNameAndSlug()
	{
		// Arrange
		var user = await CreateDomainUserAsync(DbContext);
		DbContext.Stores.Add(Domain.Entities.Store.Create(user.Id, "Old", "old desc"));
		await DbContext.SaveChangesAsync();

		var storeRepo = new StoreRepository(DbContext);
		var uow = new UnitOfWork(DbContext);
		var handler = new UpdateStoreCommandHandler(storeRepo, uow, NullLogger<UpdateStoreCommandHandler>.Instance);

		var cmd = new UpdateStoreCommand(user.Id, "New Name", "new desc");

		// Act
		var res = await handler.Handle(cmd, CancellationToken.None);

		// Assert
		res.IsSuccess.Should().BeTrue();
		var updated = await DbContext.Stores.SingleAsync(s => s.UserId == user.Id);
		updated.Name.Should().Be("New Name");
		updated.Description.Should().Be("new desc");
		updated.Slug.Should().NotBeNullOrWhiteSpace();
	}

	[Fact]
	public async Task DeleteStore_ShouldRemoveStore()
	{
		// Arrange
		var user = await CreateDomainUserAsync(DbContext);
		DbContext.Stores.Add(Domain.Entities.Store.Create(user.Id, "My Store", null));
		await DbContext.SaveChangesAsync();

		var storeRepo = new StoreRepository(DbContext);
		var uow = new UnitOfWork(DbContext);
		var handler = new DeleteStoreCommandHandler(storeRepo, uow, NullLogger<DeleteStoreCommandHandler>.Instance);

		var cmd = new DeleteStoreCommand(user.Id);

		// Act
		var res = await handler.Handle(cmd, CancellationToken.None);

		// Assert
		res.IsSuccess.Should().BeTrue();
		(await DbContext.Stores.CountAsync()).Should().Be(0);
	}

	[Fact]
	public async Task VerifyAndSuspendStore_ShouldSetFlags()
	{
		// Arrange
		var user = await CreateDomainUserAsync(DbContext);
		var store = Domain.Entities.Store.Create(user.Id, "My Store", null);
		DbContext.Stores.Add(store);
		await DbContext.SaveChangesAsync();

		var storeRepo = new StoreRepository(DbContext);
		var uow = new UnitOfWork(DbContext);

		var verifyHandler = new VerifyStoreCommandHandler(storeRepo, uow, NullLogger<VerifyStoreCommandHandler>.Instance);
		var suspendHandler = new SuspendStoreCommandHandler(storeRepo, uow, NullLogger<SuspendStoreCommandHandler>.Instance);

		// Act
		var verifyRes = await verifyHandler.Handle(new VerifyStoreCommand(store.Id), CancellationToken.None);
		var suspendRes = await suspendHandler.Handle(new SuspendStoreCommand(store.Id), CancellationToken.None);

		// Assert
		verifyRes.IsSuccess.Should().BeTrue();
		suspendRes.IsSuccess.Should().BeTrue();

		var updated = await DbContext.Stores.SingleAsync(s => s.Id == store.Id);
		updated.IsVerified.Should().BeTrue();
		updated.IsSuspended.Should().BeTrue();
	}
}
