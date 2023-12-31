﻿using SampleMicroserviceApp.Identity.Application.Common.Contracts;

namespace SampleMicroserviceApp.Identity.Web.API.Middlewares;

public class CorrelationIdHandlingMiddleware(ICorrelationIdManager correlationIdManager) : IMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-ID";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        AddCorrelationIdToResponse(context, correlationId);

        await next(context);
    }

    private string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var cId))
        {
            var correlationId = cId.ToString();

            if (string.IsNullOrWhiteSpace(correlationId) is false)
            {
                correlationIdManager.Set(correlationId);

                return correlationId;
            }
        }

        return correlationIdManager.Get();
    }

    private void AddCorrelationIdToResponse(HttpContext context, string correlationId)
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.Add(CorrelationIdHeaderName, new[] { correlationId });
            return Task.CompletedTask;
        });
    }
}