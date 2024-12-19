using CollabTasks_2._0.Classes.Models;
using CollabTasks_2._0.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabTasks_2._0.Classes
{
    public class GroupService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GroupService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> CreateGroupAsync(Group group, string userId)
        {
            var groupId = await _unitOfWork.Groups.AddGroupAsync(group);

            // Обновляем пользователей и группы
            await _unitOfWork.Groups.AddUserToGroupAsync(groupId, userId);
            await _unitOfWork.Users.AddGroupToUserAsync(userId, groupId);

            return groupId;
        }

        public async Task<Group> GetGroupByIdAsync(string groupId)
        {
            return await _unitOfWork.Groups.GetGroupByIdAsync(groupId);
        }
        public async Task UpdateGroupAsync(Group group, string groupId)
        {
            await _unitOfWork.Groups.UpdateGroupAsync(groupId, group);
        }
        public async Task<List<Group>> GetGroupsForUserAsync(string userId)
        {
            return await _unitOfWork.Groups.GetGroupsForUserAsync(userId);
        }

    }
}
