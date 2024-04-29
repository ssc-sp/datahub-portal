namespace Datahub.Application.Services;

public interface IMiscStorageService
{
    Task SaveObject<T>(T obj, string id);
    Task SaveObjects<T>(IEnumerable<T> objects, Func<T, string> idGenerator);
    Task<T> GetObject<T>(string id);
    Task<IEnumerable<T>> GetAllObjects<T>();
}