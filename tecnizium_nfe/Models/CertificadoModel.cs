namespace tecnizium_nfe.models;

public class CertificadoModel
{
    public string CNPJ { get; set; }
    public string Certificado { get; set; }
    public string? Senha { get; set; }

    public CertificadoModel(string cnpj, string certificado, string? senha)
    {
        CNPJ = cnpj;
        Certificado = certificado;
        Senha = senha;
    }
}