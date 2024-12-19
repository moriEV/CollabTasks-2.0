using CollabTasks_2._0.Classes.Reositories;
using CollabTasks_2._0.Classes.Services;
using CollabTasks_2._0.Interfaces;
using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabTasks_2._0.Classes.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly FirestoreDb _db;
        private IUserRepository _userRepository;
        private IGroupRepository _groupRepository;
        public IQuestionRepository _questionRepository;
        public UnitOfWork(FirestoreDb db)
        {
            _db = db;
        }

        public IUserRepository Users => _userRepository ??= new UserRepository(_db);
        public IGroupRepository Groups => _groupRepository ??= new GroupRepository(_db);
        public IQuestionRepository Questions => _questionRepository ??= new QuestionRepository(_db);
    }
}
