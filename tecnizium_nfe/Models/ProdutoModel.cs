using NFe.Classes.Informacoes.Detalhe;
using tecnizium_nfe.DTOs;

namespace tecnizium_nfe.Models;

public class ProdutoModel : prod
{
    public ProdutoModel(ProdutoDto produto)
    {
        this.CFOP = produto.Cfop;
        this.cProd = produto.Codigo;
        this.xProd = produto.Descricao;
        this.NCM = produto.Ncm;
        this.cEAN = produto.Ean;
        this.cEANTrib = produto.Ean;
        this.CEST = produto.Cest;
        this.qCom = produto.Quantidade;
        this.qTrib = produto.Quantidade;
        this.uCom = produto.UnidadeMedida;
        this.uTrib = produto.UnidadeMedida;
        this.vUnCom = produto.ValorUnitario;
        this.vUnTrib = produto.ValorUnitario;
        this.vProd = produto.ValorUnitario * produto.Quantidade;
        this.indTot = IndicadorTotal.ValorDoItemCompoeTotalNF;
    }
}