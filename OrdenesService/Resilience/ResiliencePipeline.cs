using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace OrdenesService.Resilience;

public static class ResilienceExtensions
{
    public static IHttpClientBuilder AddShopFlowResilience(
        this IHttpClientBuilder builder)
    {
        // AddResilienceHandler configura la pipeline de Polly v8.
        // Las estrategias se aplican en orden: Retry -> CircuitBreaker -> Timeout -> HTTP.
        builder.AddResilienceHandler("shopflow-pipeline", pipeline =>
        {
            // Reintenta hasta 3 veces ante errores 5xx, 408, 429 y timeouts.
            // Backoff exponencial con jitter evita que multiples instancias
            // reintenten exactamente al mismo tiempo.
            pipeline.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromSeconds(1),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true
            });

            // Si >= 50% de las peticiones fallan en una ventana de 30s
            // (con al menos 5 muestras), el circuito se abre por 15s:
            // las peticiones fallan de inmediato sin llamar a la red.
            pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
            {
                FailureRatio = 0.5,
                SamplingDuration = TimeSpan.FromSeconds(30),
                MinimumThroughput = 5,
                BreakDuration = TimeSpan.FromSeconds(15)
            });
            // Cada intento individual tiene un limite de 5 segundos.
            pipeline.AddTimeout(TimeSpan.FromSeconds(5));
        });

        // IMPORTANTE: AddResilienceHandler devuelve IHttpResiliencePipelineBuilder,
        // no IHttpClientBuilder. Por eso devolvemos "builder" explicitamente.
        return builder;
    }
}