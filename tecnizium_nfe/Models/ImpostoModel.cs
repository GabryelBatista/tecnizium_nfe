using NFe.Classes.Informacoes.Detalhe.Tributacao;
using tecnizium_nfe.DTOs;

namespace tecnizium_nfe.Models;

public class ImpostoModel : imposto
{
    public ImpostoModel(ProdutoDto produto)
    {
        this.vTotTrib = produto.ValorUnitario * produto.Quantidade;
    }
}