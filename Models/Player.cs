using TestTaskModulBank.Enums;

namespace TestTaskModulBank.Models;

public class Player
{ 
        public Guid Id { get; set; }
        public string Name { get; set; } 
        public PlayerSymbol Symbol { get; set; }
}