using exempleApiMessagerie.Models.Utilisateurs;
using System.ComponentModel.DataAnnotations;

namespace exempleApiMessagerie.Models.Messages;

/// <summary>
/// Représente un message.
/// Cette classe est utilisée par Entity Framework pour mapper les messages dans la base de données.
/// </summary>
public class Message {
    /// <summary>
    /// Clé primaire.
    /// </summary>
    /// <example>1</example>
    public long Id { get; set; }

    /// <summary>
    /// Le texte du message.
    /// </summary>
    /// <example>Bonjour, comment ça va?</example>
    [MinLength(1)]
    [MaxLength(65535)]
    public required string Texte { get; set; }

    /// <summary>
    /// Clé étrangère vers l'utilisateur qui a envoyé le message.
    /// </summary>
    /// <example>1</example>
    public required long UtilisateurId { get; set; }

    /// <summary>
    /// Propriété de navigation vers l'utilisateur qui a envoyé le message.
    /// </summary>
    /// <remarks>Il faut utiliser <a href="https://learn.microsoft.com/fr-ca/ef/core/querying/related-data/eager">Include</a> pour que cette propriété soit chargée.</remarks>
    public Utilisateur Utilisateur { get; set; } = null!;

    public static Message FromDTO(MessageInsertDTO dto, long utilisateurId) {
        return new Message {
            Texte = dto.Texte,
            UtilisateurId = utilisateurId,
        };
    }
}