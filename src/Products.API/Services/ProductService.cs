// ProductService tiene la lógica de negocio de Products.
// Solo usa IProductRepository para los datos y lanza las excepciones del catálogo.
using Products.API.Data;
using Products.API.DTOs;
using Products.API.Exceptions;
using Products.API.Models;

namespace Products.API.Services
{
    public class ProductService
    {
        private readonly IProductRepository _repository;

        public ProductService(IProductRepository repository)
        {
            _repository = repository;
        }

        /// <summary>Lista productos, con filtros opcionales por categoría y nombre.</summary>
        public async Task<IEnumerable<ProductResponse>> ObtenerTodos(string? categoria, string? nombre)
        {
            var productos = await _repository.ObtenerTodos(categoria, nombre);
            return productos.Select(ToResponse);
        }

        /// <summary>Devuelve un producto o lanza PRD-001 si no existe.</summary>
        public async Task<ProductResponse> ObtenerPorId(Guid id)
        {
            var producto = await BuscarOFallar(id);
            return ToResponse(producto);
        }

        /// <summary>Crea un producto. Lanza PRD-003 si ya existe el nombre en esa categoría.</summary>
        public async Task<ProductResponse> Crear(CreateProductRequest request)
        {
            if (await _repository.ExistePorNombreYCategoria(request.Nombre, request.Categoria))
                throw new BusinessRuleException(ErrorCodes.ProductoDuplicado,
                    $"Ya existe un producto con ese nombre en la categoría '{request.Categoria}'.");

            var producto = new Product
            {
                Id = Guid.NewGuid(),
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                Precio = request.Precio!.Value,
                Stock = request.Stock!.Value,
                Categoria = request.Categoria,
                FechaCreacion = DateTime.UtcNow
            };

            await _repository.Guardar(producto);
            return ToResponse(producto);
        }

        // se reutiliza en GET, PUT y DELETE
        private async Task<Product> BuscarOFallar(Guid id)
        {
            return await _repository.ObtenerPorId(id)
                ?? throw new NotFoundException(ErrorCodes.ProductoNoEncontrado, "Producto no encontrado.");
        }

        private static ProductResponse ToResponse(Product p) => new()
        {
            Id = p.Id,
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
            Precio = p.Precio,
            Stock = p.Stock,
            Categoria = p.Categoria,
            FechaCreacion = p.FechaCreacion
        };
    }
}