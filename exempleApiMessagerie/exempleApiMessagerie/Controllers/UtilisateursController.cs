using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using exempleApiMessagerie.Models;
using exempleApiMessagerie.Models.Utilisateurs;
using Microsoft.Data.Sqlite;

namespace exempleApiMessagerie.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UtilisateursController : ControllerBase {
    private readonly AppDbContext _context;

    public UtilisateursController(AppDbContext context) {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Utilisateur>>> GetUtilisateurs() {
        return await _context.Utilisateurs.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Utilisateur>> GetUtilisateur(long id) {
        var utilisateur = await _context.Utilisateurs.FindAsync(id);

        if (utilisateur == null) {
            return NotFound();
        }

        return utilisateur;
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> PutUtilisateur(long id, Utilisateur utilisateur) {
        if (id != utilisateur.Id) {
            return BadRequest();
        }

        _context.Entry(utilisateur).State = EntityState.Modified;

        try {
            await _context.SaveChangesAsync();
        } catch (DbUpdateConcurrencyException) {
            if (!UtilisateurExists(id)) {
                return NotFound();
            } else {
                throw;
            }
        }

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult<Utilisateur>> PostUtilisateur(Utilisateur utilisateur) {
        _context.Utilisateurs.Add(utilisateur);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetUtilisateur", new { id = utilisateur.Id }, utilisateur);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUtilisateur(long id) {
        var utilisateur = await _context.Utilisateurs.FindAsync(id);
        if (utilisateur == null) {
            return NotFound();
        }

        _context.Utilisateurs.Remove(utilisateur);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool UtilisateurExists(long id) {
        return _context.Utilisateurs.Any(e => e.Id == id);
    }
}
