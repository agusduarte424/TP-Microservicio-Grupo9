namespace Cart.API.Services
{
    /// <summary>Datos de Products que necesita Cart.</summary>
    public class ProductoInfo
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Stock { get; set; }
    }

    /// <summary>Consulta productos en Products API.</summary>
    public interface IProductsClient
    {
        /// <summary>Devuelve el producto, o null si Products responde 404. Cualquier otra falla lanza excepción.</summary>
        Task<ProductoInfo?> ObtenerProducto(Guid productoId);
    }
}