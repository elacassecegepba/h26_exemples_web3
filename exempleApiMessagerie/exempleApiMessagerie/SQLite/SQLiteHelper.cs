using exempleApiMessagerie.Models;
using exempleApiMessagerie.Models.Utilisateurs;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace exempleApiMessagerie.SQLite;

public static class SQLiteHelper {
    public static bool EstErreurContrainteUnique(DbUpdateException ex) {
        return ex.InnerException is SqliteException sqliteEx &&
            sqliteEx.SqliteErrorCode == 19 &&
            sqliteEx.SqliteExtendedErrorCode == 2067;
    }

    /// <summary>
    /// Ajoute un utilisateur admin par défaut à la base de données si aucun utilisateur n'existe déjà.
    /// </summary>
    /// <param name="context"></param>
    public static void SeedDatabase(AppDbContext context) {
        if (!context.Utilisateurs.Any()) {
            context.Utilisateurs.Add(new() {
                Nom = "admin",
                Email = "admin@test.com",
                MotDePasse = Utilisateur.HashMotDePasse("Admin123!")
            });
            context.SaveChanges();
        }
    }
}