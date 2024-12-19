using CollabTasks_2._0.Classes.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabTasks_2._0.Interfaces
{
    public interface IQuestionRepository
    {
        Task<string> AddQuestionAsync(Question question);
        Task<List<Question>> GetQuestionsByGroupAsync(string groupId);
        Task<List<Question>> GetQuestionsByUserAsync(string userId);
        Task<Question> GetQuestionByIdAsync(string questionId);
        Task UpdateQuestionAsync(Question question);
        Task DeleteQuestionAsync(string questionId);
    }
}
