using exempleApiMessagerie.Models;
using exempleApiMessagerie.Models.Conversations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;

namespace exempleApiMessagerie.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ConversationsController : ControllerBase {
    private readonly AppDbContext _context;

    public ConversationsController(AppDbContext context) {
        _context = context;
    }

    /// <summary>
    /// Récupère toutes les conversations.
    /// </summary>
    /// <returns>Une liste de conversations.</returns>
    /// <response code="200">Retourne la liste des conversations.</response>
    [HttpGet]
    [ProducesResponseType<IEnumerable<ConversationDTO>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    public async Task<ActionResult<IEnumerable<ConversationDTO>>> GetConversations() {
        return await _context.Conversations
            .Select(conversation => ConversationDTO.FromConversation(conversation))
            .ToListAsync();
    }

    /// <summary>
    /// Crée une conversation.
    /// </summary>
    /// <returns>La conversation créée.</returns>
    /// <response code="201">La conversation a été créée avec succès.</response>
    [HttpPost]
    [ProducesResponseType<ConversationDTO>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)]
    public async Task<ActionResult<ConversationDTO>> PostConversation() { // Pas de paramètre nécessaire pour créer une conversation vide
        Conversation nouvelleConversation = new Conversation();
        _context.Conversations.Add(nouvelleConversation);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetConversations), // Référence à l'action pour récupérer toutes les conversations, car il n'y a pas d'action spécifique pour une seule conversation
            new {},
            ConversationDTO.FromConversation(nouvelleConversation)
        );
    }
}