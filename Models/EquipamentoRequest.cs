public class EquipamentoRequest
{
    public int IdCliente { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public string Marca { get; set; } = string.Empty;

    public string Modelo { get; set; } = string.Empty;

    public string NumeroSerie { get; set; } = string.Empty;

    public string Observacoes { get; set; } = string.Empty;
}