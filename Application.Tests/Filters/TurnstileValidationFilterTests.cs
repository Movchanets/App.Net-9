using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Interfaces;
using API.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Application.Tests.Filters
{
	/// <summary>
	/// Unit tests for <see cref="TurnstileValidationFilter"/>.
	///
	/// These tests simulate ASP.NET Core's filter pipeline by constructing an
	/// <see cref="ActionExecutingContext"/> and passing a small <see cref="ActionExecutionDelegate"/>
	/// that returns an <see cref="ActionExecutedContext"/>. We mock the
	/// <see cref="ITurnstileService"/> to control validation outcomes.
	///
	/// Tests follow Arrange / Act / Assert pattern and include comments explaining
	/// the scenario being tested.
	/// </summary>
	public class TurnstileValidationFilterTests
	{
		/// <summary>
		/// Helper to create an ActionExecutingContext with the provided action arguments.
		/// The HttpContext contains a loopback RemoteIpAddress so the filter will pass
		/// that value to the validator mock.
		/// </summary>
		private static ActionExecutingContext CreateContext(Dictionary<string, object?> args, out DefaultHttpContext httpContext)
		{
			httpContext = new DefaultHttpContext();
			httpContext.Connection.RemoteIpAddress = System.Net.IPAddress.Parse("127.0.0.1");

			var actionContext = new ActionContext(httpContext, new RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
			var filters = new List<IFilterMetadata>();

			// controller can be any object; the filter does not depend on controller type
			return new ActionExecutingContext(actionContext, filters, args, controller: new object());
		}

		[Fact]
		public async Task NoToken_Present_Calls_Next()
		{
			// Arrange: no token in action arguments; the validator must not be called
			var mockTurnstile = new Mock<ITurnstileService>(MockBehavior.Strict);
			var mockLogger = new Mock<ILogger<TurnstileValidationFilter>>();
			var filter = new TurnstileValidationFilter(mockTurnstile.Object, mockLogger.Object);

			var context = CreateContext(new Dictionary<string, object?>(), out _);

			// Act: provide a next delegate that returns a real ActionExecutedContext
			var nextCalled = false;
			var actionCtx = new ActionContext(context.HttpContext, context.RouteData, context.ActionDescriptor);
			ActionExecutionDelegate next = () =>
			{
				nextCalled = true;
				var executed = new ActionExecutedContext(actionCtx, new List<IFilterMetadata>(), context.Controller);
				return Task.FromResult(executed);
			};

			await filter.OnActionExecutionAsync(context, next);

			// Assert: next() was invoked and context.Result left null
			Assert.True(nextCalled);
			Assert.Null(context.Result);
			mockTurnstile.VerifyNoOtherCalls();
		}

		[Fact]
		public async Task StringToken_Valid_Calls_Next()
		{
			// Arrange: provide a string token parameter, validator returns true
			var mockTurnstile = new Mock<ITurnstileService>();
			mockTurnstile.Setup(s => s.ValidateAsync("token-123", "127.0.0.1")).ReturnsAsync(true);
			var mockLogger = new Mock<ILogger<TurnstileValidationFilter>>();
			var filter = new TurnstileValidationFilter(mockTurnstile.Object, mockLogger.Object);

			var context = CreateContext(new Dictionary<string, object?> { ["turnstileToken"] = "token-123" }, out _);

			var nextCalled = false;
			var actionCtx = new ActionContext(context.HttpContext, context.RouteData, context.ActionDescriptor);
			ActionExecutionDelegate next = () =>
			{
				nextCalled = true;
				var executed = new ActionExecutedContext(actionCtx, new List<IFilterMetadata>(), context.Controller);
				return Task.FromResult(executed);
			};

			// Act
			await filter.OnActionExecutionAsync(context, next);

			// Assert: next was called and validator called once with expected args
			Assert.True(nextCalled);
			Assert.Null(context.Result);
			mockTurnstile.Verify(s => s.ValidateAsync("token-123", "127.0.0.1"), Times.Once);
		}

		[Fact]
		public async Task StringToken_Invalid_Returns_BadRequest()
		{
			// Arrange: token provided but validator returns false -> should short-circuit
			var mockTurnstile = new Mock<ITurnstileService>();
			mockTurnstile.Setup(s => s.ValidateAsync("bad", "127.0.0.1")).ReturnsAsync(false);
			var mockLogger = new Mock<ILogger<TurnstileValidationFilter>>();
			var filter = new TurnstileValidationFilter(mockTurnstile.Object, mockLogger.Object);

			var context = CreateContext(new Dictionary<string, object?> { ["token"] = "bad" }, out _);

			var nextCalled = false;
			var actionCtx = new ActionContext(context.HttpContext, context.RouteData, context.ActionDescriptor);
			ActionExecutionDelegate next = () =>
			{
				nextCalled = true;
				var executed = new ActionExecutedContext(actionCtx, new List<IFilterMetadata>(), context.Controller);
				return Task.FromResult(executed);
			};

			// Act
			await filter.OnActionExecutionAsync(context, next);

			// Assert: next was NOT called and a BadRequestObjectResult was set on the context
			Assert.False(nextCalled);
			Assert.IsType<BadRequestObjectResult>(context.Result);
			mockTurnstile.Verify(s => s.ValidateAsync("bad", "127.0.0.1"), Times.Once);
		}

		private class WithTokenObj
		{
			// This matches the filter's reflection check: property named TurnstileToken (case-insensitive)
			public string? TurnstileToken { get; set; }
		}

		[Fact]
		public async Task ObjectArg_With_TurnstileToken_Is_Used()
		{
			// Arrange: an object argument contains the TurnstileToken property
			var obj = new WithTokenObj { TurnstileToken = "obj-token" };
			var mockTurnstile = new Mock<ITurnstileService>();
			mockTurnstile.Setup(s => s.ValidateAsync("obj-token", "127.0.0.1")).ReturnsAsync(true);
			var mockLogger = new Mock<ILogger<TurnstileValidationFilter>>();
			var filter = new TurnstileValidationFilter(mockTurnstile.Object, mockLogger.Object);

			var context = CreateContext(new Dictionary<string, object?> { ["request"] = obj }, out _);

			var nextCalled = false;
			var actionCtx = new ActionContext(context.HttpContext, context.RouteData, context.ActionDescriptor);
			ActionExecutionDelegate next = () =>
			{
				nextCalled = true;
				var executed = new ActionExecutedContext(actionCtx, new List<IFilterMetadata>(), context.Controller);
				return Task.FromResult(executed);
			};

			// Act
			await filter.OnActionExecutionAsync(context, next);

			// Assert
			Assert.True(nextCalled);
			Assert.Null(context.Result);
			mockTurnstile.Verify(s => s.ValidateAsync("obj-token", "127.0.0.1"), Times.Once);
		}

		[Fact]
		public async Task Validator_Exception_Returns_500()
		{
			// Arrange: validator throws – filter should catch and set 500
			var mockTurnstile = new Mock<ITurnstileService>();
			mockTurnstile.Setup(s => s.ValidateAsync(It.IsAny<string>(), It.IsAny<string?>())).ThrowsAsync(new System.Exception("boom"));
			var mockLogger = new Mock<ILogger<TurnstileValidationFilter>>();
			var filter = new TurnstileValidationFilter(mockTurnstile.Object, mockLogger.Object);

			var context = CreateContext(new Dictionary<string, object?> { ["turnstileToken"] = "x" }, out _);

			var nextCalled = false;
			var actionCtx = new ActionContext(context.HttpContext, context.RouteData, context.ActionDescriptor);
			ActionExecutionDelegate next = () =>
			{
				nextCalled = true;
				var executed = new ActionExecutedContext(actionCtx, new List<IFilterMetadata>(), context.Controller);
				return Task.FromResult(executed);
			};

			// Act
			await filter.OnActionExecutionAsync(context, next);

			// Assert: next not called; a 500 StatusCodeResult is set
			Assert.False(nextCalled);
			Assert.IsType<StatusCodeResult>(context.Result);
			var code = ((StatusCodeResult)context.Result!).StatusCode;
			Assert.Equal(500, code);
		}
	}
}
