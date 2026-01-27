using exempleApiMessagerie.Models.Utilisateurs;
using Microsoft.EntityFrameworkCore;

namespace exempleApiMessagerie.Models;

public class AppDbContext : DbContext {
    public const string DbPath = "SQLite/maBaseDeDonnees.db";

    // Déclaration des tables (DbSet) de la BD
    public DbSet<Utilisateur> Utilisateurs { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder options) {
        options.UseSqlite($"Data Source={DbPath}");
    }
}