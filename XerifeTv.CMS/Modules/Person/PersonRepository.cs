using Microsoft.Extensions.Options;
using MongoDB.Driver;
using XerifeTv.CMS.Modules.Abstractions.Repositories;
using XerifeTv.CMS.Modules.Person.Interfaces;
using XerifeTv.CMS.Shared.Database.MongoDB;

namespace XerifeTv.CMS.Modules.Person;

public sealed class PersonRepository(IOptions<DBSettings> options)
  : BaseRepository<PersonEntity>(ECollection.PEOPLE, options), IPersonRepository
{
    public async Task UpsertAsync(PersonEntity entity)
    {
        entity.UpdateAt = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(r => r.Id == entity.Id, entity, new ReplaceOptions { IsUpsert = true });
    }
}
