# Persistencia

Cómo guardan los datos los servicios y cómo la usa un Service. Es una persistencia **propia**:
la consigna menciona una librería provista por la cátedra, pero no fue entregada, así que el
grupo armó la suya con **SQLite y Dapper**.

## Idea general

- Cada servicio tiene **su propia base SQLite**, un archivo `.db` en la carpeta de su proyecto.
  Ningún servicio lee la base de otro: si necesita un dato ajeno, se lo pide por HTTP.
- El Service **solo conoce una interfaz** de repositorio. No sabe que atrás hay SQLite.
- **Ningún repositorio hace llamadas HTTP.** Hablar con otros servicios es trabajo del Service.

```
Controller → Service → I<Entidad>Repository (interfaz) → <Entidad>Repository (Dapper) → archivo .db
```

## Las piezas de cada servicio

| Pieza | Archivo | Qué hace |
|---|---|---|
| Modelo | `Models/<Entidad>.cs` | La entidad del dominio (Apéndice A de la consigna) |
| Interfaz | `Data/I<Entidad>Repository.cs` | El contrato: qué operaciones existen. Es lo único que ve el Service |
| Implementación | `Data/<Entidad>Repository.cs` | Ejecuta el SQL con Dapper |
| Inicializador | `Data/DatabaseInitializer.cs` | Crea las tablas al arrancar, con `CREATE TABLE IF NOT EXISTS` |
| Registro | `Extensions/PersistenceExtensions.cs` | `AddPersistence()` registra todo en la inyección de dependencias; `InitializeDatabase()` crea las tablas |
| Configuración | `appsettings.json` | `ConnectionStrings:DefaultConnection`, por ejemplo `Data Source=products.db` |

`Program.cs` solo usa dos líneas para todo esto:

```csharp
builder.Services.AddPersistence();   // antes de Build()
app.InitializeDatabase();            // después de Build()
```

## Una base por servicio

| Servicio | Archivo | Tablas |
|---|---|---|
| Products.API | `products.db` | `products` |
| Users.API | `users.db` | `users` (el email es `UNIQUE` y `COLLATE NOCASE`) |
| Notifications.API | `notifications.db` | `notifications`, con un índice en `usuarioId` |
| Cart.API | `cart.db` | `carts` y `cart_items` |
| Orders.API | `orders.db` | A definir por quien implemente Orders |

Los archivos `.db` se crean solos la primera vez que arranca el servicio y están en el
`.gitignore`: no se suben al repositorio.

## Cómo la usa un Service

El Service pide la **interfaz** por su constructor, y la inyección de dependencias le entrega
la implementación. Ejemplo simplificado de Products:

```csharp
public class ProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    // el repositorio devuelve null si no existe; el Service decide qué error lanzar
    private async Task<Product> BuscarOFallar(Guid id)
    {
        return await _repository.ObtenerPorId(id)
            ?? throw new NotFoundException(ErrorCodes.ProductoNoEncontrado, "Producto no encontrado.");
    }
}
```

El reparto de responsabilidades es siempre el mismo:

- **El repositorio** guarda y busca. Devuelve `null` o una lista vacía cuando no encuentra algo.
- **El Service** decide qué significa eso: lanza `NotFoundException`, `BusinessRuleException`, etc.

## Convenciones del código

| Tema | Cómo se hace |
|---|---|
| Parámetros | Siempre con `@Nombre` (`WHERE id = @Id`), nunca concatenando texto en el SQL |
| Conexión | Una conexión nueva por operación (`using var conn = CreateConnection()`). SQLite es un archivo local y abrirla es barato |
| `Guid` | Se guarda como `TEXT` (`id.ToString()`), porque SQLite no tiene ese tipo |
| `DateTime` | Se guarda como `TEXT` en formato ISO (`ToString("o")`), que ordenado como texto coincide con el orden cronológico |
| `bool` | Se guarda como `INTEGER` 0 o 1 |
| `decimal` | Se guarda como `REAL` |
| Mapeo | Una clase privada `<Entidad>Row` con los tipos de la base y un método `To<Entidad>` que la convierte al modelo |
| Operaciones de varias tablas | Dentro de una transacción. Por ejemplo, `CartRepository.Guardar` escribe `carts` y `cart_items` juntas |

## Cómo agregar persistencia a un servicio

1. Crear el modelo en `Models/`.
2. Definir la interfaz `I<Entidad>Repository` en `Data/` con las operaciones que necesita el Service.
3. Implementarla en `<Entidad>Repository`, con una conexión por operación y parámetros `@`.
4. Escribir el `DatabaseInitializer` con el `CREATE TABLE IF NOT EXISTS` de cada tabla.
5. Copiar `PersistenceExtensions.cs` y cambiar el namespace y los nombres de la entidad.
6. Poner las dos líneas en `Program.cs` y la cadena de conexión en `appsettings.json`.
7. En el Service, pedir `I<Entidad>Repository` por el constructor y no usar nada más de la base.

## Decisiones de diseño

- **Persistencia propia con SQLite y Dapper.** La cátedra no entregó la librería. Se descartó Entity Framework para mantener el SQL a la vista y no sumar herramientas de migraciones.
- **Una interfaz por servicio.** Permite cambiar la base, o usar un repositorio falso para pruebas, sin tocar la lógica de negocio.
- **Sin llamadas HTTP en los repositorios.** En el repositorio de referencia (G17), `CartRepository` hacía llamadas HTTP. Acá un repositorio solo habla con la base.
- **`CREATE TABLE IF NOT EXISTS` en vez de migraciones.** Es simple y alcanza para el trabajo, pero no actualiza una tabla existente: si cambia el esquema, hay que borrar el archivo `.db` para que se vuelva a crear.
- **`decimal` como `REAL`.** SQLite guarda los `REAL` como números de punto flotante. Para este trabajo alcanza; en un sistema real el dinero se guardaría en centavos enteros.

## Preguntas de defensa

1. ¿Por qué el Service usa una interfaz de repositorio y no la clase directamente?
   Para no depender de SQLite. Si cambia la base, o se quiere probar con un repositorio falso, el Service no se toca. Además deja claro el reparto: el repositorio guarda y busca, y el Service decide las reglas.
2. ¿Por qué ningún repositorio hace llamadas HTTP?
   Porque un repositorio solo debe hablar con su base. Llamar a otro servicio es lógica de negocio y va en el Service, que también decide qué error devolver si esa llamada falla.
3. ¿Por qué la persistencia es propia y no la librería de la cátedra?
   Porque la consigna menciona una librería provista por la cátedra, pero no fue entregada. El grupo armó SQLite con Dapper detrás de una interfaz por servicio, y lo declara en el README.
4. ¿Cómo se evita la inyección de SQL?
   Todos los valores van como parámetros (`@Id`, `@Email`), nunca concatenados en el texto del SQL.