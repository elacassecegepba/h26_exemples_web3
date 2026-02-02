using exempleApiMessagerie.Models;
using exempleApiMessagerie.Models.Conversations;
using exempleApiMessagerie.Models.Utilisateurs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;

namespace exempleApiMessagerie.Controllers;

[Route("api/Conversations/{id}/Utilisateurs")]
[ApiController]
public class ConversationUtilisateursController : ControllerBase {
    private readonly AppDbContext _context;

    public ConversationUtilisateursController(AppDbContext context) {
        _context = context;
    }

    /// <summary>
    /// Récupère les utilisateurs d'une conversation spécifique.
    /// </summary>
    /// <param name="id">L'ID de la conversation.</param>
    /// <returns>La liste des utilisateurs de la conversation.</returns>
    /// <response code="200">La list des utilisateurs de la conversation.</response>
    /// <response code="404">Si la conversation n'est pas trouvé.</response>
    [HttpGet]
    [ProducesResponseType<ConversationDTO>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult<IEnumerable<ConversationUtilisateurDTO>>> GetUtilisateurs(long id) {
        Conversation? conversation = await _context.Conversations
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

        return conversation.Utilisateurs.Select(u => new ConversationUtilisateurDTO { UtilisateurId = u.Id }).ToList();
    }

    /// <summary>
    /// Ajoute un utilisateur à une conversation.
    /// </summary>
    /// <param name="id">L'ID de la conversation.</param>
    /// <param name="utilisateurDTO">Les informations de l'utilisateur à ajouter.</param>
    /// <returns>La conversation mise à jour.</returns>
    /// <response code="201">La conversation a été mise à jour avec succès.</response>
    /// <response code="404">Si la conversation ou l'utilisateur n'est pas trouvé.</response>
    [HttpPost]
    [ProducesResponseType<ConversationDTO>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult<ConversationDTO>> PostUtilisateur(long id, ConversationUtilisateurDTO utilisateurDTO) {
        Conversation? conversation = await _context.Conversations
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

        Utilisateur? utilisateur = await _context.Utilisateurs
            .Where(u => u.Id == utilisateurDTO.UtilisateurId)
            .FirstOrDefaultAsync();

        if (utilisateur == null) {
            return NotFound(new ProblemDetails {
                Title = "Utilisateur non trouvé",
                Detail = $"Aucun utilisateur trouvé avec l'ID {utilisateurDTO.UtilisateurId}.",
                Status = StatusCodes.Status404NotFound
            });
        }

        conversation.Utilisateurs.Add(utilisateur);
        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetUtilisateurs),
            new { id = id },
            ConversationDTO.FromConversation(conversation)
        );
    }
}