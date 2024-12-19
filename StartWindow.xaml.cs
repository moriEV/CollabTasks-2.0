using Google.Api;
using Google.Cloud.Firestore;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Firebase.Auth;
using Google.Cloud.Firestore.V1;
using Newtonsoft.Json;
using System.Net.Http;
using CollabTasks_2._0.Classes;
using CollabTasks_2._0.Classes.Services;
using DotNetEnv;
using System;
using System.IO;

namespace CollabTasks_2._0
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class StartWindow : Window
    {
        private readonly UserService _userService;
        private readonly GroupService _groupService;
        private readonly QuestionService _questionService;
        private FirestoreDb db;
        private FirebaseAuthProvider authProvider;
        private bool isSignIn = false;
        public StartWindow()
        {
            InitializeComponent();
            Env.Load("goida.env");

            this.Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri("Icon.png", UriKind.Relative));
            // Получаем переменные с env файла
            var projectId = Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID");
            var privateKey = Environment.GetEnvironmentVariable("FIREBASE_PRIVATE_KEY");
            var ApiKey = Environment.GetEnvironmentVariable("FIREBASE_API_KEY");
            //Инициализируем firestore
            FirestoreService.Initialize(privateKey, projectId);
            db = FirestoreDb.Create(projectId);
            authProvider = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));

            var unitOfWork = new Classes.Repositories.UnitOfWork(db);
            _userService = new UserService(unitOfWork);
            _groupService = new GroupService(unitOfWork);
            _questionService = new QuestionService(unitOfWork);
        }

        private async void SignButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            button.IsEnabled = false; // Отключаем кнопку
            try
            {
                var email = EmailBox.Text;
                var password = HidePassRadioButton.Visibility == System.Windows.Visibility.Collapsed ? PasswordBox.Password : PasswordTextBox.Text;
                var confirmPassword = HidePassRadioButton.Visibility == System.Windows.Visibility.Collapsed ? ConfirmPasswordBox.Password : ConfirmPassTextBox.Text;
                var name = NameBox.Text;

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
                {
                    MessageBox.Show("Поля почты и пароля не должны быть пустыми.", "Несоответствие полей", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (password.Length < 6)
                {
                    MessageBox.Show("Длина пароля должна быть как минимум 6 символов","Несоотвествие полей",MessageBoxButton.OK,MessageBoxImage.Information);
                    return;
                }
                if (!IsValidEmail(email))
                {
                    MessageBox.Show("Почта должна иметь вид example@example.example","Несоотвествие полей",MessageBoxButton.OK,MessageBoxImage.Information);
                    return;
                }
                if (password != confirmPassword)
                {
                    MessageBox.Show("Пароли не совпадают.", "Несоответствие паролей", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Проверяем, существует ли уже пользователь с таким email
                var existingUser = await _userService.GetUserByEmailAsync(email);
                if (existingUser.Data != null)
                {
                    MessageBox.Show("Эта почта уже используется, пожалуйста используйте другую почту.", "Пользователь существует", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Регистрируем пользователя в Firebase Authentication
                try
                {
                    await authProvider.CreateUserWithEmailAndPasswordAsync(email, password);
                    MessageBox.Show("Пользователь успешно зарегистрирован!", "Регистрация успешна", MessageBoxButton.OK, MessageBoxImage.Information);
                    // Регистрируем нового пользователя в базе данных
                    var newUser = new Classes.Models.User { Name = name, Email = email };
                    await _userService.RegisterUserAsync(newUser);
                    SignInButton_Click(null,null);
                }
                catch (FirebaseAuthException ex) when (ex.Reason == AuthErrorReason.EmailExists)
                {
                    MessageBox.Show("Эта почта уже используется, пожалуйста используйте другую почту.", "Пользователь существует", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка регистрации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                button.IsEnabled = true; // Включаем кнопку обратно
            }
        }

        private bool IsValidEmail(string email)
        {
            // Простейшая проверка формата электронного адреса
            return !string.IsNullOrEmpty(email) &&
                   email.Contains("@") &&
                   email.Contains(".") &&
                   email.IndexOf("@") < email.LastIndexOf(".");
        }
        private void ShowPassRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (isSignIn == false)
            {
                PasswordTextBox.Text = PasswordBox.Password;
                ConfirmPassTextBox.Text = ConfirmPasswordBox.Password;
                PasswordBox.Visibility = System.Windows.Visibility.Collapsed;
                ConfirmPasswordBox.Visibility = System.Windows.Visibility.Collapsed;
                ShowPassRadioButton.Visibility = System.Windows.Visibility.Collapsed;
                PasswordTextBox.Visibility = System.Windows.Visibility.Visible;
                ConfirmPassTextBox.Visibility = System.Windows.Visibility.Visible;
                HidePassRadioButton.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = System.Windows.Visibility.Collapsed;
                ShowPassRadioButton.Visibility = System.Windows.Visibility.Collapsed;
                PasswordTextBox.Visibility = System.Windows.Visibility.Visible;
                HidePassRadioButton.Visibility = System.Windows.Visibility.Visible;
            }
        }

        private async void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button != null)
            {
                button.IsEnabled = false; // Отключаем кнопку
            }
            try
            {
                var email = EmailBox.Text.Trim();
                var password = HidePassRadioButton.Visibility == System.Windows.Visibility.Collapsed ? PasswordBox.Password : PasswordTextBox.Text;

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Поля почты и пароля не должны быть пустыми.","Несоотвествие полей",MessageBoxButton.OK,MessageBoxImage.Information);
                    return;
                }
                if (!IsValidEmail(email))
                {
                    MessageBox.Show("Почта должна иметь вид example@example.example", "Несоотвествие полей", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                if (password.Length < 6)
                {
                    MessageBox.Show("Длина пароля должна быть как минимум 6 символов", "Несоотвествие полей", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                await authProvider.SignInWithEmailAndPasswordAsync(email, password);
                var mainWindow = new MainWindow(email, _userService, _groupService, _questionService);
                mainWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if(button != null)
                {
                    button.IsEnabled = true; // Включаем кнопку обратно
                }
            }
        }



        private void SignLabel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            isSignIn = !isSignIn;
            if (isSignIn)
            {
                this.Title = "Авторизация";
                SignLabel.Content = "Зарегистрироваться";
                QuLabel.Content = "Еще нет аккаунта?";
                NameBox.Visibility = System.Windows.Visibility.Collapsed;
                NameLabel.Visibility = System.Windows.Visibility.Collapsed;
                ConfirmPassTextBox.Visibility = System.Windows.Visibility.Collapsed;
                ConfirmPasswordBox.Visibility = System.Windows.Visibility.Collapsed;
                ConfirmLabel.Visibility = System.Windows.Visibility.Collapsed;
                SignButton.Visibility = System.Windows.Visibility.Collapsed;
                SignInButton.Visibility = System.Windows.Visibility.Visible;
                EmailBox.Text = "";
                NameBox.Text = "";
                PasswordBox.Password = "";
                ConfirmPasswordBox.Password = "";
                PasswordTextBox.Text = "";
                ConfirmPassTextBox.Text = "";
            }
            else
            {
                this.Title = "Аутентификация";
                SignLabel.Content = "Войти";
                QuLabel.Content = "Уже есть аккаунт?";
                if (PasswordBox.Visibility == System.Windows.Visibility.Visible)
                {
                    ConfirmPasswordBox.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    ConfirmPassTextBox.Visibility = System.Windows.Visibility.Visible;
                }
                NameLabel.Visibility = System.Windows.Visibility.Visible;
                NameBox.Visibility = System.Windows.Visibility.Visible;
                SignButton.Visibility = System.Windows.Visibility.Visible;
                ConfirmLabel.Visibility = System.Windows.Visibility.Visible;
                SignInButton.Visibility = System.Windows.Visibility.Collapsed;
                EmailBox.Text = "";
                NameBox.Text = "";
                PasswordBox.Password = "";
                ConfirmPasswordBox.Password = "";
                PasswordTextBox.Text = "";
                ConfirmPassTextBox.Text = "";
            }

        }

        private void HidePassRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (isSignIn == false)
            {
                PasswordBox.Password = PasswordTextBox.Text;
                ConfirmPasswordBox.Password = ConfirmPassTextBox.Text;
                PasswordBox.Visibility = System.Windows.Visibility.Visible;
                ConfirmPasswordBox.Visibility = System.Windows.Visibility.Visible;
                ShowPassRadioButton.Visibility = System.Windows.Visibility.Visible;
                PasswordTextBox.Visibility = System.Windows.Visibility.Collapsed;
                ConfirmPassTextBox.Visibility = System.Windows.Visibility.Collapsed;
                HidePassRadioButton.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordBox.Visibility = System.Windows.Visibility.Visible;
                ShowPassRadioButton.Visibility = System.Windows.Visibility.Visible;
                PasswordTextBox.Visibility = System.Windows.Visibility.Collapsed;
                HidePassRadioButton.Visibility = System.Windows.Visibility.Collapsed;
            }
        }
    }
}