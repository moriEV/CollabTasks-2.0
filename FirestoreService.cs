using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;

namespace CollabTasks_2._0
{
    internal class FirestoreService
    {
        private static FirestoreDb _db;

        public static void Initialize(string privateKey, string projectId)
        {
            if (_db == null)
            {
                if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(privateKey))
                {
                    throw new InvalidOperationException("Project ID and Private Key must be provided.");
                }

                // Формируем JSON для авторизации
                var credentialsJson = "{" +
                    "\"type\": \"service_account\"," +
                    "\"project_id\": \"" + projectId + "\"," +
                    "\"private_key\": \"" + privateKey.Replace("\n", "\\n") + "\"," +
                    "\"client_email\": \"" + (Environment.GetEnvironmentVariable("FIREBASE_CLIENT_EMAIL") ?? "") + "\"," +
                    "\"token_uri\": \"" + (Environment.GetEnvironmentVariable("FIREBASE_TOKEN_URI") ?? "") + "\"}";

                // Сохраняем JSON во временный файл
                var tempPath = Path.GetTempFileName();
                File.WriteAllText(tempPath, credentialsJson);

                // Устанавливаем путь к JSON как переменную окружения
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", tempPath);

                // Инициализируем Firestore
                _db = FirestoreDb.Create(projectId);
            }
        }

        public static FirestoreDb GetDatabase()
        {
            if (_db == null)
            {
                throw new InvalidOperationException("Firestore is not initialized. Call Initialize() first.");
            }
            return _db;
        }
    }


}
