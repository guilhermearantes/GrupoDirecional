namespace DesafioTecnico.Api.DTOs
{
    /// <summary>Envelope de resposta paginada.</summary>
    /// <typeparam name="T">Tipo dos itens na coleção.</typeparam>
    /// <param name="Items">Itens da página atual.</param>
    /// <param name="Page">Número da página atual (base 1).</param>
    /// <param name="PageSize">Quantidade máxima de itens por página.</param>
    /// <param name="TotalCount">Total de registros na coleção completa.</param>
    public record PagedResult<T>(
        IEnumerable<T> Items,
        int Page,
        int PageSize,
        int TotalCount
    )
    {
        /// <summary>Total de páginas calculado a partir de <see cref="TotalCount"/> e <see cref="PageSize"/>.</summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
