using TestTaskModulBank.Models;

namespace TestTaskModulBank.Interfaces;

public interface IGameRepository
{
    Task<Game?> GetByIdAsync(Guid id);
    Task AddAsync(Game? game);
    Task UpdateAsync(Game? game);
    Task SaveChangesAsync();
}