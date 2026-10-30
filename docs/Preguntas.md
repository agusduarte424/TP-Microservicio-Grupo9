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