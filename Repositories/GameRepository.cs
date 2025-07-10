using TestTaskModulBank.Data;
using TestTaskModulBank.Interfaces;
using TestTaskModulBank.Models;

namespace TestTaskModulBank.Repositories;

public class GameRepository : IGameRepository
{
    private readonly ApplicationDbContext _context;

    public GameRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Game?> GetByIdAsync(Guid id)
    {
        return await _context.Games.FindAsync(id);
    }

    public async Task AddAsync(Game? game)
    {
        await _context.Games.AddAsync(game);
    }

    public async Task UpdateAsync(Game? game)
    {
        _context.Games.Update(game);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}