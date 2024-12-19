using CollabTasks_2._0.Classes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabTasks_2._0.Interfaces
{
    public interface IGroupRepository
    {
        Task<string> AddGroupAsync(Group group);
        Task<Group> GetGroupByIdAsync(string groupId);
        Task AddUserToGroupAsync(string groupId, string userId);
        Task UpdateGroupAsync(string groupId, Group group);
        Task<List<Group>> GetGroupsForUserAsync(string userId);

    }
}
