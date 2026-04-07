using HotChocolate.Execution;
using HotChocolate.Execution.Instrumentation;
using HotChocolate.Resolvers;

namespace Backend.GraphQL;

public sealed class GraphQLLoggingListener(ILogger<GraphQLLoggingListener> logger) : ExecutionDiagnosticEventListener
{
    public override void ResolverError(IMiddlewareContext context, IError error)
    {
        logger.LogError(
            error.Exception,
            "GraphQL resolver error. Path={Path} Message={Message}",
            error.Path?.ToString(),
            error.Exception?.Message ?? error.Message);
    }

    public override void RequestError(IRequestContext context, Exception exception)
    {
        logger.LogError(exception, "GraphQL request error.");
    }
}

