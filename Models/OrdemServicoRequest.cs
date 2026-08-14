namespace TechService.Api.Models;

public class OrdemServicoRequest
{
    public int IdEquipamento { get; set; }

    public string DefeitoRelatado { get; set; } = string.Empty;

    public string? Diagnostico { get; set; }

    public string? Solucao { get; set; }

    public string Status { get; set; } = "ABERTA";

    public string Prioridade { get; set; } = "MEDIA";

    public decimal ValorServico { get; set; } = 0.00m;

    public decimal ValorPecas { get; set; } = 0.00m;

    public decimal Desconto { get; set; } = 0.00m;
}