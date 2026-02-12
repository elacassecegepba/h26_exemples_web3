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
    /// Méthode de seed pour la base de données en environnement de développement.
    /// </summary>
    /// <param name="context">Le contexte de la base de données.</param>
    public static Task SeedDatabaseDev(AppDbContext context) {
        context.Utilisateurs.Add(new() {
            Nom = "admin",
            Email = "admin@test.com",
            MotDePasse = Utilisateur.HashMotDePasse("Admin123!")
        });
        return context.SaveChangesAsync();
    }
}