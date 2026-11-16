using XerifeTv.CMS.Modules.Abstractions.Interfaces;

namespace XerifeTv.CMS.Modules.Person.Interfaces;

public interface IPersonRepository : IBaseRepository<PersonEntity>
{
    Task UpsertAsync(PersonEntity entity);
}
