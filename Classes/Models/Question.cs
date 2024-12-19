using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace CollabTasks_2._0.Classes.Models
{
    [FirestoreData]
    public class Question
    {
        [FirestoreProperty("Id")]
        public string Id { get; set; }

        [FirestoreProperty(nameof(Description))]
        public string Description { get; set; }

        [FirestoreProperty(nameof(Date))]
        public Timestamp? Date { get; set; }
        [FirestoreProperty("CreatorId")]
        public string CreatorId { get; set; }
        [FirestoreProperty("GroupId")]
        public string GroupId { get; set; }

        [FirestoreProperty(nameof(Users))]
        public List<string> Users { get; set; }
        [FirestoreProperty("Status")]
        public Status Status { get; set; }
    }
    public enum Status
    {
        Assign,
        Succes,
        Fail
    };
}
