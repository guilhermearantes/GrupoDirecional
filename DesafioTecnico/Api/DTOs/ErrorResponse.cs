using System.Text.Json.Serialization;

namespace DesafioTecnico.Api.DTOs
{
    /// <summary>Corpo padrão retornado pela API em respostas de erro (4xx).</summary>
    /// <param name="Error">Descrição legível do erro ocorrido.</param>
    public record ErrorResponse([property: JsonPropertyName("error")] string Error);
}
