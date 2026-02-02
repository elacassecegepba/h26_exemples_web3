using exempleApiMessagerie.Models;
using exempleApiMessagerie.Models.Conversations;
using exempleApiMessagerie.Models.Messages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;

namespace exempleApiMessagerie.Controllers;

[Route("api/Conversations/{id}/Messages")]
[ApiController]
public class ConversationMessagesController : ControllerBase {
    private readonly AppDbContext _context;

    public ConversationMessagesController(AppDbContext context) {
        _context = context;
    }

    /// <summary>
    /// Récupère les messages d'une conversation spécifique.
    /// </summary>
    /// <param name="id">L'ID de la conversation.</param>
    /// <returns>Une liste de messages.</returns>
    /// <response code="200">Retourne la liste des messages.</response>
    /// <response code="404">Si la conversation n'est pas trouvée.</response>
    [HttpGet]
    [ProducesResponseType<IEnumerable<MessageDTO>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessages(long id) {
        Conversation? conversation = await _context.Conversations
            .Include(c => c.Messages)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (conversation == null) {
            return NotFound();
        }

        return conversation.Messages.Select(m => MessageDTO.FromMessage(m)).ToList();
    }

    /// <summary>
    /// Ajoute un message à une conversation spécifique.
    /// </summary>
    /// <param name="id">L'ID de la conversation.</param>
    /// <param name="messageDTO">Les informations du message à ajouter.</param>
    /// <returns>Le message ajouté.</returns>
    /// <response code="201">Le message a été ajouté avec succès.</response>
    /// <response code="400">Si les données sont invalides.</response>
    /// <response code="403">Si l'utilisateur n'a pas la permission d'ajouter un message.</response>
    /// <response code="404">Si la conversation n'est pas trouvée.</response>
    [HttpPost]
    [ProducesResponseType<MessageDTO>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult<MessageDTO>> PostMessage(long id, MessageInsertDTO messageDTO) {
        Conversation? conversation = await _context.Conversations
            .Include(c => c.Messages)
            .Include(c => c.Utilisateurs)
            .Where(c => c.Id == id)
            .FirstOrDefaultAsync();

        if (conversation == null) {
            return NotFound(new ProblemDetails {
                Title = "Conversation non trouvée",
                Detail = $"Aucune conversation trouvée avec l'ID {id}.",
                Status = StatusCodes.Status404NotFound
            });
        }

        if (!conversation.Utilisateurs.Any(u => u.Id == messageDTO.EnvoyeurId)) {
            // Pour le moment Forbid() résulte en une erreur 500, car on n'a pas encore implémenté l'authentification.
            // Donc on retourne un BadRequest avec un message d'erreur clair à la place.
            return BadRequest(new ProblemDetails {
                Title = "Utilisateur non dans la conversation",
                Detail = $"L'utilisateur avec l'ID {messageDTO.EnvoyeurId} n'est pas un participant de la conversation {id}.",
                Status = StatusCodes.Status400BadRequest
            });
            // return Forbid(); // L'utilisateur n'a pas la permission d'ajouter un message
        }

        Message message = Message.FromDTO(messageDTO, id);

        conversation.Messages.Add(message);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetMessages),
            new { id = id },
            MessageDTO.FromMessage(message)
        );
    }
}