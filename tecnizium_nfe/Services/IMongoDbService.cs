using tecnizium_nfe.models;

namespace tecnizium_nfe.Services;

public interface IMongoDbService
{
    public Task InsertEmitente(EmitenteModel emitente);
    public Task InsertCertificado(CertificadoModel certificado);

    public Task<EmitenteModel> GetEmitente(string cnpj);
    public Task<CertificadoModel> GetCertificado(string cnpj);
}