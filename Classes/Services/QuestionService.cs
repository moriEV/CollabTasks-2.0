using CollabTasks_2._0.Classes.Models;
using CollabTasks_2._0.Classes.Repositories;
using CollabTasks_2._0.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabTasks_2._0.Classes.Services
{
    public class QuestionService
    {
        private readonly UnitOfWork _unitOfWork;

        public QuestionService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<string> CreateQuestionAsync(Question question)
        {
            var questionId = await _unitOfWork.Questions.AddQuestionAsync(question);
            return questionId;
        }

        public async Task<List<Question>> GetQuestionsForGroupAsync(string groupId)
        {
            return await _unitOfWork.Questions.GetQuestionsByGroupAsync(groupId);
        }

        public async Task<List<Question>> GetQuestionsForUserAsync(string userId)
        {
            return await _unitOfWork.Questions.GetQuestionsByUserAsync(userId);
        }
        public async Task<Question> GetQuestionForIdAsync(string questionId)
        {
            return await _unitOfWork.Questions.GetQuestionByIdAsync(questionId);
        }
        public async Task UpdateQuestionAsync(Question question)
        {
            await _unitOfWork.Questions.UpdateQuestionAsync(question);
        }
        public async Task DeleteQuestionAsync(string questionId) 
        {
            await _unitOfWork.Questions.DeleteQuestionAsync(questionId);
        }
    }

}
