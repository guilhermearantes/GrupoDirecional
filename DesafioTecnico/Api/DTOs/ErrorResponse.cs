namespace DesafioTecnico.Api.DTOs
{
    /// <summary>Corpo padrão retornado pela API em respostas de erro (4xx).</summary>
    public record ErrorResponse(
        /// <summary>Descrição legível do erro ocorrido.</summary>
        /// <example>Apartamento não disponível para reserva.</example>
        string Error);
}
