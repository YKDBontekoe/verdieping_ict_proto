using System;
using System.Security.Cryptography;
using System.Text;

namespace Verdieping_ICT
{
    public static class GenerateSha256Hash
    {
        public static string ComputeStringToSha256Hash(string plainText)
        {
            // Maak een SHA-256 hash van een string 
            using var sha256Hash = SHA256.Create();
            
            // Bereken hash 
            // Returneerd een byt array
            var bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(plainText));

            // Retuneerd de gegenereerde hash.
            return Convert.ToBase64String(bytes);
        }
        
        public static string ComputeStringToSha256HashSalt(string plainText, int iterations = 2000)     
        {
            // Genereer een random salt voor salting
            var salt = new byte[24];
            new RNGCryptoServiceProvider().GetBytes(salt);

            // hash-wachtwoord gegeven salt en iteraties (standaard op 2000)
            // iteraties leveren moeilijkheden op bij het kraken
            var pbkdf2 = new Rfc2898DeriveBytes(plainText, salt, iterations);
            var hash = pbkdf2.GetBytes(24);   
                
            // Retuneerd de gegenereerde salt iteraties en de hash.
            return Convert.ToBase64String(salt) + "|" + iterations + "|" +
                   Convert.ToBase64String(hash);
        }
        
        public static bool IsValid(string testPassword, string hash, bool isSalted)
        {
            // Als de hash niet gesalt is kan deze direct worden vergeleken
            if (!isSalted) return hash.Equals(ComputeStringToSha256Hash(testPassword));
            
            // haal originele waarden uit de gescheiden (|) hashtekst
            var origHashedParts = hash.Split('|');
            var origSalt = Convert.FromBase64String(origHashedParts[0]);
            var origIterations = Int32.Parse(origHashedParts[1]);
            var origHash = origHashedParts[2];

            // hash genereren van testwachtwoord en origineel salt en iteraties
            var pbkdf2 = new Rfc2898DeriveBytes(testPassword, origSalt, origIterations);
            var testHash = pbkdf2.GetBytes(24);
            
            
            return Convert.ToBase64String(testHash).Equals(origHash);
        }
    }
}