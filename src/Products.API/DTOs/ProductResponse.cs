namespace Products.API.DTOs
{
    /// <summary>Producto tal como lo devuelve la API.</summary>
    public class ProductResponse
    {
        /// <summary>Identificador único.</summary>
        public Guid Id { get; set; }

        /// <summary>Nombre del producto.</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Descripción del producto.</summary>
        public string? Descripcion { get; set; }

        /// <summary>Precio unitario.</summary>
        public decimal Precio { get; set; }

        /// <summary>Unidades en stock.</summary>
        public int Stock { get; set; }

        /// <summary>Categoría.</summary>
        public string Categoria { get; set; } = string.Empty;

        /// <summary>Fecha de creación (UTC).</summary>
        public DateTime FechaCreacion { get; set; }
    }
}