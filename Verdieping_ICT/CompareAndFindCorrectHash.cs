using System;
using System.Collections.Generic;

namespace Verdieping_ICT
{
    public class CompareAndFindCorrectHash
    {
        public static bool IsHashTheSame(string input, string hash, bool isSalted) 
        {
            return GenerateSha256Hash.IsValid(input, hash, isSalted);
        }

        public static InputOutputModel GenerateRandomHash(bool isSalted)
        {
            var strInput = GenerateRandomString();
            var hashOutput = isSalted ? GenerateSha256Hash.ComputeStringToSha256HashSalt(strInput) : GenerateSha256Hash.ComputeStringToSha256Hash(strInput);
            
            return new InputOutputModel(){InputString = strInput, OutputHash = hashOutput};
        }

        private static string GenerateRandomString()
        {
            
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[new Random().Next(2, 30)];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }
    }
}