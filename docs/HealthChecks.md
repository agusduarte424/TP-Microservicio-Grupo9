# Health checks (F5)

## Qué resuelve
Cada servicio informa si está vivo y si puede atender. Así se detecta un servicio caído, o uno que funciona a medias porque una dependencia no responde.

## Endpoints (en los 5 servicios)

| URL | Qué revisa | Uso |
|---|---|---|
| `/health/live` | Nada: si responde, el proceso está vivo | Saber si hay que reiniciar el servicio |
| `/health/ready` | La base propia y cada servicio del que depende | Saber si puede recibir pedidos |
| `/health` | Lo mismo que ready | Estado general |

## Estados

| Estado | Cuándo | HTTP |
|---|---|---|
| Healthy | La base y todas las dependencias responden | 200 |
| Degraded | La base responde, pero una dependencia no | 200 |
| Unhealthy | La base no responde | 503 |

## Dónde está
- `Extensions/HealthChecksExtensions.cs`, el mismo archivo en los 5 servicios con su namespace.
  - `DatabaseHealthCheck`: abre la base SQLite y ejecuta `SELECT 1`.
  - `DependencyHealthCheck`: llama al `/health/live` de otro servicio, con timeout de 3 segundos.
  - Las dependencias salen de la sección `Services` de `appsettings.json`, así que no hay que tocar el archivo en cada servicio.
- `Program.cs`: `builder.Services.AddServiceHealthChecks(builder.Configuration)` y `app.MapServiceHealthChecks()`.

## Qué revisa cada servicio

| Servicio | Dependencias |
|---|---|
| Products | Orders |
| Users | ninguna |
| Orders | Users y Products |
| Cart | Products |
| Notifications | Users |

## Decisiones
- **La dependencia se consulta en `/health/live` y no en `/health`:** si Products le pidiera a Orders su `/health` completo, Orders se lo pediría a Products, y no terminaría nunca. `live` no revisa a nadie y corta la cadena.
- **Una dependencia caída es Degraded y no Unhealthy:** el servicio sigue respondiendo lo que no necesita a los demás. Por ejemplo, Orders lista órdenes aunque Users esté caído.

## Cómo probar
Con los 5 servicios corriendo, los 5 `/health` dan Healthy. Al apagar Users, `http://localhost:5003/health` da Degraded con `UsersApi` "no responde".