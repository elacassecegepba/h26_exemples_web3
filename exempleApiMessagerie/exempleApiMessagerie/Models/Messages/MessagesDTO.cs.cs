using System.ComponentModel.DataAnnotations;

namespace exempleApiMessagerie.Models.Messages;

/// <summary>
/// Représente un message.
/// Cette classe est utilisée pour transférer les données d'un message.
/// </summary>
public class MessageDTO {
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
    /// Clé étrangère vers l'utilisateur qui reçoit le message.
    /// </summary>
    /// <example>1</example>
    public required long ReceveurId { get; set; }

    public static MessageDTO FromMessage(Message message) {
        return new MessageDTO {
            Id = message.Id,
            Texte = message.Texte,
            UtilisateurId = message.EnvoyeurId,
            ReceveurId = message.ReceveurId
        };
    }
}