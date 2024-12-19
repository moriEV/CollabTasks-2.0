using CollabTasks_2._0.Classes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabTasks_2._0.Interfaces
{
    public interface IUserRepository
    {
        Task<string> AddUserAsync(User user);
        Task AddGroupToUserAsync(string userId, string groupId);
        Task<User> GetUserByDocumentIdAsync(string documentId);
        Task<(User Data, string DocumentId)> GetUserByEmailAsync(string email);
    }
}
