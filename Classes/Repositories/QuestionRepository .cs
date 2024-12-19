using CollabTasks_2._0.Classes.Models;
using CollabTasks_2._0.Interfaces;
using Google.Cloud.Firestore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabTasks_2._0.Classes.Reositories
{
    public class QuestionRepository : IQuestionRepository
    {
        private readonly FirestoreDb _db;

        public QuestionRepository(FirestoreDb db)
        {
            _db = db;
        }

        public async Task<string> AddQuestionAsync(Question question)
        {
            var docRef = _db.Collection("Questions").Document();
            question.Id = docRef.Id;
            await docRef.SetAsync(question);
            return docRef.Id;
        }

        public async Task<List<Question>> GetQuestionsByGroupAsync(string groupId)
        {
            var query = _db.Collection("Questions").WhereEqualTo("GroupId", groupId);
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<Question>()).ToList();
        }

        public async Task<List<Question>> GetQuestionsByUserAsync(string userId)
        {
            var query = _db.Collection("Questions").WhereArrayContains("Users", userId);
            var snapshot = await query.GetSnapshotAsync();
            return snapshot.Documents.Select(doc => doc.ConvertTo<Question>()).ToList();
        }

        public async Task UpdateQuestionAsync(Question question)
        {
            var docRef = _db.Collection("Questions").Document(question.Id);
            await docRef.SetAsync(question, SetOptions.Overwrite);
        }
        public async Task<Question> GetQuestionByIdAsync(string questionId)
        {
            try
            {
                var docRef = _db.Collection("Questions").Document(questionId);
                var snapshot = await docRef.GetSnapshotAsync();

                if (snapshot.Exists)
                {
                    return snapshot.ConvertTo<Question>();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error fetching question by ID: {ex.Message}", ex);
            }
        }
        public async Task DeleteQuestionAsync(string questionId)
        {
            var questionRef = _db.Collection("Questions").Document(questionId);
            var docSnapshot = await questionRef.GetSnapshotAsync();

            if (!docSnapshot.Exists)
            {
                throw new Exception($"Document with ID {questionId} does not exist.");
            }

            await questionRef.DeleteAsync();
        }

    }
}
