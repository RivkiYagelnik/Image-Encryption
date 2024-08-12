using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL.Interface
{
    public interface IUserService
    {
        public Task<bool> CreateUser(UserDto newUser);
        public Task<bool> UpdateUser(UserDto updateUser);
        public Task<UserDto> GetUser(int userId);
        public Task<List<UserDto>> GetAllUsers();
        public Task<UserDto>Login(int userId, string password); 
        public  Task<bool> ResetPassword(string password1, string password2, int id, string validation);
        public Task<bool>DeleteUser(int userId);    

    }
}
