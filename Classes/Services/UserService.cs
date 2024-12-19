using CollabTasks_2._0.Classes.Models;
using CollabTasks_2._0.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabTasks_2._0.Classes
{
    public class UserService
    {
        private readonly IUnitOfWork _unitOfWork;

        public UserService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> RegisterUserAsync(User user)
        {
            return await _unitOfWork.Users.AddUserAsync(user);
        }
        public async Task<User> GetUserByDocumentIdAsync(string documentId)
        {
            var userDoc = await _unitOfWork.Users.GetUserByDocumentIdAsync(documentId);
            return userDoc;
        }
        public async Task<(User Data, string DocumentId)> GetUserByEmailAsync(string email)
        {
            return await _unitOfWork.Users.GetUserByEmailAsync(email);
        }
    }
}
