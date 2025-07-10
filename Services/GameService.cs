using System.Text.Json; 
using Microsoft.Extensions.Configuration;
using TestTaskModulBank.Enums;
using TestTaskModulBank.Interfaces;
using TestTaskModulBank.Models;

namespace TestTaskModulBank.Services;

public class GameService : IGameService
{
    private readonly IGameRepository _gameRepository;
    private readonly Random _random;
    private readonly int _winningCondition;
    private readonly JsonSerializerOptions _jsonOptions; 

    public GameService(IGameRepository gameRepository, IConfiguration configuration, JsonSerializerOptions jsonOptions) // Внедряем опции
    {
        _gameRepository = gameRepository;
        _random = new Random();
        _winningCondition = configuration.GetValue<int>("WINNING_CONDITION", 3);
        _jsonOptions = jsonOptions; 
    }

    public async Task<Game?> CreateGameAsync(int boardSize)
    {
        var game = new Game(boardSize, _jsonOptions);
        await _gameRepository.AddAsync(game);
        await _gameRepository.SaveChangesAsync();
        return game;
    }

    public async Task<Game?> GetGameAsync(Guid gameId)
    {
        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game == null)
        {
            throw new ApplicationException($"Game with ID {gameId} not found.");
        }
        return game;
    }

    public async Task<(Game?, bool)> MakeMoveAsync(Guid gameId, int row, int col, PlayerSymbol playerSymbol)
    {
        var game = await _gameRepository.GetByIdAsync(gameId);
        if (game == null)
        {
            throw new ApplicationException($"Game with ID {gameId} not found.");
        }

        if (game.Status != GameStatus.InProgress)
        {
            throw new ApplicationException("Game is already finished or drawn.");
        }

        if (game.CurrentTurn != playerSymbol)
        {
            throw new ApplicationException($"It's not {playerSymbol}'s turn.");
        }

        PlayerSymbol[,] boardData = JsonSerializer.Deserialize<PlayerSymbol[,]>(game.BoardJson, _jsonOptions); // Использование _jsonOptions

        if (row < 0 || row >= game.BoardSize || col < 0 || col >= game.BoardSize)
        {
            throw new ArgumentOutOfRangeException("Move is out of board boundaries.");
        }

        if (boardData[row, col] != PlayerSymbol.None)
        {
            throw new ApplicationException("Cell is already occupied.");
        }

        bool opponentSymbolPlaced = false;
        game.MovesCount++;

        if (game.MovesCount % 3 == 0 && _random.Next(1, 11) == 1)
        {
            boardData[row, col] = GetOpponentSymbol(playerSymbol);
            opponentSymbolPlaced = true;
        }
        else
        {
            boardData[row, col] = playerSymbol;
        }

        game.BoardJson = JsonSerializer.Serialize(boardData, _jsonOptions); 

        CheckGameStatus(game);

        if (game.Status == GameStatus.InProgress)
        {
            game.CurrentTurn = GetOpponentSymbol(game.CurrentTurn);
        }

        game.LastUpdated = DateTime.UtcNow;
        await _gameRepository.UpdateAsync(game);
        await _gameRepository.SaveChangesAsync();

        return (game, opponentSymbolPlaced);
    }

    private PlayerSymbol GetOpponentSymbol(PlayerSymbol currentSymbol)
    {
        return currentSymbol == PlayerSymbol.X ? PlayerSymbol.O : PlayerSymbol.X;
    }

    private void CheckGameStatus(Game game)
    {
        PlayerSymbol[,] boardData = JsonSerializer.Deserialize<PlayerSymbol[,]>(game.BoardJson, _jsonOptions); // Использование _jsonOptions

        if (CheckForWinInternal(game.BoardSize, boardData, PlayerSymbol.X))
        {
            game.Winner = PlayerSymbol.X;
            game.Status = GameStatus.Finished;
            return;
        }
        if (CheckForWinInternal(game.BoardSize, boardData, PlayerSymbol.O))
        {
            game.Winner = PlayerSymbol.O;
            game.Status = GameStatus.Finished;
            return;
        }

        bool isBoardFull = true;
        for (int r = 0; r < game.BoardSize; r++)
        {
            for (int c = 0; c < game.BoardSize; c++)
            {
                if (boardData[r, c] == PlayerSymbol.None)
                {
                    isBoardFull = false;
                    break;
                }
            }
            if (!isBoardFull) break;
        }

        if (isBoardFull)
        {
            game.Status = GameStatus.Draw;
            game.Winner = null;
        }
    }

    private bool CheckForWinInternal(int size, PlayerSymbol[,] board, PlayerSymbol symbol)
    {
        int winCount = _winningCondition;

        for (int r = 0; r < size; r++)
        {
            for (int c = 0; c <= size - winCount; c++)
            {
                bool win = true;
                for (int k = 0; k < winCount; k++)
                {
                    if (board[r, c + k] != symbol)
                    {
                        win = false;
                        break;
                    }
                }
                if (win) return true;
            }
        }

        for (int c = 0; c < size; c++)
        {
            for (int r = 0; r <= size - winCount; r++)
            {
                bool win = true;
                for (int k = 0; k < winCount; k++)
                {
                    if (board[r + k, c] != symbol)
                    {
                        win = false;
                        break;
                    }
                }
                if (win) return true;
            }
        }

        for (int r = 0; r <= size - winCount; r++)
        {
            for (int c = 0; c <= size - winCount; c++)
            {
                bool win = true;
                for (int k = 0; k < winCount; k++)
                {
                    if (board[r + k, c + k] != symbol)
                    {
                        win = false;
                        break;
                    }
                }
                if (win) return true;
            }
        }

        for (int r = 0; r <= size - winCount; r++)
        {
            for (int c = winCount - 1; c < size; c++)
            {
                bool win = true;
                for (int k = 0; k < winCount; k++)
                {
                    if (board[r + k, c - k] != symbol)
                    {
                        win = false;
                        break;
                    }
                }
                if (win) return true;
            }
        }

        return false;
    }
}