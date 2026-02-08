using exempleApiMessagerie.Models;
using exempleApiMessagerie.Models.Auth;
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
    /// <response code="201">Le nouvel utilisateur a été créé avec succès.</response>
    /// <response code="400">Les données fournies sont invalides.</response>
    /// <response code="409">Conflit en cas de violation de contrainte unique.</response>
    [HttpPost("register")]
    [ProducesResponseType<UtilisateurDTO>(StatusCodes.Status201Created, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict, MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult<UtilisateurDTO>> Register(UtilisateurUpsertDTO utilisateurDTO) {
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
            nameof(UtilisateursController.GetUtilisateur),
            "Utilisateurs", // Lorsque l'action est dans un autre contrôleur, il faut spécifier le nom du contrôleur en deuxième argument
            new { id = nouvelUtilisateur.Id },
            UtilisateurDTO.FromUtilisateur(nouvelUtilisateur)
        );
    }

    /// <summary>
    /// Authentifie un utilisateur et retourne un token JWT s'il est valide.
    /// </summary>
    /// <param name="loginDTO">Les identifiants de connexion de l'utilisateur.</param>
    /// <returns>Le token JWT si l'authentification est réussie.</returns>
    /// <response code="200">Le token JWT a été généré avec succès.</response>
    /// <response code="400">Les données fournies sont invalides.</response>
    /// <response code="401">Les identifiants de connexion sont invalides.</response>
    [HttpPost("login")]
    [ProducesResponseType<LoginReponseDTO>(StatusCodes.Status200OK, MediaTypeNames.Application.Json)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest, MediaTypeNames.Application.ProblemJson)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized, MediaTypeNames.Application.ProblemJson)]
    public async Task<ActionResult<LoginReponseDTO>> Login(UtilisateurLoginDTO loginDTO) {
        Utilisateur? utilisateur = await _context.Utilisateurs
            .Where(u => u.Email == loginDTO.Email)
            .FirstOrDefaultAsync();

        if (utilisateur == null || !utilisateur.VerifierMotDePasse(loginDTO.MotDePasse)) {
            return Unauthorized();
        }

        LoginReponseDTO loginReponse = new() {
            AccessToken = CreerToken(utilisateur),
            Utilisateur = UtilisateurDTO.FromUtilisateur(utilisateur)
        };


        return Ok(loginReponse);
    }

    private string CreerToken(Utilisateur utilisateur) {
        List<Claim> claims = new() {
            new Claim(JwtRegisteredClaimNames.Sub, utilisateur.Id.ToString()), // Sujet du token (ID de l'utilisateur)
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // ID unique pour le token
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64), // Date d'émission du token
            new Claim("UserId", utilisateur.Id.ToString()), // Claim personnalisé pour l'ID de l'utilisateur
            new Claim(JwtRegisteredClaimNames.Email, utilisateur.Email), // Email de l'utilisateur
            new Claim(JwtRegisteredClaimNames.Name, utilisateur.Nom) // Nom d'utilisateur
        };

        if (utilisateur.Nom == "admin")
        {
            // Exemple de claim de rôle, à adapter selon les besoins
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        // Les JwtSettings doivent être configurés dans appsettings.json
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["JwtSettings:TokenPassKey"]!)
        );

        var token = new JwtSecurityToken(
            issuer: _configuration["JwtSettings:Issuer"], // Émetteur du token (nom ou URL du serveur)
            audience: _configuration["JwtSettings:Audience"], // Destinataire du token (nom ou URL du client)
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha512)
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
