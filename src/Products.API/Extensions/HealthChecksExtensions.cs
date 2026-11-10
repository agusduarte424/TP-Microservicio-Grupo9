// Bloque de health checks (F5): /health, /health/ready y /health/live con respuesta JSON.
// Program.cs lo usa con dos líneas: builder.Services.AddServiceHealthChecks(builder.Configuration)
// y app.MapServiceHealthChecks(). El mismo archivo sirve en los 5 servicios: las dependencias
// salen de la sección "Services" de appsettings.json.
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Products.API.Extensions
{
    public static class HealthChecksExtensions
    {
        // los checks con este tag son los que revisa /health/ready
        private const string TagReady = "ready";

        /// <summary>
        /// Registra un check para la base propia y uno por cada servicio de la sección "Services".
        /// Si la base falla el estado es Unhealthy; si falla otro servicio, Degraded.
        /// </summary>
        public static IServiceCollection AddServiceHealthChecks(this IServiceCollection services, IConfiguration configuration)
        {
            // la base propia: sin base el servicio no puede atender, por eso falla como Unhealthy
            var checks = services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("database", HealthStatus.Unhealthy, [TagReady]);

            // cada servicio del que depende: si no responde, este servicio funciona a medias (Degraded)
            foreach (var dependencia in configuration.GetSection("Services").GetChildren())
            {
                if (string.IsNullOrWhiteSpace(dependencia.Value))
                    continue;

                checks.AddTypeActivatedCheck<DependencyHealthCheck>(
                    dependencia.Key, HealthStatus.Degraded, [TagReady], dependencia.Value);
            }

            // cliente HTTP propio de los checks, con timeout corto para que /health no se cuelgue
            services.AddHttpClient(DependencyHealthCheck.ClientName, client => client.Timeout = TimeSpan.FromSeconds(3));

            return services;
        }

        /// <summary>Publica /health (todo), /health/ready (base y dependencias) y /health/live (solo el proceso).</summary>
        public static WebApplication MapServiceHealthChecks(this WebApplication app)
        {
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = EscribirJson
            });

            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains(TagReady),
                ResponseWriter = EscribirJson
            });

            // Predicate = false: no corre ningún check; si responde, el proceso está vivo
            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false,
                ResponseWriter = EscribirJson
            });

            return app;
        }

        // respuesta JSON: estado general, servicio y el detalle de cada check
        private static Task EscribirJson(HttpContext context, HealthReport report)
        {
            return context.Response.WriteAsJsonAsync(new
            {
                status = report.Status.ToString(),
                servicio = context.RequestServices.GetRequiredService<IHostEnvironment>().ApplicationName,
                duracionMs = Math.Round(report.TotalDuration.TotalMilliseconds, 1),
                checks = report.Entries.Select(e => new
                {
                    nombre = e.Key,
                    status = e.Value.Status.ToString(),
                    descripcion = e.Value.Description,
                    duracionMs = Math.Round(e.Value.Duration.TotalMilliseconds, 1)
                })
            });
        }
    }

    /// <summary>Verifica que la base SQLite del servicio responda.</summary>
    public class DatabaseHealthCheck(IConfiguration configuration) : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var conn = new SqliteConnection(configuration.GetConnectionString("DefaultConnection"));
                await conn.OpenAsync(cancellationToken);

                await using var command = conn.CreateCommand();
                command.CommandText = "SELECT 1";
                await command.ExecuteScalarAsync(cancellationToken);

                return HealthCheckResult.Healthy("La base responde.");
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, "La base no responde.", ex);
            }
        }
    }

    /// <summary>Verifica que otro servicio responda en su /health/live.</summary>
    public class DependencyHealthCheck(IHttpClientFactory httpClientFactory, string baseUrl) : IHealthCheck
    {
        public const string ClientName = "health";

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                var http = httpClientFactory.CreateClient(ClientName);
                var response = await http.GetAsync(new Uri(new Uri(baseUrl), "health/live"), cancellationToken);

                return response.IsSuccessStatusCode
                    ? HealthCheckResult.Healthy($"{baseUrl} responde.")
                    : new HealthCheckResult(context.Registration.FailureStatus, $"{baseUrl} respondió {(int)response.StatusCode}.");
            }
            catch (Exception ex)
            {
                return new HealthCheckResult(context.Registration.FailureStatus, $"{baseUrl} no responde.", ex);
            }
        }
    }
}