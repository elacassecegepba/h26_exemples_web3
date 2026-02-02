using exempleApiMessagerie.Models.Messages;
using exempleApiMessagerie.Models.Utilisateurs;

namespace exempleApiMessagerie.Models.Conversations;
public class ConversationDTO {
    public long Id { get; set; }

    public static ConversationDTO FromConversation(Conversation conversation) {
        return new ConversationDTO {
            Id = conversation.Id
        };
    }
}
