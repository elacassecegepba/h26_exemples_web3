using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace exempleApiMessagerie.SQLite;

public static class SQLiteHelper {
    public static bool EstErreurContrainteUnique(DbUpdateException ex) {
        return ex.InnerException is SqliteException sqliteEx &&
               sqliteEx.SqliteErrorCode == 19 &&
               sqliteEx.SqliteExtendedErrorCode == 2067;
    }
}