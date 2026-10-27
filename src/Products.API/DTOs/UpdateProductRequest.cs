using System.ComponentModel.DataAnnotations;

namespace Products.API.DTOs
{
    /// <summary>Datos para actualizar un producto (PUT /api/products/{id}).</summary>
    public class UpdateProductRequest
    {
        /// <summary>Nombre del producto. Requerido, máximo 100 caracteres.</summary>
        [Required(ErrorMessage = "El nombre es requerido.")]
        [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Descripción. Opcional, máximo 500 caracteres.</summary>
        [StringLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public string? Descripcion { get; set; }

        /// <summary>Precio unitario. Requerido, mayor a 0.</summary>
        [Required(ErrorMessage = "El precio es requerido.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0.")]
        public decimal? Precio { get; set; }

        /// <summary>Unidades en stock. Requerido, mayor o igual a 0.</summary>
        [Required(ErrorMessage = "El stock es requerido.")]
        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
        public int? Stock { get; set; }

        /// <summary>Categoría (informativa, no se valida contra una lista).</summary>
        [Required(ErrorMessage = "La categoría es requerida.")]
        public string Categoria { get; set; } = string.Empty;
    }
}