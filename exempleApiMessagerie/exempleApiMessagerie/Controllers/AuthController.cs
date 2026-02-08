using exempleApiMessagerie.Models;
using exempleApiMessagerie.Models.Utilisateurs;
using exempleApiMessagerie.SQLite;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mime;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace exempleApiMessagerie.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase {
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, IConfiguration configuration) {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Crée un nouvel utilisateur.
    /// </summary>
    /// <param name="utilisateurDTO">L'utilisateur à créer.</param>
    /// <returns>Le nouvel utilisateur créé.</returns>
    /// <response code="200">Le nouvel utilisateur a été créé avec succès.</response>
    /// <response code="400">Les données fournies sont invalides.</response>
    /// <response code="409">Conflit en cas de violation de contrainte unique.</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult> Register(UtilisateurUpsertDTO utilisateurDTO) {
        Utilisateur nouvelUtilisateur = Utilisateur.FromDTO(utilisateurDTO);
        _context.Utilisateurs.Add(nouvelUtilisateur);
        try {
            await _context.SaveChangesAsync();
        } catch (DbUpdateException e) when (SQLiteHelper.EstErreurContrainteUnique(e)) {
            return Conflict(new ProblemDetails {
                Detail = "Email ou nom d'utilisateur déjà utilisé."
            });
        }

        return Ok();
    }


    [HttpPost("login")]
    public async Task<ActionResult<string>> Login(UtilisateurLoginDTO loginDTO) {
        Utilisateur? utilisateur = await _context.Utilisateurs
            .Where(u => u.Email == loginDTO.Email)
            .FirstOrDefaultAsync();

        if (utilisateur == null || !utilisateur.VerifierMotDePasse(loginDTO.MotDePasse)) {
            return Unauthorized();
        }

        return Ok(CreerToken(utilisateur));
    }

    private string CreerToken(Utilisateur utilisateur) {
        List<Claim> claims = new() {
            new Claim("UserId", utilisateur.Id.ToString())
        };

        // TokenPassKey doit être défini dans appsettings.json ou dans les variables d'environnement
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["TokenPassKey"]!) 
        );

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddHours(1),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
