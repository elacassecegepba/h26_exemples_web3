using exempleApiMessagerie.Models.Utilisateurs;

namespace exempleApiMessagerie.Models.Auth;

/// <summary>
/// Représente les données retournées lors du login d'un utilisateur.
/// </summary>
public class LoginReponseDTO {
    /// <summary>
    /// Le token JWT à utiliser pour les requêtes authentifiées.
    /// </summary>
    public required string AccessToken  { get; set; }

    /// <summary>
    /// Les informations de l'utilisateur connecté.
    /// </summary>
    public required UtilisateurDTO Utilisateur { get; set; }
}
