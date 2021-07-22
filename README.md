# Polly With Retry And Circuit Breaker
Implement resistance and handling of transient faults such as time out, repetition and broken circuit. in your applications

add configuration extension method

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHttpClient<ExampleClient>(client =>
    {
        client.BaseAddress = new Uri("https://localhost:5001");
    })
    .AddPollyWithRetryAndCircuitBreaker(services); //CODE HERE
    services.AddControllers();
}
```

### `WaitAndRetry`
Standby time: 5s<br />
Retry Attempt: 0s, 2s, 4s, 8s, 16s<br />
OnRetry: has LogWarning

### `CircuitBreaker`
Allowed before: 3s<br />
Duration of break: 15s<br />
OnBreak: has LogWarning<br />
OnReset: has LogWarning

All problems presented during the application can be viewed in your log system
