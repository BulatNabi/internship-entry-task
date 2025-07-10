using System.Text.Json;
using System.Text.Json.Serialization;
using TestTaskModulBank.Enums; 

namespace TestTaskModulBank.Converters; 

public class PlayerSymbolArrayConverter : JsonConverter<PlayerSymbol[,]>
{
    public override PlayerSymbol[,] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException("Expected StartArray token.");
        }

        var boardList = new List<List<PlayerSymbol>>();
        reader.Read(); 

        while (reader.TokenType != JsonTokenType.EndArray)
        {
            if (reader.TokenType != JsonTokenType.StartArray)
            {
                throw new JsonException("Expected nested StartArray token.");
            }

            var rowList = new List<PlayerSymbol>();
            reader.Read(); 

            while (reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.Number)
                {
                    rowList.Add((PlayerSymbol)reader.GetInt32());
                }
                else if (reader.TokenType == JsonTokenType.String)
                {
                    var enumString = reader.GetString();
                    if (Enum.TryParse(typeof(PlayerSymbol), enumString, true, out var result))
                    {
                        rowList.Add((PlayerSymbol)result);
                    }
                    else
                    {
                        throw new JsonException($"Unable to parse '{enumString}' as PlayerSymbol.");
                    }
                }
                else
                {
                    throw new JsonException("Expected number or string for PlayerSymbol.");
                }
                reader.Read();
            }
            boardList.Add(rowList);
            reader.Read(); 
        }

        if (boardList.Count == 0)
        {
            return new PlayerSymbol[0, 0]; 
        }

        int rows = boardList.Count;
        int cols = boardList[0].Count; 
        var boardArray = new PlayerSymbol[rows, cols];

        for (int r = 0; r < rows; r++)
        {
            if (boardList[r].Count != cols)
            {
                throw new JsonException("Jagged array is not supported for 2D array deserialization.");
            }
            for (int c = 0; c < cols; c++)
            {
                boardArray[r, c] = boardList[r][c];
            }
        }
        return boardArray;
    }

    public override void Write(Utf8JsonWriter writer, PlayerSymbol[,] value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray(); 
        int rows = value.GetLength(0);
        int cols = value.GetLength(1);

        for (int r = 0; r < rows; r++)
        {
            writer.WriteStartArray();
            for (int c = 0; c < cols; c++)
            {
                writer.WriteNumberValue((int)value[r, c]);
            }
            writer.WriteEndArray();
        }
        writer.WriteEndArray(); 
    }
}