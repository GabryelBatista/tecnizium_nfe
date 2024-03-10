using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using DFe.Classes.Flags;
using DFe.Utils;
using DFe.Utils.Assinatura;
using NFe.Classes;
using NFe.Classes.Informacoes;
using NFe.Classes.Informacoes.Cobranca;
using NFe.Classes.Informacoes.Destinatario;
using NFe.Classes.Informacoes.Detalhe;
using NFe.Classes.Informacoes.Detalhe.Tributacao;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Estadual.Tipos;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Federal;
using NFe.Classes.Informacoes.Detalhe.Tributacao.Federal.Tipos;
using NFe.Classes.Informacoes.Emitente;
using NFe.Classes.Informacoes.Identificacao;
using NFe.Classes.Informacoes.Identificacao.Tipos;
using NFe.Classes.Informacoes.Observacoes;
using NFe.Classes.Informacoes.Pagamento;
using NFe.Classes.Informacoes.Total;
using NFe.Classes.Informacoes.Transporte;
using NFe.Classes.Servicos.ConsultaCadastro;
using NFe.Classes.Servicos.Tipos;
using NFe.Servicos;
using NFe.Servicos.Retorno;
using NFe.Utils;
using NFe.Utils.Excecoes;
using NFe.Utils.InformacoesSuplementares;
using NFe.Utils.NFe;
using tecnizium_nfe.Config;
using tecnizium_nfe.Models;

namespace tecnizium_nfe.Services;

public class NFecService : INFecService
{
    private readonly NFecConfig _nFecConfig;

    public NFecService(NFecConfig nFecConfig)
    {
        _nFecConfig = nFecConfig;
    }

    public Task<RetornoBasico> ServiceStatus()
    {
        try
        {
            using (ServicosNFe servicoNFe = new ServicosNFe(_nFecConfig.CfgServico))
            {
                var retornoStatus = servicoNFe.NfeStatusServico();
                return Task.FromResult<RetornoBasico>(retornoStatus);
            }
        }
        catch (ComunicacaoException ex)
        {
            throw ex;
        }
        catch (ValidacaoSchemaException ex)
        {
            throw ex;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public Task<RetornoBasico> ConsultaCadastroContribuinte(string uf, string tipoDocumento, string documento)
    {
        try
        {
            if (string.IsNullOrEmpty(uf))
            {
                throw new Exception("A UF deve ser informada!");
            }

            if (uf.Length != 2)
            {
                throw new Exception("UF deve conter 2 caracteres!");
            }

            if (string.IsNullOrEmpty(tipoDocumento))
            {
                throw new Exception("O Tipo de documento deve ser informado!");
            }

            if (tipoDocumento.Length != 1)
            {
                throw new Exception("O Tipo de documento deve conter um apenas um número!");
            }

            if (!tipoDocumento.All(char.IsDigit))
            {
                throw new Exception("O Tipo de documento deve ser um número inteiro");
            }

            int intTipoDocumento = int.Parse(tipoDocumento);
            if (!(intTipoDocumento >= 0 && intTipoDocumento <= 2))
            {
                throw new Exception("Tipos válidos: (0 - IE; 1 - CNPJ; 2 - CPF)");
            }

            if (string.IsNullOrEmpty(documento))
            {
                throw new Exception("O Documento(IE/CNPJ/CPF) deve ser informado!");
            }

            ServicosNFe servicoNFe = new ServicosNFe(_nFecConfig.CfgServico);
            var retornoConsulta =
                servicoNFe.NfeConsultaCadastro(uf, (ConsultaCadastroTipoDocumento)intTipoDocumento, documento);
            return Task.FromResult<RetornoBasico>(retornoConsulta);
        }
        catch (ComunicacaoException ex)
        {
            throw ex;
        }
        catch (ValidacaoSchemaException ex)
        {
            throw ex;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public Task<RetornoNFeAutorizacao> EnviaNfeAssincrono(List<ProdutoModel> produtos)
    {
        try
        {
            var numero = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();
            var lote = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            if (string.IsNullOrEmpty(numero)) throw new Exception("O Número deve ser informado!");


            if (string.IsNullOrEmpty(lote)) throw new Exception("A Id do lote deve ser informada!");

            var versaoServico = _nFecConfig.CfgServico.VersaoNFeAutorizacao;
            var modelo = _nFecConfig.CfgServico.ModeloDocumento;


            var nfe = GetNf(_nFecConfig, Convert.ToInt32(numero), modelo, versaoServico, produtos);
            nfe.Assina();


            if (nfe.infNFe.ide.mod == ModeloDocumento.NFCe)
            {
                nfe.infNFeSupl = new infNFeSupl();
                if (versaoServico == VersaoServico.Versao400)
                    nfe.infNFeSupl.urlChave =
                        nfe.infNFeSupl.ObterUrlConsulta(nfe, _nFecConfig.ConfiguracaoDanfeNfce.VersaoQrCode);
                nfe.infNFeSupl.qrCode = nfe.infNFeSupl.ObterUrlQrCode(nfe,
                    _nFecConfig.ConfiguracaoDanfeNfce.VersaoQrCode, _nFecConfig.CIdToken, _nFecConfig.Csc);
            }

            nfe.Valida();
            

            ServicosNFe servicoNFe = new ServicosNFe(_nFecConfig.CfgServico);
            var retornoEnvio = servicoNFe.NFeAutorizacao(Convert.ToInt32(lote), IndicadorSincronizacao.Assincrono,
                new List<NFe.Classes.NFe> { nfe }, false);

            return Task.FromResult<RetornoNFeAutorizacao>(retornoEnvio);
        }
        catch (ComunicacaoException ex)
        {
            //Faça o tratamento de contingência OffLine aqui.
            throw ex;
        }
        catch (ValidacaoSchemaException ex)
        {
            throw ex;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public async Task<string> DownloadXML(string chave)
    {
        try
        {
            bool manifestar = string.Equals("n", "s");


            using (var _certificado = CertificadoDigital.ObterCertificado(_nFecConfig.CfgServico.Certificado))
            using (var servicoNFe = new ServicosNFe(_nFecConfig.CfgServico, _certificado))
            {
                if (manifestar)
                {
                    try
                    {
                        var retornoManifestacao = servicoNFe.RecepcaoEventoManifestacaoDestinatario(idlote: 1,
                            sequenciaEvento: 1,
                            chavesNFe: new string[] { chave },
                            nFeTipoEventoManifestacaoDestinatario: NFeTipoEvento.TeMdCienciaDaOperacao,
                            cpfcnpj: _nFecConfig.Emitente.CNPJ,
                            justificativa: null);

                        return retornoManifestacao.RetornoStr;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                var retornoNFeDistDFe = servicoNFe.NfeDistDFeInteresse(
                    ufAutor: _nFecConfig.Emitente.enderEmit.UF.ToString(), documento: _nFecConfig.Emitente.CNPJ,
                    chNFE: chave);
                if (retornoNFeDistDFe.Retorno.loteDistDFeInt == null)
                {
                    await Task.Delay(2000);

                    retornoNFeDistDFe = servicoNFe.NfeDistDFeInteresse(
                        ufAutor: _nFecConfig.Emitente.enderEmit.UF.ToString(), documento: _nFecConfig.Emitente.CNPJ,
                        chNFE: chave);

                    if (retornoNFeDistDFe.Retorno.loteDistDFeInt == null)
                        throw new Exception(retornoNFeDistDFe.Retorno.xMotivo);
                }

                if ((retornoNFeDistDFe.Retorno.loteDistDFeInt.Count()) > 0)
                {
                    var xmlBytes = retornoNFeDistDFe.Retorno.loteDistDFeInt[0].XmlNfe;
                    string xmlStr = Compressao.Unzip(xmlBytes);

                    return xmlStr;
                }
                else
                {
                    throw new Exception(retornoNFeDistDFe.Retorno.xMotivo);
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    public Task<X509Certificate2> CarregaDadosCertificado(string caminho, string? password)
    {
        try
        {
            var cert = CertificadoDigitalUtils.ObterDoCaminho(caminho, password);
            _nFecConfig.CfgServico.Certificado.Serial = cert.SerialNumber;
            return Task.FromResult(cert);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    #region Funcoes Auxiliares

    private static string FormatXml(string xml)
    {
        try
        {
            XDocument doc = XDocument.Parse(xml);
            return doc.ToString();
        }
        catch (Exception)
        {
            return xml;
        }
    }

    #endregion

    #region Criar NFe

    private static NFe.Classes.NFe GetNf(NFecConfig nFecConfig, int numero, ModeloDocumento modelo,
        VersaoServico versao, List<ProdutoModel> produtos)
    {
        NFe.Classes.NFe nf = new NFe.Classes.NFe { infNFe = GetInf(nFecConfig, numero, modelo, versao, produtos) };
        return nf;
    }

    private static infNFe GetInf(NFecConfig nfecConfig, int numero, ModeloDocumento modelo, VersaoServico versao,
        List<ProdutoModel> produtos)
    {
        infNFe infNFe = new infNFe
        {
            versao = versao.VersaoServicoParaString(),
            ide = GetIdentificacao(nfecConfig, numero, modelo, versao),
            emit = nfecConfig.Emitente,
            dest = GetDestinatario(versao, modelo),
            transp = GetTransporte()
        };

        for (int i = 0; i < produtos.Count; i++)
        {
            infNFe.det.Add(GetDetalhe(i, infNFe.emit.CRT, modelo, produtos[i]));
        }

        infNFe.total = GetTotal(versao, infNFe.det);

        if (infNFe.ide.mod == ModeloDocumento.NFCe ||
            (infNFe.ide.mod == ModeloDocumento.NFe & versao == VersaoServico.Versao400))
        {
            infNFe.pag = GetPagamento(infNFe.total.ICMSTot, versao); //NFCe Somente  
        }

        if (infNFe.ide.mod == ModeloDocumento.NFCe & versao != VersaoServico.Versao400)
        {
            infNFe.infAdic = new infAdic() { infCpl = "" }; //Susgestão para impressão do troco em NFCe
        }

        return infNFe;
    }

    private static ide GetIdentificacao(NFecConfig nfecConfig, int numero, ModeloDocumento modelo, VersaoServico versao)
    {
        ide ide = new ide
        {
            cUF = nfecConfig.Emitente.enderEmit.UF,
            natOp = "VENDA",
            mod = modelo,
            serie = 1,
            nNF = numero,
            tpNF = TipoNFe.tnSaida,
            cMunFG = nfecConfig.Emitente.enderEmit.cMun,
            tpEmis = nfecConfig.CfgServico.tpEmis,
            tpImp = TipoImpressao.tiRetrato,
            cNF = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
            tpAmb = nfecConfig.CfgServico.tpAmb,
            finNFe = FinalidadeNFe.fnNormal,
            verProc = "1.000"
        };

        if (ide.tpEmis != TipoEmissao.teNormal)
        {
            ide.dhCont = DateTime.Now;
            ide.xJust = "TESTE DE CONTIGÊNCIA PARA NFe/NFCe";
        }

        #region V2.00

        if (versao == VersaoServico.Versao200)
        {
            ide.dEmi = DateTime.Today; //Mude aqui para enviar a nfe vinculada ao EPEC, V2.00
            ide.dSaiEnt = DateTime.Today;
        }

        #endregion

        #region V3.00

        if (versao == VersaoServico.Versao200)
        {
            return ide;
        }

        if (versao == VersaoServico.Versao310)
        {
            ide.indPag = IndicadorPagamento.ipVista;
        }


        ide.idDest = DestinoOperacao.doInterna;
        ide.dhEmi = DateTime.Now;
        //Mude aqui para enviar a nfe vinculada ao EPEC, V3.10
        if (ide.mod == ModeloDocumento.NFe)
        {
            ide.dhSaiEnt = DateTime.Now;
        }
        else
        {
            ide.tpImp = TipoImpressao.tiNFCe;
        }

        ide.procEmi = ProcessoEmissao.peAplicativoContribuinte;
        ide.indFinal = ConsumidorFinal.cfConsumidorFinal; //NFCe: Tem que ser consumidor Final
        ide.indPres = PresencaComprador.pcPresencial; //NFCe: deve ser 1 ou 4

        #endregion

        return ide;
    }

    private static dest GetDestinatario(VersaoServico versao, ModeloDocumento modelo)
    {
        dest dest = new dest(versao)
        {
            //CNPJ = "99999999000191",
            CPF = "99999999999",
        };
      
        //dest.indIEDest = indIEDest.NaoContribuinte; //NFCe: Tem que ser não contribuinte V3.00 Somente
        //dest.email = "teste@gmail.com"; //V3.00 Somente
        return dest;
    }
    

    private static det GetDetalhe(int i, CRT crt, ModeloDocumento modelo, ProdutoModel produto)
    {
        det det = new det
        {
            nItem = i + 1,
            prod = produto,
            imposto = new imposto
            {
                vTotTrib = produto.vUnTrib * produto.qTrib,

                ICMS = new ICMS
                {
                    TipoICMS =
                        crt == CRT.SimplesNacional
                            ? InformarCSOSN(Csosnicms.Csosn102)
                            : InformarICMS(Csticms.Cst00, VersaoServico.Versao310)
                },


                COFINS = new COFINS
                {
                    TipoCOFINS = new COFINSOutr { CST = CSTCOFINS.cofins99, pCOFINS = 0, vBC = 0, vCOFINS = 0 }
                },

                PIS = new PIS
                {
                    TipoPIS = new PISOutr { CST = CSTPIS.pis99, pPIS = 0, vBC = 0, vPIS = 0 }
                }
            }
        };

        return det;
    }

    private static ICMSBasico InformarICMS(Csticms CST, VersaoServico versao)
    {
        ICMS20 icms20 = new ICMS20
        {
            orig = OrigemMercadoria.OmNacional,
            CST = Csticms.Cst20,
            modBC = DeterminacaoBaseIcms.DbiValorOperacao,
            vBC = 1.1m,
            pICMS = 18,
            vICMS = 0.20m,
            motDesICMS = MotivoDesoneracaoIcms.MdiTaxi
        };
        if (versao == VersaoServico.Versao310)
        {
            icms20.vICMSDeson = 0.10m; //V3.00 ou maior Somente
        }

        switch (CST)
        {
            case Csticms.Cst00:
                return new ICMS00
                {
                    CST = Csticms.Cst00,
                    modBC = DeterminacaoBaseIcms.DbiValorOperacao,
                    orig = OrigemMercadoria.OmNacional,
                    pICMS = 18,
                    vBC = 1.1m,
                    vICMS = 0.20m
                };
            case Csticms.Cst20:
                return icms20;
            //Outros casos aqui
        }

        return new ICMS10();
    }

    private static ICMSBasico InformarCSOSN(Csosnicms CST)
    {
        switch (CST)
        {
            case Csosnicms.Csosn101:
                return new ICMSSN101
                {
                    CSOSN = Csosnicms.Csosn101,
                    orig = OrigemMercadoria.OmNacional
                };
            case Csosnicms.Csosn102:
                return new ICMSSN102
                {
                    CSOSN = Csosnicms.Csosn102,
                    orig = OrigemMercadoria.OmNacional
                };
            //Outros casos aqui
            default:
                return new ICMSSN201();
        }
    }

    private static total GetTotal(VersaoServico versao, List<det> produtos)
    {
        ICMSTot icmsTot = new ICMSTot
        {
            vProd = produtos.Sum(p => p.prod.vProd),
            vDesc = produtos.Sum(p => p.prod.vDesc ?? 0),
            vTotTrib = produtos.Sum(p => p.imposto.vTotTrib ?? 0),
        };

        if (versao == VersaoServico.Versao310 || versao == VersaoServico.Versao400)
        {
            icmsTot.vICMSDeson = 0;
        }

        if (versao == VersaoServico.Versao400)
        {
            icmsTot.vFCPUFDest = 0;
            icmsTot.vICMSUFDest = 0;
            icmsTot.vICMSUFRemet = 0;
            icmsTot.vFCP = 0;
            icmsTot.vFCPST = 0;
            icmsTot.vFCPSTRet = 0;
            icmsTot.vIPIDevol = 0;
        }

        foreach (var produto in produtos)
        {
            if (produto.imposto.IPI != null && produto.imposto.IPI.TipoIPI.GetType() == typeof(IPITrib))
            {
                icmsTot.vIPI = icmsTot.vIPI + ((IPITrib)produto.imposto.IPI.TipoIPI).vIPI ?? 0;
            }

            if (produto.imposto.ICMS.TipoICMS.GetType() == typeof(ICMS00))
            {
                icmsTot.vBC = icmsTot.vBC + ((ICMS00)produto.imposto.ICMS.TipoICMS).vBC;
                icmsTot.vICMS = icmsTot.vICMS + ((ICMS00)produto.imposto.ICMS.TipoICMS).vICMS;
            }

            if (produto.imposto.ICMS.TipoICMS.GetType() == typeof(ICMS20))
            {
                icmsTot.vBC = icmsTot.vBC + ((ICMS20)produto.imposto.ICMS.TipoICMS).vBC;
                icmsTot.vICMS = icmsTot.vICMS + ((ICMS20)produto.imposto.ICMS.TipoICMS).vICMS;
            }
            //Outros Ifs aqui, caso vá usar as classes ICMS00, ICMS10 para totalizar
        }

        //** Regra de validação W16-10 que rege sobre o Total da NF **//
        icmsTot.vNF =
            icmsTot.vProd
            - icmsTot.vDesc
            - icmsTot.vICMSDeson.GetValueOrDefault()
            + icmsTot.vST
            + icmsTot.vFCPST.GetValueOrDefault()
            + icmsTot.vFrete
            + icmsTot.vSeg
            + icmsTot.vOutro
            + icmsTot.vII
            + icmsTot.vIPI
            + icmsTot.vIPIDevol.GetValueOrDefault();

        total t = new total { ICMSTot = icmsTot };
        return t;
    }

    private static transp GetTransporte()
    {

        transp t = new transp
        {
            modFrete = ModalidadeFrete.mfSemFrete
        };

        return t;
    }

    private static List<pag> GetPagamento(ICMSTot icmsTot, VersaoServico versao)
    {
        decimal valorPagto = (icmsTot.vNF / 2).Arredondar(2);

        if (versao != VersaoServico.Versao400) // difernte de versão 4 retorna isso
        {
            List<pag> p = new List<pag>
            {
                new pag { tPag = FormaPagamento.fpDinheiro, vPag = valorPagto },
                //new pag { tPag = FormaPagamento.fpCheque, vPag = icmsTot.vNF - valorPagto }
            };
            return p;
        }


        // igual a versão 4 retorna isso
        List<pag> p4 = new List<pag>
        {
            //new pag {detPag = new detPag {tPag = FormaPagamento.fpDinheiro, vPag = valorPagto}},
            //new pag {detPag = new detPag {tPag = FormaPagamento.fpCheque, vPag = icmsTot.vNF - valorPagto}}
            new pag
            {
                detPag = new List<detPag>
                {
                    new detPag { tPag = FormaPagamento.fpDinheiro, vPag = valorPagto },
                }
            }
        };


        return p4;
    }

    #endregion
}