namespace Products.API.DTOs
{
    /// <summary>Producto tal como lo devuelve la API.</summary>
    public class ProductResponse
    {
        /// <summary>Identificador único.</summary>
        /// <example>3fa85f64-5717-4562-b3fc-2c963f66afa6</example>
        public Guid Id { get; set; }

        /// <summary>Nombre del producto.</summary>
        /// <example>Notebook Dell XPS 15</example>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Descripción del producto.</summary>
        /// <example>Laptop 15 pulgadas, 32GB RAM</example>
        public string? Descripcion { get; set; }

        /// <summary>Precio unitario.</summary>
        /// <example>1500.00</example>
        public decimal Precio { get; set; }

        /// <summary>Unidades en stock.</summary>
        /// <example>10</example>
        public int Stock { get; set; }

        /// <summary>Categoría.</summary>
        /// <example>Electrónica</example>
        public string Categoria { get; set; } = string.Empty;

        /// <summary>Fecha de creación (UTC).</summary>
        /// <example>2024-01-15T10:30:00Z</example>
        public DateTime FechaCreacion { get; set; }
    }
}