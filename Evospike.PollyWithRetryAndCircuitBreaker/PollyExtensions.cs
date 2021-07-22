using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;

namespace Evospike.PollyWithRetryAndCircuitBreaker
{
    public static class PollyExtensions
    {
        public static void AddPollyWithRetryAndCircuitBreaker(this IHttpClientBuilder httpClientBuilder, IServiceCollection services)
        {
            Random jitterer = new();

            httpClientBuilder
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().WaitAndRetryAsync(
                5,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Next(0, 1000)),
                onRetry: (outcome, timeSpan, retryAttempt) =>
                {
                    var servicesProvider = services.BuildServiceProvider();
                    servicesProvider.GetRequiredService<ILoggerFactory>()?.CreateLogger("PollyLogger").LogWarning($"Deleying for {timeSpan.TotalSeconds} seconds, then making retry {retryAttempt}");
                })
            )
            .AddTransientHttpErrorPolicy(builder => builder.Or<TimeoutRejectedException>().CircuitBreakerAsync(
                3,
                TimeSpan.FromSeconds(15),
                onBreak: (outcome, timeSpan) =>
                {
                    var servicesProvider = services.BuildServiceProvider();
                    servicesProvider.GetRequiredService<ILoggerFactory>()?.CreateLogger("PollyLogger").LogWarning($"Opening the circuit for {timeSpan.TotalSeconds} seconds...");
                },
                onReset: () =>
                {
                    var servicesProvider = services.BuildServiceProvider();
                    servicesProvider.GetRequiredService<ILoggerFactory>()?.CreateLogger("PollyLogger").LogWarning($"Closing the circuit...");
                })
            )
            .AddPolicyHandler(Policy.TimeoutAsync<HttpResponseMessage>(1));
        }
    }
}