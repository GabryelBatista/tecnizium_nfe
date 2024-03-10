using NFe.Classes.Informacoes.Detalhe;

namespace tecnizium_nfe.Models;

public class DetalhesModel : det
{
    public DetalhesModel(ProdutoModel produto, ImpostoModel imposto)
    {
        this.prod = produto;
        this.imposto = imposto;
    }
}