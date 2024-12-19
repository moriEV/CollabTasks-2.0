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
    public class UserRepository : IUserRepository
    {
        private readonly FirestoreDb _db;

        public UserRepository(FirestoreDb db)
        {
            _db = db;
        }

        public async Task<string> AddUserAsync(User user)
        {
            var usersRef = _db.Collection("Users");
            var docRef = usersRef.Document(); // Генерация уникального идентификатора
            user.Id = docRef.Id;
            await docRef.SetAsync(user);
            return user.Id;
        }

        public async Task AddGroupToUserAsync(string userId, string groupId)
        {
            var userDoc = _db.Collection("Users").Document(userId);
            await userDoc.UpdateAsync("GroupIds", FieldValue.ArrayUnion(groupId));
        }

        public async Task<User> GetUserByDocumentIdAsync(string documentId)
        {
            var userDoc = await _db.Collection("Users").Document(documentId).GetSnapshotAsync();
            return userDoc.Exists ? userDoc.ConvertTo<User>() : null;
        }
        public async Task<(User Data, string DocumentId)> GetUserByEmailAsync(string email)
        {
            var usersRef = _db.Collection("Users");
            var query = usersRef.WhereEqualTo("Email", email);
            var snapshot = await query.GetSnapshotAsync();

            if (snapshot.Documents.Count == 0)
                return (null, null);

            var doc = snapshot.Documents.First();
            var user = doc.ConvertTo<User>();
            return (user, user.Id);
        }
    }
}
