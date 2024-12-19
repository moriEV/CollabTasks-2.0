using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollabTasks_2._0.Classes.Models
{
    [FirestoreData]
    public class User
    {
        [FirestoreProperty("Id")]
        public string Id { get; set; }

        [FirestoreProperty("Name")]
        public string Name { get; set; }

        [FirestoreProperty("Email")]
        public string Email { get; set; }

        [FirestoreProperty("GroupIds")]
        public List<string> GroupIds { get; set; } = new();
    }
}
