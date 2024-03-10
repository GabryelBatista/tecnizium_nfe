using NFe.Classes.Informacoes.Emitente;

namespace tecnizium_nfe.DTOs;

public class EmitenteDto
{
    public string cnpj { get; set; }
    public string xNome { get; set; }
    public string xFant { get; set; }
    public EnderEmitDto enderEmit { get; set; }
    public string ie { get; set; }
    public string iest { get; set; }
    public string im { get; set; }
    public string cnae { get; set; }
    //public CRT crt { get; set; }
}

public class EnderEmitDto
{
    public string xLgr { get; set; }
    public string nro { get; set; }
    public string xCpl { get; set; }
    public string xBairro { get; set; }
    public long cMun { get; set; }
    public string xMun { get; set; }
    public string proxyUF { get; set; }
    public string cep { get; set; }
    public long fone { get; set; }
}