using System.Security.Cryptography.X509Certificates;
using NFe.Servicos.Retorno;
using tecnizium_nfe.Config;
using tecnizium_nfe.Models;

namespace tecnizium_nfe.Services;

public interface INFecService
{
    Task<RetornoBasico> ServiceStatus();
    Task<RetornoBasico> ConsultaCadastroContribuinte(string uf, string tipoDocumento, string documento);
    Task<RetornoNFeAutorizacao> EnviaNfeAssincrono(List<ProdutoModel> produtos);
    Task<string> DownloadXML(string chave);
    Task<X509Certificate2> CarregaDadosCertificado(string caminho, string? password);
}