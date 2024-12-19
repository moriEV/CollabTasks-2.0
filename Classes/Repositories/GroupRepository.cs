using CollabTasks_2._0.Classes.Models;
using CollabTasks_2._0.Interfaces;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabTasks_2._0.Classes
{
    public class GroupRepository : IGroupRepository
    {
        private readonly FirestoreDb _db;

        public GroupRepository(FirestoreDb db)
        {
            _db = db;
        }

        public async Task<string> AddGroupAsync(Group group)
        {
            var groupsRef = _db.Collection("Groups");
            var docRef = groupsRef.Document(); // Генерация уникального идентификатора
            group.Id = docRef.Id;
            await docRef.SetAsync(group);
            return docRef.Id;
        }
        public async Task<Group> GetGroupByIdAsync(string groupId)
        {
            var groupDoc = await _db.Collection("Groups").Document(groupId).GetSnapshotAsync();
            return groupDoc.Exists ? groupDoc.ConvertTo<Group>() : null;
        }

        public async Task AddUserToGroupAsync(string groupId, string userId)
        {
            var groupDoc = _db.Collection("Groups").Document(groupId);
            await groupDoc.UpdateAsync("UserIds", FieldValue.ArrayUnion(userId));
        }
        public async Task UpdateGroupAsync(string groupId, Group group)
        {
            var groupDoc = _db.Collection("Groups").Document(groupId);
            await groupDoc.SetAsync(group, SetOptions.Overwrite); // Перезаписываем данные
        }
        public async Task<List<Group>> GetGroupsForUserAsync(string userId)
        {
            var groupsRef = _db.Collection("Groups");
            var querySnapshot = await groupsRef.WhereArrayContains("UserIds", userId).GetSnapshotAsync();

            return querySnapshot.Documents.Select(doc => doc.ConvertTo<Group>()).ToList();
        }

    }
}
