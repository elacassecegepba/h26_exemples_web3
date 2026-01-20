using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace exempleApiMessagerie.Models.Utilisateurs;

/// <summary>
/// Représente un utilisateur.
/// Cette classe est utilisée par Entity Framework pour mapper les utilisateurs dans la base de données.
/// </summary>
[Index(nameof(Nom), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class Utilisateur {
    public required long Id { get; set; }
    public required string Nom { get; set; }
    public required string Email { get; set; }
    public required string MotDePasse { get; set; }
}
