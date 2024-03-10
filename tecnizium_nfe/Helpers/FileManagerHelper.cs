namespace tecnizium_nfe.Helpers;

abstract public class FileManagerHelper
{
    public static string SaveFile(IFormFile file)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "certificates", file.FileName);
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(fileStream);
        }

        return file.FileName;
    }

    public static string GetFile(string fileName)
    {
        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "certificates", fileName);
        return filePath;
    }
}