using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabTasks_2._0.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository Users { get; }
        IGroupRepository Groups { get; }
        IQuestionRepository Questions { get; }
    }
}
