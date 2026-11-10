namespace Orders.API.Services
{
    /// <summary>Datos de Products que necesita Orders para crear una orden.</summary>
    public class ProductoInfo
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public decimal Precio { get; set; }
        public int Stock { get; set; }
    }

    /// <summary>Consulta productos en Products API.</summary>
    public interface IProductsClient
    {
        /// <summary>Devuelve el producto, o null si Products responde 404. Cualquier otra falla lanza excepción.</summary>
        Task<ProductoInfo?> ObtenerProducto(Guid productoId);
    }
}