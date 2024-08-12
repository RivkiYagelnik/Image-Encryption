using DAL.Interfaces;
using DAL.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BL.Interface;
namespace BL.Services
{
    public class UserService:IUserService
    {
        private readonly IUserData _userData;
        public UserService(IUserData userData)
        {
            _userData = userData;
        }

        public async Task<bool> CreateUser(UserDto newUser)
        {
            return await _userData.CreateUser(newUser);
        }
        public Task<bool> UpdateUser(UserDto updateUser)
        {
            return _userData.UpdateUser(updateUser);    
        }
        public async Task<UserDto> GetUser(int userId)
        {
            return await _userData.GetUser(userId);
        }
        public async Task<List<UserDto>> GetAllUsers()
        {
            return await _userData.GetAllUsers();
        }
        public async Task<bool> ResetPassword(string password1,string password2, int id,string validation)
        {
            if (!password1.Equals(password2))
                return false;
            return await _userData.ResetPassword (password1, id);
        }

        public async Task<UserDto> Login(int userId,string password)
        {
            var user=await GetUser(userId); 
            if(user!= null)
            {
                if (user.Password != password)
                    throw new Exception("the password not match!!!");
            }
            return user;
        }

        public Task<bool> DeleteUser(int userId)
        {
            return _userData.DeleteUser(userId);    
        }
    }
}
