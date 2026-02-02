using exempleApiMessagerie.Models.Messages;
using exempleApiMessagerie.Models.Utilisateurs;

namespace exempleApiMessagerie.Models.Conversations;
public class Conversation {
    public long Id { get; set; }
    public List<Utilisateur> Utilisateurs { get; set; } = null!;
    public List<Message> Messages { get; set; } = null!;
}
