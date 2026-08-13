namespace TechService.Api.Models;

public class Equipamento
{
    public int IdEquipamento { get; set; }

    public int IdCliente { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public string Marca { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;

    public string? NumeroSerie { get; set; }

    public string? Observacoes { get; set; }

    public int Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }
}