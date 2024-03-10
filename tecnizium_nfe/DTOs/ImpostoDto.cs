namespace tecnizium_nfe.DTOs;

public class ImpostoDto
{
    public IcmsDto Icms { get; set; }
    public PisDto Pis { get; set; }
    public CofinsDto Cofins { get; set; }
}

public class IcmsDto
{
    public string SituacaoTributaria { get; set; }
}

public class PisDto
{
    public string SituacaoTributaria { get; set; }
}

public class CofinsDto
{
    public string SituacaoTributaria { get; set; }
}