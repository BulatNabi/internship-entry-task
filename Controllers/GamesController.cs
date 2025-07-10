using Microsoft.AspNetCore.Mvc;
using TestTaskModulBank.DTOs;
using TestTaskModulBank.Interfaces;
using TestTaskModulBank.Models;
using System.Text.Json; 
using TestTaskModulBank.Enums; 

namespace TestTaskModulBank.Controllers;

[ApiController]
[Route("api/games")]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;
    private readonly JsonSerializerOptions _jsonOptions; 

    public GamesController(IGameService gameService, JsonSerializerOptions jsonOptions)
    {
        _gameService = gameService;
        _jsonOptions = jsonOptions; 
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<GameDto>> CreateGame([FromBody] CreateGameRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var game = await _gameService.CreateGameAsync(request.BoardSize);
        var gameDto = MapToGameDto(game);
        return CreatedAtAction(nameof(GetGame), new { id = gameDto.Id }, gameDto);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GameDto>> GetGame(Guid id)
    {
        try
        {
            var game = await _gameService.GetGameAsync(id);
            if (game == null)
            {
                return NotFound(new ProblemDetails { Title = "Game Not Found", Detail = $"Game with ID {id} not found." });
            }

            var gameDto = MapToGameDto(game);

            var etag = $"\"{game.LastUpdated.Ticks}\"";
            Response.Headers.ETag = etag;

            if (Request.Headers.IfNoneMatch.Contains(etag))
            {
                return StatusCode(StatusCodes.Status304NotModified);
            }

            return Ok(gameDto);
        }
        catch (ApplicationException ex)
        {
            return NotFound(new ProblemDetails { Title = "Game Not Found", Detail = ex.Message });
        }
    }

    [HttpPost("{id}/move")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GameDto>> MakeMove(Guid id, [FromBody] MakeMoveRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (id != request.GameId)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid Request", Detail = "Game ID in URL and body do not match." });
        }

        try
        {
            var gameBeforeMove = await _gameService.GetGameAsync(id);
            if (gameBeforeMove == null)
            {
                return NotFound(new ProblemDetails { Title = "Game Not Found", Detail = $"Game with ID {id} not found." });
            }

            var boardBeforeMove = JsonSerializer.Deserialize<PlayerSymbol[,]>(gameBeforeMove.BoardJson, _jsonOptions);

            if (boardBeforeMove[request.Row, request.Column] == request.PlayerSymbol &&
                gameBeforeMove.CurrentTurn != request.PlayerSymbol)
            {
                var existingEtag = $"\"{gameBeforeMove.LastUpdated.Ticks}\"";
                Response.Headers.ETag = existingEtag;
                return Ok(MapToGameDto(gameBeforeMove));
            }

            var (updatedGame, opponentSymbolPlaced) = await _gameService.MakeMoveAsync(id, request.Row, request.Column, request.PlayerSymbol);
            var gameDto = MapToGameDto(updatedGame);

            var etag = $"\"{updatedGame.LastUpdated.Ticks}\"";
            Response.Headers.ETag = etag;

            return Ok(gameDto);
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Type = "https://example.com/probs/move-validation-error",
                Title = "Move Validation Error",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Type = "https://example.com/probs/invalid-coordinates",
                Title = "Invalid Coordinates",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest,
                Instance = HttpContext.Request.Path
            });
        }
        catch (Exception ex) when (ex.Message.Contains("Game with ID") && ex.Message.Contains("not found"))
        {
             return NotFound(new ProblemDetails { Title = "Game Not Found", Detail = ex.Message });
        }
    }

    private GameDto MapToGameDto(Game? game)
    {
        if (game == null) return null;

        PlayerSymbol[,] board = null;
        if (!string.IsNullOrEmpty(game.BoardJson))
        {
            board = JsonSerializer.Deserialize<PlayerSymbol[,]>(game.BoardJson, _jsonOptions);
        }
        else
        {
            board = new PlayerSymbol[game.BoardSize, game.BoardSize];
        }

        return new GameDto
        {
            Id = game.Id,
            BoardSize = game.BoardSize,
            Board = board,
            CurrentTurn = game.CurrentTurn,
            Status = game.Status,
            Winner = game.Winner,
            MovesCount = game.MovesCount
        };
    }
}