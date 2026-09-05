namespace BugCycle.Web.ViewModels;

public class ProblemViewModel
{
    public int Status { get; set; }
    public string Message { get; set; } = string.Empty;

    public string Title => Status switch
    {
        400 => "Cerere invalida",
        401 => "Autentificare necesara",
        403 => "Acces interzis",
        404 => "Pagina nu a fost gasita",
        _ => "Eroare"
    };
}
