using exempleApiMessagerie.Models;
using exempleApiMessagerie.Models.Utilisateurs;
using exempleApiMessagerie.SQLite;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Threading.Tasks;

namespace exempleApiMessagerie.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UtilisateursController : ControllerBase {
    private readonly AppDbContext _context;

    public UtilisateursController(AppDbContext context) {
        _context = context;
    }

    /// <summary>
    /// Récupère tous les utilisateurs.
    /// </summary>
    /// <returns>Une liste d'utilisateurs.</returns>
    /// <response code="200">Retourne la liste des utilisateurs.</response>
    /// <response code="401">Si l'utilisateur n'est pas authentifié.</response>
    /// <response code="403">Si l'utilisateur n'a pas les droits d'accès.</response>
    [HttpGet]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType<IEnumerable<UtilisateurDTO>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden, MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult<IEnumerable<UtilisateurDTO>>> GetUtilisateurs() {
        return await _context.Utilisateurs
            .Select(utilisateur => UtilisateurDTO.FromUtilisateur(utilisateur))
            .ToListAsync();
    }

    /// <summary>
    /// Récupère un utilisateur par son ID.
    /// </summary>
    /// <param name="id">L'ID de l'utilisateur à récupérer.</param>
    /// <returns>L'utilisateur.</returns>
    /// <response code="200">Retourne l'utilisateur.</response>
    /// <response code="401">Si l'utilisateur n'est pas authentifié.</response>
    /// <response code="404">Si l'utilisateur n'est pas trouvé.</response>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType<UtilisateurDTO>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult<UtilisateurDTO>> GetUtilisateur(long id) {
        Utilisateur? utilisateur = await _context.Utilisateurs.FindAsync(id);

        if (utilisateur == null) {
            return NotFound();
        }

        return UtilisateurDTO.FromUtilisateur(utilisateur);
    }

    /// <summary>
    /// Met à jour un utilisateur existant.
    /// </summary>
    /// <param name="utilisateurDTO">L'objet utilisateur avec les nouvelles données.</param>
    /// <returns>Statut de la mise à jour.</returns>
    /// <response code="204">L'utilisateur a été mis à jour avec succès.</response>
    /// <response code="400">Les données fournies sont invalides.</response>
    /// <response code="401">Si l'utilisateur n'est pas authentifié.</response>
    /// <response code="409">Conflit en cas de violation de contrainte unique.</response>
    [HttpPut]
    [Authorize]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> PutUtilisateur(UtilisateurUpsertDTO utilisateurDTO) {
        long utilisateurId = long.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        Utilisateur utilisateurExistant = await _context.Utilisateurs.SingleAsync(u => u.Id == utilisateurId);
        utilisateurExistant.UpdateFromDTO(utilisateurDTO);
        _context.Entry(utilisateurExistant).State = EntityState.Modified;

        try {
            await _context.SaveChangesAsync();
        } catch (DbUpdateException e) when (SQLiteHelper.EstErreurContrainteUnique(e)) {
            return Conflict(new ProblemDetails {
                Detail = "Email ou nom d'utilisateur déjà utilisé."
            });
        }

        return NoContent();
    }

    /// <summary>
    /// Supprime l'utilisateur authentifié.
    /// Attention, on ne gère pas la révocation du token JWT après la suppression de l'utilisateur, il restera valide jusqu'à son expiration, ce qui peut poser des problèmes.
    /// Dans une application réelle, il faudrait implémenter un mécanisme de révocation de token pour éviter ça.
    /// </summary>
    /// <returns>Statut de la suppression.</returns>
    /// <response code="204">L'utilisateur a été supprimé avec succès.</response>
    /// <response code="401">Si l'utilisateur n'est pas authentifié.</response>
    [HttpDelete]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> DeleteUtilisateur() {
        long utilisateurId = long.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        Utilisateur utilisateur = await _context.Utilisateurs.SingleAsync(u => u.Id == utilisateurId);

        _context.Utilisateurs.Remove(utilisateur);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
