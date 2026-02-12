using exempleApiMessagerie.Models.Messages;
using Konscious.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;

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
            MotDePasse = HashMotDePasse(dto.MotDePasse)
        };
    }

    public void UpdateFromDTO(UtilisateurUpsertDTO dto) {
        Nom = dto.Nom;
        Email = dto.Email;
        MotDePasse = HashMotDePasse(dto.MotDePasse);
    }

    public bool VerifierMotDePasse(string motDePasse) {
        return VerifierMotDePasse(motDePasse, MotDePasse);
    }

    /// <summary>
    /// Fonction utilitaire pour hasher un mot de passe avec Argon2id.
    /// </summary>
    /// <param name="motDePasse">Le mot de passe à hasher</param>
    /// <returns>Le sel et le hash combinés en une seule chaîne Base64</returns>
    public static string HashMotDePasse(string motDePasse) {
        // Générer un sel aléatoire de 16 octets
        byte[] sel = RandomNumberGenerator.GetBytes(16);
        // Hash du mot de passe avec Argon2id
        byte[] hashBytes = HashMotDePasseBytes(motDePasse, sel);

        // Combiner le sel et le hash pour les stocker ensemble
        byte[] hashAvecSel = new byte[16 + hashBytes.Length];
        Array.Copy(sel, 0, hashAvecSel, 0, 16);
        Array.Copy(hashBytes, 0, hashAvecSel, 16, hashBytes.Length);

        return Convert.ToBase64String(hashAvecSel);
    }

    /// <summary>
    /// Fonction utilitaire pour vérifier un mot de passe contre un hash stocké.
    /// </summary>
    /// <param name="motDePasse">Le mot de passe à vérifier</param>
    /// <param name="hash">Le hash stocké (contenant le sel et le hash)</param>
    /// <returns>True si le mot de passe correspond au hash, sinon false</returns>
    public static bool VerifierMotDePasse(string motDePasse, string hash) {
        byte[] hashBytes = Convert.FromBase64String(hash);

        // Extraire le sel du hash stocké
        byte[] sel = new byte[16];
        Array.Copy(hashBytes, 0, sel, 0, 16);

        // Extraire le hash stocké (sans le sel)
        byte[] hashStocke = new byte[hashBytes.Length - 16];
        Array.Copy(hashBytes, 16, hashStocke, 0, hashStocke.Length);

        // Hash du mot de passe fourni avec le même sel
        byte[] hashMotDePasse = HashMotDePasseBytes(motDePasse, sel);

        return hashMotDePasse.SequenceEqual(hashStocke);
    }

    /// <summary>
    /// Fonction utilitaire pour hasher un mot de passe avec Argon2id et un sel donné.
    /// </summary>
    /// <param name="motDePasse">Le mot de passe à hasher</param>
    /// <param name="sel">Le sel à utiliser pour le hash</param>
    /// <returns>Le hash du mot de passe</returns>
    private static byte[] HashMotDePasseBytes(string motDePasse, byte[] sel) {
        byte[] motDePasseBytes = System.Text.Encoding.UTF8.GetBytes(motDePasse);
        var argon2 = new Argon2id(motDePasseBytes) {
            Salt = sel,
            DegreeOfParallelism = 1, // Nombre de threads à utiliser
            Iterations = 2, // Nombre d'itérations
            MemorySize = 19456 // Mémoire utilisée en KiB
        };
        return argon2.GetBytes(128);
    }
}
