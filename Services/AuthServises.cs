using Supabase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase;
using TaskManagment.Data;


namespace TaskManagment.Services
{
    public class AuthServises
    {
        public static async Task LoginUser(
        string email,
        string password,
        Action<string, string> onSuccess,
        Action<string> onError)
        {
            try
            {
                var users = await SupabaseClient.supabase.From<User>().Get();
                var user = users.Models.FirstOrDefault(u => u.Email == email);

                if (user == null)
                {
                    onError("User not found");
                    return;
                }

                bool passwordVerified = BCrypt.Net.BCrypt.Verify(password, user.Password);
                if (passwordVerified)
                {
                    onSuccess(user.Id.ToString(), user.Role);
                }
                else
                {
                    onError("Invalid password");
                }
            }
            catch (Exception ex)
            {
                onError(ex.Message);
            }
        }
            public static async Task RegisterUser(
            string email,
            string password,
            string name,
            Action onSuccess,
            Action<string> onError)
            {
                try
                {
                    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
                    var newUser = new User
                    {
                        Email = email,
                        Password = hashedPassword,
                        Name = name,
                        Role = "user"
                    };

                    await SupabaseClient.supabase.From<User>().Insert(newUser);
                    onSuccess();
                }
                catch (Exception ex)
                {
                    onError(ex.Message);
                }
            }
    }

}
