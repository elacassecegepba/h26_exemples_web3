using exempleApiMessagerie.Models;
using exempleApiMessagerie.SQLite;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace exempleApiMessagerie.Controllers;

#if DEBUG
/// <summary>
/// Contrôleur de fonctionnalités de développement. Uniquement accessible en mode DEBUG.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class DevController : ControllerBase {
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public DevController(AppDbContext context, IConfiguration configuration) {
        _context = context;
        _configuration = configuration;
    }


    /// <summary>
    /// Endpoint de développement pour réinitialiser la base de données.
    /// </summary>
    /// <remarks>
    /// **Attention :** Ce endpoint supprime la base de données existante, applique les migrations pour recréer la structure de la base de données, puis seed la base de données avec des données de test.
    /// </remarks>
    /// <response code="204">La base de données a été réinitialisée avec succès.</response>
    [HttpPost("ReinitialiserBaseDeDonnees")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> ReinitialiserBaseDeDonnees() {
        await _context.Database.EnsureDeletedAsync(); // Supprime la base de données existante
        await _context.Database.MigrateAsync(); // Applique les migrations pour recréer la base de données avec la structure définie dans les migrations
        await SQLiteHelper.SeedDatabaseDev(_context); // Seed la base de données avec des données de test

        return NoContent();
    }
}
#endif