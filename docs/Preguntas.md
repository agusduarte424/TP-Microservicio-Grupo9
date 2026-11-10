# Preguntas para la defensa

## F1 - Esqueleto
1. ¿Por qué cada servicio tiene su propio puerto?
   Porque los 5 corren al mismo tiempo en la misma compu, y dos programas no pueden escuchar en el mismo puerto.
2. ¿Por qué cada servicio tiene su propia base de datos?
   Para que sean independientes: si uno se cae o cambia su tabla, no rompe a los demás.
3. ¿Para qué sirve el .gitignore?
   Para no subir archivos que se generan solos (bin, obj), configuraciones personales del editor, las bases locales ni los logs.

## F2 - Logging con Serilog
1. ¿Por qué Serilog se configura antes que todo en Program.cs?
   Para que, si algo falla mientras arranca el servicio, ese error ya quede registrado.
2. ¿Por qué dos salidas, consola y archivo?
   La consola es para leer mientras se desarrolla. El archivo JSON queda guardado y se puede buscar o procesar con herramientas, por ejemplo filtrar todos los logs de un CorrelationId.
3. ¿Cómo llega el Endpoint a todos los logs de un request?
   El middleware lo carga con LogContext.PushProperty al empezar el request, y Enrich.FromLogContext lo agrega a cada log que se escribe mientras dura ese request.
4. ¿Por qué un 404 es Warning y no Error?
   Porque es un error del cliente o de negocio, que el sistema maneja como corresponde. Error queda para lo inesperado: 5xx y excepciones que nadie manejó.

## F3 - Correlation ID
1. ¿Para qué sirve el Correlation ID?
   Para seguir una operación que pasa por varios servicios: todos sus logs llevan el mismo id, y buscándolo se ve el recorrido completo.
2. ¿Por qué se respeta el id que llega en el header en vez de generar siempre uno nuevo?
   Porque si otro servicio ya lo generó, cambiarlo cortaría el rastro: los logs de cada servicio quedarían con ids distintos.
3. ¿Cómo viaja el id a otro servicio?
   CorrelationIdHandler es un DelegatingHandler: se mete en cada llamada que hace un HttpClient y le agrega el header X-Correlation-Id antes de enviarla.
4. ¿Por qué UseCorrelationId va antes que UseRequestLogging?
   Porque los middlewares se ejecutan en orden. Si fuera después, el log "Inicio del request" se escribiría antes de que exista el id.


## S2 - Users
1. ¿Cómo y cuándo se bloquea un usuario, y qué distingue a USR-004 de USR-005?
   Cada login con contraseña incorrecta suma 1 a IntentosFallidos. Al llegar a 3, Activo pasa a false y responde USR-004. USR-005 también es un 403 con Activo = false, pero con menos de 3 intentos: es el bloqueo manual por fraude.
2. ¿Por qué se guarda un hash y no la contraseña?
   Porque un hash no se puede revertir: si roban la base, no ven contraseñas. PBKDF2 con sal aleatoria y 100.000 iteraciones hace que dos usuarios con la misma contraseña tengan hashes distintos y que probar contraseñas sea lento.
3. ¿Por qué USR-003 es 401 y USR-004 es 403?
   El 401 significa que no se pudo verificar quién sos (credenciales que no coinciden). El 403 significa que se sabe quién sos pero la cuenta tiene el acceso bloqueado.


## S5 - Notifications
1. ¿Qué pasa si Users API está caída cuando se envía una notificación?
   La llamada lanza una excepción, el Service se corta antes de guardar y llega al handler genérico, que responde 500 con NTF-004. No queda ninguna notificación registrada.
2. ¿Por qué un 404 de Users es NTF-001 y un timeout es NTF-004?
   Un 404 es una respuesta de negocio: el usuario no existe. Un timeout o un 5xx significa que no pudimos verificarlo, así que es un error interno de este servicio. Mezclarlos le diría al cliente que el usuario no existe cuando en realidad no se pudo consultar.
3. ¿Cómo viaja el Correlation ID de Notifications a Users y dónde se ve?
   CorrelationIdHandler es un DelegatingHandler que AddCorrelationId() engancha a todos los HttpClient. Agrega el header X-Correlation-Id del request actual a la llamada saliente, y el mismo valor aparece en los logs de los dos servicios.


## F6 - Swagger
1. ¿De dónde saca Swagger las descripciones de los endpoints?
   De los /// <summary> del código. GenerateDocumentationFile hace que el compilador los guarde en un .xml y IncludeXmlComments se lo pasa a Swagger.
2. ¿Cómo se documenta un error en un endpoint?
   Con [ProducesResponseType] para el status y [ErrorExample] para el código y el mensaje. Un filtro de Swagger convierte el atributo en un ejemplo con el contrato de error de la consigna.
3. ¿Por qué se usa Swashbuckle 6.6.2 y no la versión más nueva?
   Porque es la versión del plan y las 10.x cambian la API de Microsoft.OpenApi, lo que rompe los filtros. Además no puede convivir con Microsoft.AspNetCore.OpenApi, por eso se quitó.


## S4 - Cart
1. ¿Por qué el stock se compara contra el total y no contra lo que se agrega?
   Porque si ya tenés 3 unidades y agregás 2, lo que importa es que haya 5 disponibles. Compararlo solo contra el 2 dejaría pasar un carrito que supera el stock.
2. ¿Qué pasa si Products está caído cuando se agrega un producto?
   La llamada lanza una excepción, el Service se corta antes de guardar y llega al handler genérico, que responde 500 con CRT-005. Si Products responde 404, en cambio, es CRT-002.
3. ¿Por qué CRT-003 es 422 y no 400?
   El request está bien formado, así que no es un 400. Lo que falla es una regla de negocio (no hay stock), y eso es un 422.


## F7 - Persistencia
1. ¿Por qué el Service usa una interfaz de repositorio y no la clase directamente?
   Para no depender de SQLite. Si cambia la base, o se quiere probar con un repositorio falso, el Service no se toca. Además deja claro el reparto: el repositorio guarda y busca, y el Service decide las reglas.
2. ¿Por qué ningún repositorio hace llamadas HTTP?
   Porque un repositorio solo debe hablar con su base. Llamar a otro servicio es lógica de negocio y va en el Service, que también decide qué error devolver si esa llamada falla.
3. ¿Por qué la persistencia es propia y no la librería de la cátedra?
   Porque la consigna menciona una librería provista por la cátedra, pero no fue entregada. El grupo armó SQLite con Dapper detrás de una interfaz por servicio, y lo declara en el README.



## C2 - Pruebas
1. ¿Cómo se prueban los errores que dependen de otro servicio, como PRD-005 o NTF-004?
   Apagando el servicio del que depende. Si Users está apagado, Notifications no puede saber si el usuario existe, así que responde 500 con NTF-004 y no guarda nada.
2. ¿Cómo se comprueba que el Correlation ID viaja entre servicios?
   Se manda un X-Correlation-Id conocido en el request y se busca ese valor en las consolas de los dos servicios involucrados. Tiene que aparecer en los logs de ambos.
3. ¿Cómo se prueba USR-005 si no hay un endpoint para bloquear usuarios?
   Se edita la base a mano: activo = 0 e intentosFallidos = 0 en users.db. Con menos de 3 intentos, el login lo distingue como bloqueo manual y responde USR-005.

   ## S3 - Orders
1. ¿Por qué se guarda el precio unitario en la orden?
   Porque si después cambia el precio en Products, la orden tiene que seguir mostrando lo que se cobró. Por eso se copia en el momento de crearla.
2. ¿Qué pasa si Users está caído cuando se crea una orden?
   UsersClient lanza una excepción, el Service se corta antes de guardar y el handler genérico responde 500 con ORD-007. Si Users responde 404, en cambio, es ORD-003.
3. ¿Qué transiciones de estado permiten y por qué?
   Pendiente a Confirmada o Cancelada, Confirmada a Enviada o Cancelada, y Enviada a Entregada. Entregada y Cancelada son finales. La consigna no las define, así que seguimos el ciclo real de un pedido: se cancela solo lo que todavía no se envió.
4. ¿Por qué ORD-005 es 422 y ORD-006 es 409?
   El stock insuficiente es una regla de negocio que impide procesar un request bien formado (422). La transición inválida choca con el estado actual de la orden (409).
5. ¿Cómo se garantiza que no quede una orden sin items en la base?
   La orden y sus items se guardan en una transacción: si falla cualquier INSERT, no se guarda nada.
6. ¿En qué orden se valida el POST?
   Primero los datos, que no cuesta nada. Después el usuario y los productos, que requieren llamadas HTTP. Si los datos están mal, no se llama a ningún servicio.