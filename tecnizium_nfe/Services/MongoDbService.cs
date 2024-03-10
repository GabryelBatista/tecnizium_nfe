using MongoDB.Driver;
using tecnizium_nfe.models;

namespace tecnizium_nfe.Services;

public class MongoDbService : IMongoDbService
{
    private readonly IMongoCollection<EmitenteModel> _emitenteCollection;
    private readonly IMongoCollection<CertificadoModel> _certificadoCollection;

    public MongoDbService(IConfiguration config, IMongoClient client)
    {
        var database = client.GetDatabase(config["MongoDb:Database"]);
        _emitenteCollection = database.GetCollection<EmitenteModel>("Emitente");
        _certificadoCollection = database.GetCollection<CertificadoModel>("Certificado");
    }

    public Task InsertEmitente(EmitenteModel emitente)
    {
        _emitenteCollection.InsertOne(emitente);
        return Task.CompletedTask;
    }

    public Task InsertCertificado(CertificadoModel certificado)
    {
        _certificadoCollection.InsertOne(certificado);
        return Task.CompletedTask;
    }

    public Task<EmitenteModel> GetEmitente(string cnpj)
    {
        var emitente = _emitenteCollection.Find(e => e.CNPJ == cnpj).FirstOrDefault();
        return Task.FromResult(emitente);
    }

    public Task<CertificadoModel> GetCertificado(string cnpj)
    {
        var certificado = _certificadoCollection.Find(c => c.CNPJ == cnpj).FirstOrDefault();
        return Task.FromResult(certificado);
    }
}