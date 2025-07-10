using TestTaskModulBank.Enums;
using TestTaskModulBank.Models;

namespace TestTaskModulBank.Interfaces;

public interface IGameService
{
    Task<Game?> CreateGameAsync(int boardSize);
    Task<Game?> GetGameAsync(Guid gameId);
    Task<(Game?, bool)> MakeMoveAsync(Guid gameId, int row, int col, PlayerSymbol playerSymbol);
}