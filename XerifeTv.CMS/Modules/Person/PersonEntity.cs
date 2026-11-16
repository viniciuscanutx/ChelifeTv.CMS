using XerifeTv.CMS.Modules.Abstractions.Entities;

namespace XerifeTv.CMS.Modules.Person;

// Id = id da pessoa no TMDB; copia local para nao chamar o TMDB a cada acesso
public class PersonEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Biography { get; set; }
    public string? Birthday { get; set; }
    public string? Deathday { get; set; }
    public string? PlaceOfBirth { get; set; }
    public string? ProfileUrl { get; set; }
    public string? KnownForDepartment { get; set; }
    public ICollection<PersonCredit> Credits { get; set; } = [];
    public DateTime FetchedAt { get; set; } = DateTime.UtcNow;
}
