namespace tecnizium_nfe.DTOs;

public class CertificadoDto
{
    public string CNPJ { get; set; }
    public IFormFile Certificado { get; set; }
    public string? Senha { get; set; }
}