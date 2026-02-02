using exempleApiMessagerie.Models.Conversations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
    /// Clé étrangère vers la conversation qui reçoit le message.
    /// </summary>
    /// <example>1</example>
    public required long ConversationId { get; set; }

    /// <summary>
    /// Propriété de navigation vers la conversation qui reçoit le message.
    /// </summary>
    /// <remarks>Il faut utiliser <a href="https://learn.microsoft.com/fr-ca/ef/core/querying/related-data/eager">Include</a> pour que cette propriété soit chargée.</remarks>
    [ForeignKey(nameof(ConversationId))]
    public Conversation Conversation { get; set; } = null!;

    public static Message FromDTO(MessageInsertDTO dto, long conversationId) {
        return new Message {
            Texte = dto.Texte,
            ConversationId = conversationId
        };
    }
}