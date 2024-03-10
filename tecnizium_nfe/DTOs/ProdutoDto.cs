namespace tecnizium_nfe.DTOs;

public class ProdutoDto
{
    public int Cfop { get; set; }
    public string Codigo { get; set; }
    public string Descricao { get; set; }
    public string Ncm { get; set; }
    public string Ean { get; set; }
    public string Cest { get; set; }
    public decimal Quantidade { get; set; }
    public string UnidadeMedida { get; set; }
    public decimal ValorUnitario { get; set; }

    //public ImpostoDto Imposto { get; set; }
}