namespace Products.API.Services
{
    /// <summary>Consulta a Orders.API si un producto está en órdenes activas.</summary>
    public interface IOrdersClient
    {
        /// <summary>true si hay órdenes Pendiente o Confirmada con ese producto.</summary>
        Task<bool> TieneOrdenesActivas(Guid productoId);
    }
}