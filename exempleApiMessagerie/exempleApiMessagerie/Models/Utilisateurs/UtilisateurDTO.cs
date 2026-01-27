using System.ComponentModel.DataAnnotations;

namespace exempleApiMessagerie.Models.Utilisateurs;

/// <summary>
/// Représente un utilisateur.
/// Cette classe est utilisée pour transférer les données d'un utilisateur.
/// Notez que le mot de passe n'est pas inclus dans ce DTO pour des raisons de sécurité.
/// </summary>
public class UtilisateurDTO {
    /// <summary>
    /// Clé primaire de l'utilisateur.
    /// </summary>
    /// <example>1</example>
    public required long Id { get; set; }

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

    public static UtilisateurDTO FromUtilisateur(Utilisateur utilisateur) {
        return new UtilisateurDTO {
            Id = utilisateur.Id,
            Nom = utilisateur.Nom,
            Email = utilisateur.Email
        };
    }
}
