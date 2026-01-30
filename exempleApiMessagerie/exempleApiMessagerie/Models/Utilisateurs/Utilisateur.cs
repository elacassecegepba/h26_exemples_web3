using exempleApiMessagerie.Models.Messages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace exempleApiMessagerie.Models.Utilisateurs;

/// <summary>
/// Représente un utilisateur.
/// Cette classe est utilisée par Entity Framework pour mapper les utilisateurs dans la base de données.
/// </summary>
[Index(nameof(Nom), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class Utilisateur {
    /// <summary>
    /// Clé primaire de l'utilisateur.
    /// </summary>
    /// <example>1</example>
    public long Id { get; set; }

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

    /// <summary>
    /// Propriété de navigation vers les messages envoyés par l'utilisateur.
    /// </summary>
    /// <remarks>Il faut utiliser <a href="https://learn.microsoft.com/fr-ca/ef/core/querying/related-data/eager">Include</a> pour que cette propriété soit chargée.</remarks>
    [InverseProperty(nameof(Message.Envoyeur))]
    public List<Message> MessagesEnvoyes { get; set; } = null!;

    /// <summary>
    /// Propriété de navigation vers les messages reçu par l'utilisateur.
    /// </summary>
    /// <remarks>Il faut utiliser <a href="https://learn.microsoft.com/fr-ca/ef/core/querying/related-data/eager">Include</a> pour que cette propriété soit chargée.</remarks>
    [InverseProperty(nameof(Message.Receveur))]
    public List<Message> MessagesRecus { get; set; } = null!;

    public static Utilisateur FromDTO(UtilisateurUpsertDTO dto) {
        return new Utilisateur {
            Nom = dto.Nom,
            Email = dto.Email,
            MotDePasse = dto.MotDePasse
        };
    }

    public void UpdateFromDTO(UtilisateurUpsertDTO dto) {
        Nom = dto.Nom;
        Email = dto.Email;
        MotDePasse = dto.MotDePasse;
    }
}
