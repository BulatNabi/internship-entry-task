using System.ComponentModel.DataAnnotations;
using TestTaskModulBank.Enums;

namespace TestTaskModulBank.DTOs;

public class MakeMoveRequestDto
{
    [Required]
    public Guid GameId { get; set; }
    [Range(0, 9, ErrorMessage = "Row must be a non-negative integer.")] 
    public int Row { get; set; }
    [Range(0, 9, ErrorMessage = "Column must be a non-negative integer.")] 
    public int Column { get; set; }
    public PlayerSymbol PlayerSymbol { get; set; }
}