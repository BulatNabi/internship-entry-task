using Xunit;
using System.Text.Json;
using System.Text.Json.Serialization;
using TestTaskModulBank.Converters;
using TestTaskModulBank.Enums; 

namespace TestTaskModulBank.Tests
{
    public class PlayerSymbolArrayConverterTests
    {
        private readonly JsonSerializerOptions _options;

        public PlayerSymbolArrayConverterTests()
        {
            _options = new JsonSerializerOptions();
            _options.Converters.Add(new PlayerSymbolArrayConverter());
            _options.Converters.Add(new JsonStringEnumConverter()); 
        }

        [Fact]
        public void CanSerialize_EmptyBoard()
        {
            PlayerSymbol[,] board = new PlayerSymbol[0, 0];
            string expectedJson = "[]";

            string actualJson = JsonSerializer.Serialize(board, _options);

            Assert.Equal(expectedJson, actualJson);
        }

        [Fact]
        public void CanDeserialize_EmptyBoard()
        {
            string json = "[]";
            PlayerSymbol[,] expectedBoard = new PlayerSymbol[0, 0];

            PlayerSymbol[,] actualBoard = JsonSerializer.Deserialize<PlayerSymbol[,]>(json, _options);

            Assert.NotNull(actualBoard);
            Assert.Equal(expectedBoard.GetLength(0), actualBoard.GetLength(0));
            Assert.Equal(expectedBoard.GetLength(1), actualBoard.GetLength(1));
        }

        [Fact]
        public void CanSerialize_2x2Board()
        {
            PlayerSymbol[,] board = new PlayerSymbol[2, 2]
            {
                { PlayerSymbol.X, PlayerSymbol.None },
                { PlayerSymbol.None, PlayerSymbol.O }
            };
            string expectedJson = "[[1,0],[0,2]]";

            string actualJson = JsonSerializer.Serialize(board, _options);

            Assert.Equal(expectedJson, actualJson);
        }

        [Fact]
        public void CanDeserialize_2x2Board()
        {
            string json = "[[1,0],[0,2]]";
            PlayerSymbol[,] expectedBoard = new PlayerSymbol[2, 2]
            {
                { PlayerSymbol.X, PlayerSymbol.None },
                { PlayerSymbol.None, PlayerSymbol.O }
            };

            PlayerSymbol[,] actualBoard = JsonSerializer.Deserialize<PlayerSymbol[,]>(json, _options);

            Assert.NotNull(actualBoard);
            Assert.Equal(expectedBoard.GetLength(0), actualBoard.GetLength(0));
            Assert.Equal(expectedBoard.GetLength(1), actualBoard.GetLength(1));
            Assert.Equal(expectedBoard[0, 0], actualBoard[0, 0]);
            Assert.Equal(expectedBoard[0, 1], actualBoard[0, 1]);
            Assert.Equal(expectedBoard[1, 0], actualBoard[1, 0]);
            Assert.Equal(expectedBoard[1, 1], actualBoard[1, 1]);
        }

        [Fact]
        public void CanDeserialize_3x3Board_WithEnumStrings()
        {
            string json = "[[\"X\",\"None\",\"O\"],[\"None\",\"X\",\"None\"],[\"O\",\"None\",\"X\"]]";
            PlayerSymbol[,] expectedBoard = new PlayerSymbol[3, 3]
            {
                { PlayerSymbol.X, PlayerSymbol.None, PlayerSymbol.O },
                { PlayerSymbol.None, PlayerSymbol.X, PlayerSymbol.None },
                { PlayerSymbol.O, PlayerSymbol.None, PlayerSymbol.X }
            };

            PlayerSymbol[,] actualBoard = JsonSerializer.Deserialize<PlayerSymbol[,]>(json, _options);

            Assert.NotNull(actualBoard);
            Assert.Equal(3, actualBoard.GetLength(0));
            Assert.Equal(3, actualBoard.GetLength(1));
            Assert.Equal(expectedBoard[0,0], actualBoard[0,0]);
        }
        
        [Fact]
        public void ThrowsException_OnInvalidJson_NotStartArray()
        {
            string json = "{}";

            var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<PlayerSymbol[,]>(json, _options));
            Assert.Contains("Expected StartArray token.", ex.Message);
        }

        [Fact]
        public void ThrowsException_OnInvalidJson_JaggedArray()
        {
            string json = "[[1,0],[0]]";

            var ex = Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<PlayerSymbol[,]>(json, _options));
            Assert.Contains("Jagged array is not supported for 2D array deserialization.", ex.Message);
        }
    }
}