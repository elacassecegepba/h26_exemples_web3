using System.ComponentModel.DataAnnotations;

namespace exempleApiMessagerie.Models.Messages;

/// <summary>
/// Cette classe représente les données nécessaires pour créer un message.
/// </summary>
public class MessageInsertDTO {
    /// <summary>
    /// Clé étrangère vers l'utilisateur qui a envoyé le message.
    /// ! À remplacer par authentification
    /// </summary>
    /// <example>1</example>
    public required long EnvoyeurId { get; set; }

    /// <summary>
    /// Le texte du message.
    /// </summary>
    /// <example>Bonjour, comment ça va?</example>
    [MinLength(1)]
    [MaxLength(65535)]
    public required string Texte { get; set; }
}