using exempleApiMessagerie.Models.Messages;
using Konscious.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

namespace exempleApiMessagerie.Models.Utilisateurs;

/// <summary>
/// Représente un utilisateur.
/// Utilisée par les endpoints d'authentification pour le login, elle contient le mot de passe en clair pour permettre la vérification du mot de passe lors du login.
/// </summary>
public class UtilisateurLoginDTO {
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
