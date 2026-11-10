namespace Orders.API.Services
{
    /// <summary>Consulta usuarios en Users API.</summary>
    public interface IUsersClient
    {
        /// <summary>true si el usuario existe, false si Users responde 404. Cualquier otra falla lanza excepción.</summary>
        Task<bool> Existe(Guid usuarioId);
    }
}