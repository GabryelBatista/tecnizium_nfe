using Microsoft.AspNetCore.Mvc;
using tecnizium_nfe.Config;
using tecnizium_nfe.DTOs;
using tecnizium_nfe.Helpers;
using tecnizium_nfe.models;
using tecnizium_nfe.Models;
using tecnizium_nfe.Services;

namespace tecnizium_nfe.controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class NFeController : ControllerBase
{
    private readonly IMongoDbService _mongoDbService;

    public NFeController(IMongoDbService mongoDbService)
    {
        _mongoDbService = mongoDbService;
    }


    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Is Working");
    }

    //Dados do Emitente
    [HttpPost]
    public IActionResult DadosEmitente([FromBody] EmitenteDto emitente)
    {
        var emit = new EmitenteModel(emitente);
        _mongoDbService.InsertEmitente(emit);
        return Ok();
    }

    //Upload do Certificado
    [HttpPost]
    public IActionResult UploadCertificado([FromForm] CertificadoDto certificado)
    {
        //Save certificate in wwwroot
        var filePath = FileManagerHelper.SaveFile(certificado.Certificado);
        var cert = new CertificadoModel(certificado.CNPJ, filePath, certificado.Senha);

        //Save certificate in MongoDB
        _mongoDbService.InsertCertificado(cert);

        return Ok();
    }


    //Emissão de NFe
    [HttpPost]
    public async Task<IActionResult> EmissaoNFe([FromHeader] string cnpj, [FromBody] List<ProdutoDto> produto)
    {
        var emitente = await _mongoDbService.GetEmitente(cnpj);
        
        if (emitente == null)
        {
            return BadRequest("Emitente não encontrado");
        }
        var nfecConfig = new NFecConfig(emitente);
        var nfecService = new NFecService(nfecConfig);
        var certificado = await _mongoDbService.GetCertificado(cnpj);

        await nfecService.CarregaDadosCertificado(FileManagerHelper.GetFile(certificado.Certificado),
            certificado.Senha);
        
        var produtos = produto.Select(p => new ProdutoModel(p)).ToList();
        var nfce = await nfecService.EnviaNfeAssincrono(produtos);

        return Ok(nfce);
    }
}