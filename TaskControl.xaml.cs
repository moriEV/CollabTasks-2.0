using CollabTasks_2._0.Classes.Services;
using CollabTasks_2._0.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CollabTasks_2._0.Classes.Models;

namespace CollabTasks_2._0
{
    /// <summary>
    /// Логика взаимодействия для TaskControl.xaml
    /// </summary>
    public partial class TaskControl : UserControl
    {
        public string QuestionId { get; set; }
        public string GroupId { get; set; }
        private readonly UserService _userService;
        private readonly QuestionService _questionService;
        private readonly GroupService _groupService;
        private readonly string _currentUserEmail;
        private readonly MainWindow _mainWindow;

        public TaskControl(UserService userService, QuestionService questionService, GroupService groupService, string currentUserEmail, string groupId, string questionId, MainWindow mainWindow)
        {
            InitializeComponent();
            _userService = userService;
            _questionService = questionService;
            _groupService = groupService;
            _currentUserEmail = currentUserEmail;
            GroupId = groupId;
            QuestionId = questionId;
            _mainWindow = mainWindow;

            InitializeButtons();
        }

        private async void InitializeButtons()
        {
            try
            {
                if (string.IsNullOrEmpty(QuestionId) || string.IsNullOrEmpty(GroupId))
                {
                    CompleteButton.IsEnabled = false;
                    FailButton.IsEnabled = false;
                    DeleteButton.IsEnabled = false;
                    return;
                }

                var currentUser = await _userService.GetUserByEmailAsync(_currentUserEmail);
                if (currentUser.Data == null)
                {
                    CompleteButton.IsEnabled = false;
                    FailButton.IsEnabled = false;
                    DeleteButton.IsEnabled = false;
                    return;
                }

                var group = await _groupService.GetGroupByIdAsync(GroupId);
                if (group == null)
                {
                    CompleteButton.IsEnabled = false;
                    FailButton.IsEnabled = false;
                    DeleteButton.IsEnabled = false;
                    return;
                }

                var question = await _questionService.GetQuestionForIdAsync(QuestionId);
                if (question == null)
                {
                    CompleteButton.IsEnabled = false;
                    FailButton.IsEnabled = false;
                    DeleteButton.IsEnabled = false;
                    return;
                }
                // Проверка прав
                bool isDignified = question.CreatorId == currentUser.DocumentId ||
                                    group.AdminIds.Contains(currentUser.DocumentId) ||
                                    group.IdCreator == currentUser.DocumentId;
                bool Assigned = question.Status != Status.Fail && question.Status != Status.Succes;
                CompleteButton.IsEnabled = Assigned && isDignified ? isDignified : false;
                FailButton.IsEnabled = Assigned && isDignified ? isDignified : false;
                DeleteButton.IsEnabled = isDignified;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации кнопок: {ex.Message}", "Ошибка создания",MessageBoxButton.OK,MessageBoxImage.Error);
                CompleteButton.IsEnabled = false;
                FailButton.IsEnabled = false;
                DeleteButton.IsEnabled = false;
            }
        }

        private async void CompleteButton_Click(object sender, RoutedEventArgs e)
        {
            await UpdateTaskStatus(Status.Succes);
        }

        private async void FailButton_Click(object sender, RoutedEventArgs e)
        {
            await UpdateTaskStatus(Status.Fail);
        }

        private async Task UpdateTaskStatus(Status newStatus)
        {
            try
            {
                if (string.IsNullOrEmpty(QuestionId) || string.IsNullOrEmpty(GroupId))
                {
                    MessageBox.Show("ID задания или группы не найдены.","Ошибка наличия связи группы и задания",MessageBoxButton.OK,MessageBoxImage.Error);
                    return;
                }

                var currentUser = await _userService.GetUserByEmailAsync(_currentUserEmail);
                if (currentUser.Data == null)
                {
                    MessageBox.Show("Пользователь не найден.", "Ошибка наличия пользователя", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var group = await _groupService.GetGroupByIdAsync(GroupId);
                if (group == null)
                {
                    MessageBox.Show("Группа не найдена.", "Ошибка наличия группы", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var question = await _questionService.GetQuestionForIdAsync(QuestionId);
                if (question == null)
                {
                    MessageBox.Show("Задание не найдено.", "Ошибка наличия задания", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }


                // Проверка даты
                if (question.Date.HasValue && question.Date.Value.ToDateTime() < DateTime.UtcNow && newStatus != Status.Fail)
                {
                    MessageBox.Show("Это задание просрочено и не может быть помечено как выполненное.", "Ошибка срока задачи", MessageBoxButton.OK, MessageBoxImage.Information);
                    newStatus = Status.Fail;
                }

                // Обновление статуса задачи
                question.Status = newStatus;
                await _questionService.UpdateQuestionAsync(question);

                // Блокировка кнопок после изменения статуса
                CompleteButton.IsEnabled = false;
                FailButton.IsEnabled = false;

                MessageBox.Show($"Статус задачи поменян на {newStatus}.", "Смена статуса задачи", MessageBoxButton.OK, MessageBoxImage.Information);

                // Обновление листбоксов
                _mainWindow.UpdateQuestionTabs();

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка изменения статуса задачи: {ex.Message}", "Ошибка смены статуса задачи", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(QuestionId) || string.IsNullOrEmpty(GroupId))
                {
                    MessageBox.Show("ID группы или задания не найден.", "Ошибка наличия связи группы и задания", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var currentUser = await _userService.GetUserByEmailAsync(_currentUserEmail);
                if (currentUser.Data == null)
                {
                    MessageBox.Show("Пользователь не найден.","Ошибка наличия пользователя", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var group = await _groupService.GetGroupByIdAsync(GroupId);
                if (group == null)
                {
                    MessageBox.Show("Группа не найдена.", "Ошибка наличия группы", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var question = await _questionService.GetQuestionForIdAsync(QuestionId);
                if (question == null)
                {
                    MessageBox.Show("Задание не найдено.", "Ошибка наличия задания", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // Проверка прав
                bool isDignified = question.CreatorId == _currentUserEmail ||
                                    group.AdminIds.Contains(currentUser.DocumentId) ||
                                    group.IdCreator == currentUser.DocumentId;

                if (!isDignified)
                {
                    MessageBox.Show("У вас нет прав для удаления этого задания.", "Несоотвествие прав доступа", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show("Вы уверены что хотите удалить это задание?", "Удаление", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.No)
                {
                    return;
                }

                // Удаление задания
                await _questionService.DeleteQuestionAsync(QuestionId);

                // Блокировка кнопок после удаления
                CompleteButton.IsEnabled = false;
                FailButton.IsEnabled = false;
                DeleteButton.IsEnabled = false;

                MessageBox.Show("Задание удалено.", "Удаление", MessageBoxButton.OK, MessageBoxImage.Information);

                // Обновление листбоксов
                _mainWindow.UpdateQuestionTabs();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления задачи: {ex.Message}", "Системная ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
