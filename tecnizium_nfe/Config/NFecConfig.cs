using System.Net;
using DFe.Classes.Flags;
using DFe.Utils;
using NFe.Classes.Informacoes.Emitente;
using NFe.Classes.Informacoes.Identificacao.Tipos;
using NFe.Danfe.Base.NFCe;
using NFe.Utils;
using NFe.Utils.Email;
using tecnizium_nfe.models;

namespace tecnizium_nfe.Config;

public class NFecConfig
{
    private ConfiguracaoServico _cfgServico;

    public NFecConfig(EmitenteModel emitente)
    {
        CfgServico = ConfiguracaoServico.Instancia;
        CfgServico.tpAmb = TipoAmbiente.Homologacao;
        CfgServico.tpEmis = TipoEmissao.teNormal;
        CfgServico.ProtocoloDeSeguranca = ServicePointManager.SecurityProtocol;
        CfgServico.VersaoNFeAutorizacao = VersaoServico.Versao400;
        CfgServico.ModeloDocumento = ModeloDocumento.NFCe;
        ConfiguracaoDanfeNfce = new ConfiguracaoDanfeNfce();


        Emitente = emitente;
        // = new ConfiguracaoEmail("email@dominio.com", "senha", "Envio de NFE", Properties.Resources.MensagemHtml, "smtp.dominio.com", 587, true, true);
    }


    public ConfiguracaoServico CfgServico
    {
        get
        {
            ConfiguracaoServico.Instancia.CopiarPropriedades(_cfgServico);
            return _cfgServico;
        }
        set
        {
            _cfgServico = value;
            ConfiguracaoServico.Instancia.CopiarPropriedades(value);
        }
    }

    public emit Emitente { get; set; }
    public ConfiguracaoEmail ConfiguracaoEmail { get; set; }
    public ConfiguracaoDanfeNfce ConfiguracaoDanfeNfce { get; set; }
    public string CIdToken { get; set; }
    public string Csc { get; set; }
}