using exempleApiMessagerie.Models;
using exempleApiMessagerie.Models.Messages;
using exempleApiMessagerie.Models.Utilisateurs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;

namespace exempleApiMessagerie.Controllers;

[Route("api/Utilisateurs/{utilisateurId}/Messages")]
[ApiController]
public class MessagesController : ControllerBase {
    private readonly AppDbContext _context;

    public MessagesController(AppDbContext context) {
        _context = context;
    }

    /// <summary>
    /// Récupère tous les messages reçus de l'utilisateur spécifié.
    /// </summary>
    /// <param name="utilisateurId">L'identifiant de l'utilisateur.</param>
    /// <returns>Une liste d'utilisateurs.</returns>
    /// <response code="200">Retourne la liste des utilisateurs.</response>
    /// <response code="404">L'utilisateur spécifié n'a pas été trouvé.</response>
    [HttpGet]
    [ProducesResponseType<IEnumerable<MessageDTO>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessages(long utilisateurId) {
        // Récupération des messages directement à partir de la table Messages
        if (!UtilisateurExiste(utilisateurId)) {
            return NotFound(CreerProblemDetailsUtilisateurNonTrouve(utilisateurId));
        }

        return await _context.Messages
                        .Where(m => m.ReceveurId == utilisateurId)
                        .Select(m => MessageDTO.FromMessage(m))
                        .ToListAsync();

        // Récupération des messages à partir de la propriété MessagesRecus de l'utilisateur
        Utilisateur? utilisateur = await _context.Utilisateurs
            .Include(u => u.MessagesRecus)
            .Where(u => u.Id == utilisateurId)
            .FirstOrDefaultAsync();

        if (utilisateur is null) {
            return NotFound(CreerProblemDetailsUtilisateurNonTrouve(utilisateurId));
        }

        return utilisateur.MessagesRecus
            .Select(m => MessageDTO.FromMessage(m))
            .ToList();
    }

    /// <summary>
    /// Envoie un message à l'utilisateur spécifié
    /// </summary>
    /// <param name="utilisateurId">L'identifiant de l'utilisateur à qui envoyer le message.</param>
    /// <param name="messageDTO">Le message à créer.</param>
    /// <returns>Le message créé.</returns>
    /// <response code="201">Le message a été créé avec succès.</response>
    /// <response code="400">Les données fournies sont invalides.</response>
    /// <response code="404">L'utilisateur spécifié n'a pas été trouvé.</response>
    [HttpPost]
    [ProducesResponseType<MessageDTO>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult<MessageDTO>> PostMessage(long utilisateurId, MessageInsertDTO messageDTO) {
        if (!UtilisateurExiste(utilisateurId)) {
            return NotFound(CreerProblemDetailsUtilisateurNonTrouve(utilisateurId));
        }
        if (!UtilisateurExiste(messageDTO.EnvoyeurId)) {
            return NotFound(CreerProblemDetailsUtilisateurNonTrouve(messageDTO.EnvoyeurId));
        }

        Message message = Message.FromDTO(messageDTO, utilisateurId);
        _context.Messages.Add(message);

        await _context.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetMessages),
            new { utilisateurId = utilisateurId },
            MessageDTO.FromMessage(message)
        );
    }

    private bool UtilisateurExiste(long id) {
        return _context.Utilisateurs.Any(e => e.Id == id);
    }

    private static ProblemDetails CreerProblemDetailsUtilisateurNonTrouve(long utilisateurId) {
        return new ProblemDetails {
            Title = "Utilisateur non trouvé",
            Detail = $"L'utilisateur avec l'identifiant {utilisateurId} n'existe pas.",
            Status = StatusCodes.Status404NotFound
        };
    }
}