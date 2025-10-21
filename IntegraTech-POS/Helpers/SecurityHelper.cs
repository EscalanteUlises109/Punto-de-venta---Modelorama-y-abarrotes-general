using System;
using System.Security.Cryptography;
using System.Text;

namespace IntegraTech_POS.Helpers
{
    public static class SecurityHelper
    {
        
        
        
        
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseÃ±a no puede estar vacÃ­a");

            using var sha256 = SHA256.Create();
            
            var bytes = Encoding.UTF8.GetBytes(password.Trim());
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        
        
        
        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
                return false;

            var hashOfInput = HashPassword(password);
            return hashOfInput.Equals(storedHash, StringComparison.Ordinal);
        }

        
        public static string HashPasswordBCrypt(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("La contraseÃ±a no puede estar vacÃ­a");
#if BCRYPT_ENABLED
            return BCrypt.Net.BCrypt.HashPassword(password.Trim());
#else
            
            return HashPassword(password);
#endif
        }

        public static bool VerifyPasswordBCrypt(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
                return false;
#if BCRYPT_ENABLED
            return BCrypt.Net.BCrypt.Verify(password.Trim(), storedHash);
#else
            return VerifyPassword(password, storedHash);
#endif
        }

        public static bool VerifyByAlgorithm(string password, string storedHash, string? algorithm)
        {
            if (string.Equals(algorithm, "BCRYPT", StringComparison.OrdinalIgnoreCase))
                return VerifyPasswordBCrypt(password, storedHash);
            return VerifyPassword(password, storedHash);
        }

        
        
        
        public static string GenerateRandomPassword(int length = 12)
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
            var random = new Random();
            var password = new char[length];
            
            for (int i = 0; i < length; i++)
            {
                password[i] = validChars[random.Next(validChars.Length)];
            }
            
            return new string(password);
        }
    }
}

