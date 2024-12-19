using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabTasks_2._0.Classes.Models
{
    [FirestoreData]
    public class Group
    {
        [FirestoreProperty("Id")]
        public string Id { get; set; }

        [FirestoreProperty("Name")]
        public string Name { get; set; }

        [FirestoreProperty("IdCreator")]
        public string IdCreator { get; set; }

        [FirestoreProperty("UserIds")]
        public List<string> UserIds { get; set; } = new();

        [FirestoreProperty("QuestionsId")]
        public List<string> QuestionsId { get; set; } = new();
        [FirestoreProperty("AdminIds")]
        public List<string> AdminIds { get; set; } = new();
        [FirestoreProperty("BannedIds")]
        public List<string> BannedIds { get; set; } = new();
    }
}
