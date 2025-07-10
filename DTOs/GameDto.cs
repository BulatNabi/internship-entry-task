using TestTaskModulBank.Enums;

namespace TestTaskModulBank.DTOs;

public class GameDto
{
    public Guid Id { get; set; }
    public int BoardSize { get; set; }
    public PlayerSymbol[,] Board { get; set; }
    public PlayerSymbol CurrentTurn { get; set; }
    public GameStatus Status { get; set; }
    public PlayerSymbol? Winner { get; set; }
    public int MovesCount { get; set; }
}