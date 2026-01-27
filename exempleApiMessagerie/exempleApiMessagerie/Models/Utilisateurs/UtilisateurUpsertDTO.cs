using System.ComponentModel.DataAnnotations;

namespace exempleApiMessagerie.Models.Utilisateurs;

/// <summary>
/// Cette classe représente les données nécessaires pour créer ou mettre à jour un utilisateur.
/// </summary>
public class UtilisateurUpsertDTO {
    /// <summary>
    /// Le nom de l'utilisateur.
    /// </summary>
    /// <example>Jean Dupont</example>
    [MinLength(3)]
    [MaxLength(255)]
    public required string Nom { get; set; }

    /// <summary>
    /// Le courriel de l'utilisateur.
    /// </summary>
    /// <example>jean.dupont@test.com</example>
    [EmailAddress]
    [MaxLength(255)]
    public required string Email { get; set; }

    /// <summary>
    /// Le mot de passe de l'utilisateur.
    /// </summary>
    /// <example>Password1!</example>
    [MinLength(3)]
    [MaxLength(255)]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).*$", ErrorMessage = "Au moins une lettre et un nombre")]
    public required string MotDePasse { get; set; }
}
