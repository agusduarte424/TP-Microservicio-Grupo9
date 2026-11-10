# Pruebas

Checklist para probar los 5 servicios, escenario de la demo en vivo y registro de las pruebas
cruzadas. Los requests están en el archivo `.http` de cada servicio (`src/<Servicio>.API/<Servicio>.API.http`).

## Antes de probar

1. Con los servicios apagados, borrar los archivos `.db` de cada carpeta de proyecto para empezar
   con las bases vacías.
2. Levantar los 5 servicios, en este orden: Products (5001), Users (5002), Cart (5004),
   Notifications (5005) y Orders (5003).
3. Para probar el borrado de productos contra el Orders real, poner `OrdersSimulado:Usar` en
   `false` en `src/Products.API/appsettings.Development.json`. Dejarlo en `true` si Orders
   no está corriendo.

## Qué se verifica en cada respuesta

- [ ] Cada endpoint tiene una request exitosa y una de error, con el status que fija la consigna.
- [ ] Cada `errorCode` del catálogo se provoca al menos una vez y el `errorMessage` coincide.
- [ ] Todo error trae los 8 campos: `type`, `title`, `status`, `detail`, `instance`, `errorCode`, `errorMessage` y `correlationId`.
- [ ] Ninguna respuesta incluye un stack trace ni `PasswordHash`.
- [ ] Toda respuesta trae el header `X-Correlation-Id`.
- [ ] En la consola, los errores de negocio salen como `Warning` (con el `errorCode`) y los inesperados como `Error`.

## Los 27 códigos de error

Cada fila indica cómo provocar el error. La columna **OK** se marca al probarlo.

### Products.API (`Products.API.http`)

| Código | HTTP | Cómo provocarlo | OK |
|---|---|---|---|
| PRD-001 | 404 | GET, PUT o DELETE con un id que no existe | ☐ |
| PRD-002 | 400 | POST con nombre vacío, precio 0 o stock negativo; o un JSON malformado | ☐ |
| PRD-003 | 409 | POST del mismo producto dos veces (mismo nombre y categoría) | ☐ |
| PRD-004 | 409 | DELETE de un producto con una orden Pendiente o Confirmada | ☐ |
| PRD-005 | 500 | DELETE con `OrdersSimulado:Usar` en `false` y Orders apagado | ☐ |

### Users.API (`Users.API.http`)

| Código | HTTP | Cómo provocarlo | OK |
|---|---|---|---|
| USR-001 | 409 | POST /register con un email ya registrado | ☑ |
| USR-002 | 400 | POST /register con nombre vacío, email sin formato y contraseña corta; o un JSON malformado | ☑ |
| USR-003 | 401 | POST /login con contraseña incorrecta, o con un email inexistente | ☑ |
| USR-004 | 403 | POST /login con contraseña incorrecta tres veces seguidas | ☑ |
| USR-005 | 403 | POST /login de un usuario con `activo = 0` e `intentosFallidos = 0` en `users.db` | ☑ |
| USR-006 | 500 | POST /register con la tabla `users` renombrada en `users.db` |☑ ||

### Orders.API (`Orders.API.http`)

| Código | HTTP | Cómo provocarlo | OK |
|---|---|---|---|
| ORD-001 | 404 | GET o PUT /status con un id de orden que no existe | ☑ |
| ORD-002 | 400 | POST con la lista de items vacía, cantidad 0 o usuarioId vacío | ☑ |
| ORD-003 | 404 | POST con un usuarioId que no existe en Users | ☑ |
| ORD-004 | 404 | POST con un productoId que no existe en Products | ☑ |
| ORD-005 | 422 | POST con una cantidad mayor al stock del producto | ☑ |
| ORD-006 | 409 | PUT /status con una transición no válida (por ejemplo de Confirmada a Pendiente) | ☑ |
| ORD-007 | 500 | POST válido con Users o Products apagado | ☑ |

### Cart.API (`Cart.API.http`)

| Código | HTTP | Cómo provocarlo | OK |
|---|---|---|---|
| CRT-001 | 404 | GET, PUT o DELETE de un usuario sin carrito | ☐ |
| CRT-002 | 404 | POST con un producto que no existe en Products; PUT o DELETE de un producto que no está en el carrito | ☐ |
| CRT-003 | 422 | POST o PUT con una cantidad mayor al stock | ☐ |
| CRT-004 | 400 | POST o PUT con cantidad 0 o negativa; o un JSON malformado | ☐ |
| CRT-005 | 500 | POST válido con Products apagado | ☐ |

### Notifications.API (`Notifications.API.http`)

| Código | HTTP | Cómo provocarlo | OK |
|---|---|---|---|
| NTF-001 | 404 | POST /send con un usuarioId que no existe en Users | ☑ |
| NTF-002 | 400 | POST /send con tipo `Telegrama`, campos faltantes o un JSON malformado | ☑ |
| NTF-003 | 404 | GET de un usuario que no tiene notificaciones | ☑ |
| NTF-004 | 500 | POST /send válido con Users apagado | ☑ |

## Escenario de la demo en vivo

Orden pensado para la defensa. Cada paso indica qué se muestra.

| # | Paso | Qué se muestra |
|---|---|---|
| 1 | Crear un producto con stock 10 y repetirlo | 201 y después PRD-003 (409) |
| 2 | Registrar un usuario y repetir el email | 201 y después USR-001 (409) |
| 3 | Login con contraseña incorrecta tres veces | USR-003, USR-003 y USR-004 (403) |
| 4 | Agregar al carrito más unidades que el stock | CRT-003 (422) |
| 5 | Crear una orden válida y otra con stock excesivo | 201 con estado Pendiente y después ORD-005 (422) |
| 6 | Pasar la orden a Confirmada y volver a Pendiente | 200 y después ORD-006 (409) |
| 7 | Intentar borrar el producto con la orden activa | PRD-004 (409) |
| 8 | Enviar y listar una notificación; probar un usuario inexistente | 201, 200 y NTF-001 (404) |
| 9 | Mostrar el mismo Correlation ID en los logs de Orders y Products | Se manda `X-Correlation-Id` en el POST de la orden y se busca en las dos consolas |
| 10 | Mostrar `/health` de cada servicio, con uno apagado | JSON con `Healthy`, y `Degraded` o `Unhealthy` cuando una dependencia no responde |

Para el paso 7 hace falta `OrdersSimulado:Usar` en `false`. Para el paso 3, un usuario nuevo
(no el del paso 2, para no bloquearlo).

## Health checks

- [x] `GET /health`, `/health/ready` y `/health/live` responden JSON en los 5 servicios.
- [x] El estado es `Healthy`, `Degraded` o `Unhealthy`.
- [x] Con una dependencia apagada, `/health/ready` deja de decir `Healthy`.
- [x] `/health/live` solo verifica que el proceso responde.

## Pruebas cruzadas

Cada integrante prueba servicios que no escribió. Se anota el resultado de cada uno.

| Quién prueba | Servicio | Fecha | Resultado | Problemas encontrados |
|---|---|---|---|---|
| Agus | Users | | | |
| Agus | Notifications | | | |
| Pili | Orders | | | |
| Rama | Products | | | |
| Rama | Cart | | | |

## Preguntas de defensa

1. ¿Cómo se prueban los errores que dependen de otro servicio, como PRD-005 o NTF-004?
   Apagando el servicio del que depende. Si Users está apagado, Notifications no puede saber
   si el usuario existe, así que responde 500 con NTF-004 y no guarda nada.
2. ¿Cómo se comprueba que el Correlation ID viaja entre servicios?
   Se manda un `X-Correlation-Id` conocido en el request y se busca ese valor en las consolas
   de los dos servicios involucrados. Tiene que aparecer en los logs de ambos.
3. ¿Cómo se prueba USR-005 si no hay un endpoint para bloquear usuarios?
   Se edita la base a mano: `activo = 0` e `intentosFallidos = 0` en `users.db`. Con menos de 3
   intentos, el login lo distingue como bloqueo manual y responde USR-005.