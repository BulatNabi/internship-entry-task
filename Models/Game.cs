using System;
using System.Text.Json;
using TestTaskModulBank.Enums;

namespace TestTaskModulBank.Models
{
    public class Game
    {
        public Guid Id { get; set; }
        public int BoardSize { get; set; }
        public string BoardJson { get; set; }
        public PlayerSymbol CurrentTurn { get; set; }
        public GameStatus Status { get; set; }
        public PlayerSymbol? Winner { get; set; }
        public int MovesCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdated { get; set; }

        public Game(int boardSize, JsonSerializerOptions jsonOptions)
        {
            Id = Guid.NewGuid();
            BoardSize = boardSize;
            BoardJson = JsonSerializer.Serialize(new PlayerSymbol[boardSize, boardSize], jsonOptions);
            CurrentTurn = PlayerSymbol.X;
            Status = GameStatus.InProgress;
            MovesCount = 0;
            CreatedAt = DateTime.UtcNow;
            LastUpdated = DateTime.UtcNow;
        }

        public Game() { }
    }
}