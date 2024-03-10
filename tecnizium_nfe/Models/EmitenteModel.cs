using NFe.Classes.Informacoes.Emitente;
using tecnizium_nfe.DTOs;

namespace tecnizium_nfe.models;

public class EmitenteModel : emit
{
    public EmitenteModel(EmitenteDto emitente)
    {
        this.CNPJ = emitente.cnpj;
        this.xNome = emitente.xNome;
        this.xFant = emitente.xFant;
        this.enderEmit = new enderEmit
        {
            xLgr = emitente.enderEmit.xLgr,
            nro = emitente.enderEmit.nro,
            xCpl = emitente.enderEmit.xCpl,
            xBairro = emitente.enderEmit.xBairro,
            cMun = emitente.enderEmit.cMun,
            xMun = emitente.enderEmit.xMun,
            ProxyUF = emitente.enderEmit.proxyUF,
            CEP = emitente.enderEmit.cep,
            fone = emitente.enderEmit.fone,
            xPais = "BRASIL",
            cPais = 1058
        };
        this.IE = emitente.ie;
        this.IEST = emitente.iest;
        this.IM = emitente.im;
        this.CNAE = emitente.cnae;
        this.CRT = CRT.SimplesNacional;
    }
}