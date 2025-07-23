using HmsNet.Data;
using HmsNet.Models.Domain;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;

namespace HmsNet.Services
{
    public class AuthService
    {
        private readonly AppDbContext _context;

        public AuthService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> Register(User user)
        {
            if (await _context.Users.AnyAsync(u => u.Username == user.Username))
                return "Username already exists";

            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                return "Email already exists";

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return "Success";
        }

        public async Task<User?> Authenticate(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                return null;

            return user;
        }
    }
}