using System.ComponentModel.DataAnnotations;

namespace TestTaskModulBank.DTOs;

public class CreateGameRequestDto
{
    [Range(3, 10, ErrorMessage = "Board size must be between 3 and 10.")]
    public int BoardSize { get; set; } = 3;
}