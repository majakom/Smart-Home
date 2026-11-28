
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute;

using SmartHome.Api.ErrorHandling;

namespace SmartHome.Api.Tests.ErrorHandling;

public sealed class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_ReturnsInternalServerError()
    {
        // Arrange
        var handler = CreateHandler(out _);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/test/path";

        // Act
        var result = await handler.TryHandleAsync(
            httpContext,
            new InvalidOperationException("Sensitive internal details"),
            CancellationToken.None
        );

        // Assert
        Assert.True(result);
        Assert.Equal(
            StatusCodes.Status500InternalServerError,
            httpContext.Response.StatusCode
        );
    }

    [Fact]
    public async Task TryHandleAsync_ReturnsGenericProblemDetails()
    {
        // Arrange
        var handler = CreateHandler(out var problemDetailsService);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = "/test/path";

        ProblemDetailsContext? capturedContext = null;

        problemDetailsService
            .TryWriteAsync(Arg.Do<ProblemDetailsContext>(
                context => capturedContext = context
            ))
            .Returns(ValueTask.FromResult(true));

        // Act
        await handler.TryHandleAsync(
            httpContext,
            new InvalidOperationException("Sensitive internal details"),
            CancellationToken.None
        );

        // Assert
        Assert.NotNull(capturedContext);

        var problemDetails = Assert.IsType<ProblemDetails>(
            capturedContext.ProblemDetails
        );

        Assert.Equal(StatusCodes.Status500InternalServerError, problemDetails.Status);
        Assert.Equal("An unexpected error occurred.", problemDetails.Title);
        Assert.Equal("/test/path", problemDetails.Instance);
        Assert.False(problemDetails.Extensions.ContainsKey("exception"));
        Assert.True(problemDetails.Extensions.ContainsKey("traceId"));

        await problemDetailsService.Received(1)
            .TryWriteAsync(Arg.Any<ProblemDetailsContext>());
    }

    [Fact]
    public async Task TryHandleAsync_ReturnsFalse_WhenProblemDetailsCannotBeWritten()
    {
        // Arrange
        var problemDetailsService = Substitute.For<IProblemDetailsService>();

        problemDetailsService
            .TryWriteAsync(Arg.Any<ProblemDetailsContext>())
            .Returns(ValueTask.FromResult(false));

        var handler = new GlobalExceptionHandler(
            NullLogger<GlobalExceptionHandler>.Instance,
            problemDetailsService
        );

        var httpContext = new DefaultHttpContext();

        // Act
        var result = await handler.TryHandleAsync(
            httpContext,
            new InvalidOperationException("Internal error"),
            CancellationToken.None
        );

        // Assert
        Assert.False(result);

        await problemDetailsService.Received(1)
            .TryWriteAsync(Arg.Any<ProblemDetailsContext>());
    }

    private static GlobalExceptionHandler CreateHandler(
        out IProblemDetailsService problemDetailsService
    )
    {
        problemDetailsService = Substitute.For<IProblemDetailsService>();

        problemDetailsService
            .TryWriteAsync(Arg.Any<ProblemDetailsContext>())
            .Returns(ValueTask.FromResult(true));

        return new GlobalExceptionHandler(
            NullLogger<GlobalExceptionHandler>.Instance,
            problemDetailsService
        );
    }
}
