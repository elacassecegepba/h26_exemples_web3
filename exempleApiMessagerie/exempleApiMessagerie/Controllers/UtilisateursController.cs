using exempleApiMessagerie.Models;
using exempleApiMessagerie.Models.Utilisateurs;
using exempleApiMessagerie.SQLite;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
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
    [HttpGet]
    [ProducesResponseType<IEnumerable<UtilisateurDTO>>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
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
    /// <response code="404">Si l'utilisateur n'est pas trouvé.</response>
    [HttpGet("{id}")]
    [ProducesResponseType<UtilisateurDTO>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
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
    /// <param name="id">L'ID de l'utilisateur à mettre à jour.</param>
    /// <param name="utilisateurDTO">L'objet utilisateur avec les nouvelles données.</param>
    /// <returns>Statut de la mise à jour.</returns>
    /// <response code="204">L'utilisateur a été mis à jour avec succès.</response>
    /// <response code="400">Les données fournies sont invalides.</response>
    /// <response code="404">Si l'utilisateur n'est pas trouvé.</response>
    /// <response code="409">Conflit en cas de violation de contrainte unique.</response>
    [HttpPut("{id}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> PutUtilisateur(long id, UtilisateurUpsertDTO utilisateurDTO) {
        Utilisateur? utilisateurExistant = await _context.Utilisateurs.FindAsync(id);

        if (utilisateurExistant == null) {
            return NotFound();
        }
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
    /// Crée un nouvel utilisateur.
    /// </summary>
    /// <param name="utilisateurDTO">L'utilisateur à créer.</param>
    /// <returns>Le nouvel utilisateur créé.</returns>
    /// <response code="201">Le nouvel utilisateur a été créé avec succès.</response>
    /// <response code="400">Les données fournies sont invalides.</response>
    /// <response code="409">Conflit en cas de violation de contrainte unique.</response>
    [HttpPost]
    [ProducesResponseType<UtilisateurDTO>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult<UtilisateurDTO>> PostUtilisateur(UtilisateurUpsertDTO utilisateurDTO) {
        Utilisateur nouvelUtilisateur = Utilisateur.FromDTO(utilisateurDTO);
        _context.Utilisateurs.Add(nouvelUtilisateur);
        try {
            await _context.SaveChangesAsync();
        } catch (DbUpdateException e) when (SQLiteHelper.EstErreurContrainteUnique(e)) {
            return Conflict(new ProblemDetails {
                Detail = "Email ou nom d'utilisateur déjà utilisé."
            });
        }

        return CreatedAtAction(
            nameof(GetUtilisateur),
            new { id = nouvelUtilisateur.Id },
            UtilisateurDTO.FromUtilisateur(nouvelUtilisateur)
        );
    }

    /// <summary>
    /// Supprime un utilisateur existant.
    /// </summary>
    /// <param name="id">L'ID de l'utilisateur à supprimer.</param>
    /// <returns>Statut de la suppression.</returns>
    /// <response code="204">L'utilisateur a été supprimé avec succès.</response>
    /// <response code="404">Si l'utilisateur n'est pas trouvé.</response>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, MediaTypeNames.Application.ProblemJson)]
    public async Task<IActionResult> DeleteUtilisateur(long id) {
        var utilisateur = await _context.Utilisateurs.FindAsync(id);
        if (utilisateur == null) {
            return NotFound();
        }

        _context.Utilisateurs.Remove(utilisateur);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
