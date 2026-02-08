using exempleApiMessagerie.Models;
using exempleApiMessagerie.Models.Messages;
using exempleApiMessagerie.Models.Utilisateurs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;

namespace exempleApiMessagerie.Controllers;

[Route("api/Messages")]
[ApiController]
public class MessagesController : ControllerBase {
    private readonly AppDbContext _context;

    public MessagesController(AppDbContext context) {
        _context = context;
    }

    /// <summary>
    /// Récupère tous les messages reçus de l'utilisateur authentifié.
    /// </summary>
    /// <returns>Une liste de messages.</returns>
    /// <response code="200">Retourne la liste des messages.</response>
    /// <response code="401">Si l'utilisateur n'est pas authentifié.</response>
    [HttpGet]
    [Authorize] // Exige que l'utilisateur soit authentifié
    [ProducesResponseType<IEnumerable<MessageDTO>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult<IEnumerable<MessageDTO>>> GetMessages() {
        // Récupération de l'identifiant de l'utilisateur à partir du token JWT
        long utilisateurId = long.Parse(User.FindFirstValue("UserId")!);

        // Récupération des messages directement à partir de la table Messages
        return await _context.Messages
            .Where(m => m.ReceveurId == utilisateurId)
            .Select(m => MessageDTO.FromMessage(m))
            .ToListAsync();
    }

    /// <summary>
    /// Envoie un message à l'utilisateur spécifié
    /// </summary>
    /// <param name="utilisateurId">L'identifiant de l'utilisateur à qui envoyer le message.</param>
    /// <param name="messageDTO">Le message à créer.</param>
    /// <returns>Le message créé.</returns>
    /// <response code="201">Le message a été créé avec succès.</response>
    /// <response code="400">Les données fournies sont invalides.</response>
    /// <response code="401">Si l'utilisateur n'est pas authentifié.</response>
    /// <response code="404">L'utilisateur spécifié n'a pas été trouvé.</response>
    [HttpPost("/api/Utilisateurs/{utilisateurId}/Messages")] // Route personnalisée pour spécifier l'utilisateur destinataire dans l'URL
    [Authorize] // Exige que l'utilisateur soit authentifié
    [ProducesResponseType<MessageDTO>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult<MessageDTO>> PostMessage(long utilisateurId, MessageInsertDTO messageDTO) {
        if (!UtilisateurExiste(utilisateurId)) {
            return NotFound(CreerProblemDetailsUtilisateurNonTrouve(utilisateurId));
        }
        
        // Récupération de l'identifiant de l'utilisateur à partir du token JWT
        long envoyeurId = long.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);

        Message message = Message.FromDTO(messageDTO, utilisateurId, envoyeurId);
        _context.Messages.Add(message);

        await _context.SaveChangesAsync();

        // L'API n'a pas d'endpoint pour récupérer un message individuel, donc on retourne le message créé directement
        return StatusCode(StatusCodes.Status201Created, MessageDTO.FromMessage(message));
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