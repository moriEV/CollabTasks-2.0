using Firebase.Auth;
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
using System.Windows.Shapes;
using Google.Cloud.Firestore;
using Google.Cloud.Firestore.V1;
using Newtonsoft.Json;
using System.Net.Http;
using CollabTasks_2._0.Classes;
using Supabase.Gotrue;
using CollabTasks_2._0.Interfaces;
using CollabTasks_2._0.Classes.Services;
using Google.Protobuf.WellKnownTypes;
using CollabTasks_2._0.Classes.Models;

namespace CollabTasks_2._0
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly UserService _userService;
        private readonly GroupService _groupService;
        private readonly QuestionService _questionService;
        private readonly string _email;
        private Dictionary<string, string> _groupIdMapping = new();

        public MainWindow(string email, UserService userService, GroupService groupService, QuestionService questionService)
        {
            this.Icon = new System.Windows.Media.Imaging.BitmapImage(new Uri("Icon.png", UriKind.Relative));
            InitializeComponent();
            _email = email;
            _userService = userService;
            _groupService = groupService;
            _questionService = questionService;
        }

        private async void AddGroupButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddGroupButton.IsEnabled = false;
                var dialog = new CreateGroupDialogxaml();
                if (dialog.ShowDialog() == true)
                {
                    var groupName = dialog.groupName;
                    var user = await _userService.GetUserByEmailAsync(_email);
                    if (user.Data == null)
                    {
                        MessageBox.Show("Пользователь не найден.","Ошибка создания группы",MessageBoxButton.OK,MessageBoxImage.Error);
                        return;
                    }

                    var group = new Group
                    {
                        Name = groupName,
                        IdCreator = user.DocumentId,
                        UserIds = new List<string> { user.DocumentId }
                    };

                    var groupId = await _groupService.CreateGroupAsync(group, user.DocumentId);
                    MessageBox.Show($"Группа '{groupName}' успешно создана с ID: {groupId}", "Добавление группы", MessageBoxButton.OK, MessageBoxImage.Information);
                    await LoadUserGroupsAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка создания группы", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                AddGroupButton.IsEnabled = true;
            } 
        }

        private void MainWindow1_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUserGroupsAsync();
        }

        private async Task LoadUserGroupsAsync()
        {
            try
            {
                _groupIdMapping.Clear();

                // Получаем пользователя по email
                var userResult = await _userService.GetUserByEmailAsync(_email);
                var userId = userResult.DocumentId;

                // Получаем все группы для пользователя
                var groups = await _groupService.GetGroupsForUserAsync(userId);

                foreach (var group in groups)
                {
                    // Получаем группу по ID документа
                    var groupFromDb = await _groupService.GetGroupByIdAsync(group.Id);

                    if (groupFromDb != null)
                    {
                        // Используем Name и Firestore document Id
                        _groupIdMapping[groupFromDb.Name] = groupFromDb.Id;
                    }
                }


                GroupsListBox.ItemsSource = _groupIdMapping.Keys.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка загрузки группы", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void GroupsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                GroupsListBox.IsEnabled = false;
                string selectedGroupName = GroupsListBox.SelectedItem as string;
                if (string.IsNullOrEmpty(selectedGroupName) || !_groupIdMapping.ContainsKey(selectedGroupName))
                    return;

                string groupId = _groupIdMapping[selectedGroupName];
                var group = await _groupService.GetGroupByIdAsync(groupId);

                if (group == null)
                {
                    MessageBox.Show("Группа не найдена.", "Ошибка выбора группы", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var currentUser = await _userService.GetUserByEmailAsync(_email);
                if (currentUser.Data == null)
                {
                    MessageBox.Show("Не удалось получить данные текущего пользователя.", "Ошибка выбора группы", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string currentUserId = currentUser.DocumentId;

                // Проверяем, является ли пользователь забаненным
                if (group.BannedIds.Contains(currentUserId))
                {
                    MessageBox.Show("Группа недоступна, вы забанены.", "Бан", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Проверяем, является ли пользователь создателем или администратором группы
                bool isAdminOrCreator = group.IdCreator == currentUserId || group.AdminIds.Contains(currentUserId);

                // Блокируем или разблокируем элементы управления в зависимости от прав
                AddUserButton.IsEnabled = isAdminOrCreator;
                UsersListBox.IsEnabled = isAdminOrCreator;

                UpdateQuestionTabs(); // Обновляем вкладки с задачами
                                      // Обновляем список пользователей
                await LoadUsersForSelectedGroupAsync(groupId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка выбора группы", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                GroupsListBox.IsEnabled = true;
            }
            
        }
        private async void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddUserButton.IsEnabled = false;
                string selectedGroupName = GroupsListBox.SelectedItem as string;
                if (string.IsNullOrEmpty(selectedGroupName) || !_groupIdMapping.ContainsKey(selectedGroupName))
                {
                    MessageBox.Show("Пожалуйста выберите группу.", "Добавление пользователя", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string groupId = _groupIdMapping[selectedGroupName];

                // Проверяем, является ли текущий пользователь администратором или создателем группы
                var groupResult = await _groupService.GetGroupByIdAsync(groupId);
                var currentUserResult = await _userService.GetUserByEmailAsync(_email);
                var currentUserId = currentUserResult.DocumentId;

                var addUserDialog = new AddUser();
                if (addUserDialog.ShowDialog() == true)
                {
                    string email = addUserDialog.Email;
                    bool isAdmin = addUserDialog.Admin;

                    var userResult = await _userService.GetUserByEmailAsync(email);
                    if (userResult.Data == null)
                    {
                        MessageBox.Show("Пользователь не найден.", "Ошибка добавления пользоввателя", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    string userId = userResult.DocumentId;

                    if (groupResult.UserIds.Contains(userId))
                    {
                        MessageBox.Show("Пользователь уже состоит в этой группе.", "Добавление пользователя", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    // Добавляем пользователя в список участников группы
                    groupResult.UserIds.Add(userId);

                    // Если пользователь выбран как администратор, добавляем его в список администраторов
                    if (isAdmin)
                    {
                        if (!groupResult.AdminIds.Contains(userId))
                        {
                            groupResult.AdminIds.Add(userId);
                        }
                    }

                    // Обновляем группу в базе данных
                    await _groupService.UpdateGroupAsync(groupResult, groupId);

                    MessageBox.Show($"Пользователь '{email}' Добавлен в группу." + (isAdmin ? " Пользователю выданы права админа." : ""), "Добавление пользователя", MessageBoxButton.OK, MessageBoxImage.Information);

                    // Обновляем список пользователей в интерфейсе
                    await LoadUsersForSelectedGroupAsync(groupId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка добавления пользователя", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                AddUserButton.IsEnabled = true;
            }
            
        }
        private async Task LoadUsersForSelectedGroupAsync(string groupId)
        {
            try
            {
                var group = await _groupService.GetGroupByIdAsync(groupId);
                if (group == null || !group.UserIds.Any())
                {
                    UsersListBox.ItemsSource = null;
                    MessageBox.Show("Нет пользователей в данной группе.", "Ошибка показа пользователей", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var userNames = new List<string>();
                foreach (var userId in group.UserIds)
                {
                    var user = await _userService.GetUserByDocumentIdAsync(userId);
                    if (user != null)
                    {
                        userNames.Add(user.Name);
                    }
                }

                UsersListBox.ItemsSource = userNames;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка показа пользователей", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void AddTaskButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AddTaskButton.IsEnabled = false;
                string selectedGroupName = GroupsListBox.SelectedItem as string;
                if (string.IsNullOrEmpty(selectedGroupName) || !_groupIdMapping.ContainsKey(selectedGroupName))
                {
                    MessageBox.Show("Выберите группу.", "Создание задания", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string groupId = _groupIdMapping[selectedGroupName];
                var group = await _groupService.GetGroupByIdAsync(groupId);

                if (group == null || !group.UserIds.Any())
                {
                    MessageBox.Show("У группы нет пользователей.", "Ошибка создания задания", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var userNames = new List<string>();
                var userIds = new List<string>();

                foreach (var userId in group.UserIds)
                {
                    var user = await _userService.GetUserByDocumentIdAsync(userId);
                    if (user != null)
                    {
                        userNames.Add(user.Name);
                        userIds.Add(user.Id);
                    }
                }

                var addTaskWindow = new AddTaskWindow(userNames, userIds);
                if (addTaskWindow.ShowDialog() == true)
                {
                    // Получаем информацию о текущем пользователе
                    var currentUser = await _userService.GetUserByEmailAsync(_email);
                    if (currentUser.Data == null)
                    {
                        MessageBox.Show("Не удалось определить текущего пользователя.", "Ошибка создания задания", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var selectedAssignee = addTaskWindow.SelectedAssignee;

                    // Создаем новую задачу
                    var question = new Question
                    {
                        Description = addTaskWindow.TaskDescriptionTextBox.Text,
                        Date = addTaskWindow.DeadlineDatePicker.SelectedDate.HasValue
                            ? Google.Cloud.Firestore.Timestamp.FromDateTime(
                                DateTime.SpecifyKind(addTaskWindow.DeadlineDatePicker.SelectedDate.Value, DateTimeKind.Utc))
                            : (Google.Cloud.Firestore.Timestamp?)null,
                        GroupId = groupId,
                        CreatorId = currentUser.DocumentId,
                        Users = selectedAssignee == "All" ? group.UserIds : new List<string> { userIds[userNames.IndexOf(selectedAssignee)] },
                        Status = Status.Assign
                    };

                    var questionId = await _questionService.CreateQuestionAsync(question);
                    MessageBox.Show($"Задача успешно создана. ID: {questionId}", "Создание задания", MessageBoxButton.OK, MessageBoxImage.Information);
                    UpdateQuestionTabs();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка создания задания", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                AddTaskButton.IsEnabled = true;
            }
        }

        public async void UpdateQuestionTabs()
        {
            try
            {
                // Проверяем, выбрана ли группа
                string selectedGroupName = GroupsListBox.SelectedItem as string;
                if (string.IsNullOrEmpty(selectedGroupName) || !_groupIdMapping.ContainsKey(selectedGroupName))
                {
                    MessageBox.Show("Выберите группу.", "Обновление заданий", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                string groupId = _groupIdMapping[selectedGroupName];

                // Получаем актуальные задачи из базы данных
                var tasks = await _questionService.GetQuestionsForGroupAsync(groupId);

                // Очистка ListBox перед добавлением новых элементов
                groupTaskListBox.Items.Clear();
                CompletedTaskListbox.Items.Clear();
                FailedTaskListbox.Items.Clear();
                personalTaskListBox.Items.Clear();

                // Получаем текущего пользователя по email для получения его ID
                var currentUser = await _userService.GetUserByEmailAsync(_email);
                if (currentUser.Data == null)
                {
                    MessageBox.Show("Не удалось получить данные пользователя.", "Ошибка обновления задания", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string currentUserId = currentUser.DocumentId;
                DateTime today = DateTime.Today;

                // Заполнение ListBox UserControl'ами
                foreach (var task in tasks)
                {
                    // Если дата задания меньше сегодняшней, отмечаем задание как проваленное
                    if (task.Date.HasValue && task.Date.Value.ToDateTime() < today && task.Status != Status.Succes && task.Status != Status.Fail)
                    {
                        task.Status = Status.Fail;
                        await _questionService.UpdateQuestionAsync(task);
                    }

                    // Создаем новый UserControl для каждой задачи
                    var taskControl = new TaskControl(_userService, _questionService, _groupService, _email, groupId, task.Id, this)
                    {
                        DescriptionTextBlock = { Text = task.Description },
                        DeadlineTextBlock = { Text = task.Date.HasValue ? task.Date.Value.ToDateTime().ToString("dd MMM yyyy") : "Без срока" }
                    };

                    // Блокируем кнопки для завершенных или проваленных задач
                    if (task.Status == Status.Succes || task.Status == Status.Fail)
                    {
                        taskControl.CompleteButton.IsEnabled = false;
                        taskControl.FailButton.IsEnabled = false;
                    }

                    // Добавляем задачи в соответствующие ListBox в зависимости от их статуса
                    if (task.Status == Status.Succes)
                    {
                        CompletedTaskListbox.Items.Add(taskControl);
                    }
                    else if (task.Status == Status.Fail)
                    {
                        FailedTaskListbox.Items.Add(taskControl);
                    }
                    else
                    {
                        groupTaskListBox.Items.Add(taskControl);
                    }

                    // Добавляем задачи в PersonalTaskListBox, если текущий пользователь указан в задаче
                    if (task.Users.Contains(currentUserId)) // Проверка, содержит ли задача ID текущего пользователя
                    {
                        // Создаем новый экземпляр для личных задач
                        var personalTaskControl = new TaskControl(_userService, _questionService, _groupService, _email, groupId, task.Id, this)
                        {
                            DescriptionTextBlock = { Text = task.Description },
                            DeadlineTextBlock = { Text = task.Date.HasValue ? task.Date.Value.ToDateTime().ToString("dd MMM yyyy") : "Без срока" }
                        };

                        // Блокируем кнопки для завершенных или проваленных задач в персональной группе
                        if (task.Status == Status.Succes || task.Status == Status.Fail)
                        {
                            personalTaskControl.CompleteButton.IsEnabled = false;
                            personalTaskControl.FailButton.IsEnabled = false;
                        }

                        // Добавляем задачу в личный ListBox
                        personalTaskListBox.Items.Add(personalTaskControl);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка обновления задания", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void UsersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                UsersListBox.IsEnabled = false;
                string selectedGroupName = GroupsListBox.SelectedItem as string;

                string groupId = _groupIdMapping[selectedGroupName];
                string selectedUserName = UsersListBox.SelectedItem as string;


                var group = await _groupService.GetGroupByIdAsync(groupId);
                var currentUser = await _userService.GetUserByEmailAsync(_email);
                var selectedUser = await Task.Run(async () =>
                {
                    foreach (var userId in group.UserIds)
                    {
                        var user = await _userService.GetUserByDocumentIdAsync(userId);
                        if (user.Name == selectedUserName)
                        {
                            return userId;
                        }
                    }
                    return null;
                });

                if (selectedUser == null)
                {
                    MessageBox.Show("Пользователь не найден.", "Ошибка выбора пользователя", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var selectedUserData = await _userService.GetUserByDocumentIdAsync(selectedUser);

                // Определяем роли
                bool isCreator = group.IdCreator == currentUser.DocumentId;
                bool isAdmin = group.AdminIds.Contains(currentUser.DocumentId);
                bool isSelf = currentUser.DocumentId == selectedUserData.Id;

                var userDetailsWindow = new UserDetails();
                userDetailsWindow.txtId.Text = selectedUserData.Id;
                userDetailsWindow.txtName.Text = selectedUserData.Name;
                userDetailsWindow.txtEmail.Text = selectedUserData.Email;
                userDetailsWindow.chkAdmin.IsChecked = group.AdminIds.Contains(selectedUser);
                userDetailsWindow.chkBan.IsChecked = group.BannedIds.Contains(selectedUser);

                // Ограничиваем доступ к изменениям в зависимости от роли и выбора
                if (isSelf)
                {
                    userDetailsWindow.chkAdmin.IsEnabled = false;
                    userDetailsWindow.chkBan.IsEnabled = false;
                    userDetailsWindow.btnSave.IsEnabled = false;
                }
                else if (isCreator)
                {
                    userDetailsWindow.chkAdmin.IsEnabled = true;
                    userDetailsWindow.chkBan.IsEnabled = true;
                    userDetailsWindow.btnSave.IsEnabled = true;
                }
                else
                {
                    userDetailsWindow.chkAdmin.IsEnabled = false;
                    userDetailsWindow.chkBan.IsEnabled = false;
                    userDetailsWindow.btnSave.IsEnabled = false;
                }

                userDetailsWindow.btnSave.Click += async (s, args) =>
                {
                    userDetailsWindow.btnSave.IsEnabled = false;
                    if (isCreator)
                    {
                        // Обновление прав администратора
                        bool isNowAdmin = userDetailsWindow.chkAdmin.IsChecked == true;
                        if (isNowAdmin && !group.AdminIds.Contains(selectedUser))
                        {
                            group.AdminIds.Add(selectedUser);
                        }
                        else if (!isNowAdmin && group.AdminIds.Contains(selectedUser))
                        {
                            group.AdminIds.Remove(selectedUser);
                        }

                        // Обновление блокировки
                        bool isNowBanned = userDetailsWindow.chkBan.IsChecked == true;
                        if (isNowBanned && !group.BannedIds.Contains(selectedUser))
                        {
                            group.BannedIds.Add(selectedUser);
                        }
                        else if (!isNowBanned && group.BannedIds.Contains(selectedUser))
                        {
                            group.BannedIds.Remove(selectedUser);
                        }

                        await _groupService.UpdateGroupAsync(group, groupId);
                        MessageBox.Show("Изменения сохранены.", "Изменение данных пользователя", MessageBoxButton.OK, MessageBoxImage.Information);
                        userDetailsWindow.Close();
                    }
                };

                userDetailsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка выбора пользователя", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                UsersListBox.IsEnabled = true;
            }
            
        }

        private async void GroupsListBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                GroupsListBox.IsEnabled = false;
                string selectedGroupName = GroupsListBox.SelectedItem as string;
                if (string.IsNullOrEmpty(selectedGroupName) || !_groupIdMapping.ContainsKey(selectedGroupName))
                {
                    return;
                }

                string groupId = _groupIdMapping[selectedGroupName];
                var group = await _groupService.GetGroupByIdAsync(groupId);

                if (group == null)
                {
                    MessageBox.Show("Группа не найдена.", "Ошибка выбора группы", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var currentUser = await _userService.GetUserByEmailAsync(_email);
                if (currentUser.Data == null)
                {
                    MessageBox.Show("Не удалось получить данные текущего пользователя.", "Ошибка выбора группы", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string currentUserId = currentUser.DocumentId;

                // Проверяем, является ли пользователь забаненным
                if (group.BannedIds.Contains(currentUserId))
                {
                    MessageBox.Show("Группа недоступна, вы забанены.", "Бан", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                // Проверяем, является ли пользователь создателем или администратором группы
                bool isAdminOrCreator = group.IdCreator == currentUserId || group.AdminIds.Contains(currentUserId);

                // Блокируем или разблокируем элементы управления в зависимости от прав
                AddUserButton.IsEnabled = isAdminOrCreator;
                UsersListBox.IsEnabled = isAdminOrCreator;

                UpdateQuestionTabs(); // Обновляем вкладки с задачами
                                      // Обновляем список пользователей
                await LoadUsersForSelectedGroupAsync(groupId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка выбора группы", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                GroupsListBox.IsEnabled = true;
            }
            
        }

        private async void UsersListBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                UsersListBox.IsEnabled = false;
                string selectedGroupName = GroupsListBox.SelectedItem as string;
                if (string.IsNullOrEmpty(selectedGroupName) || !_groupIdMapping.ContainsKey(selectedGroupName))
                {
                    MessageBox.Show("Выберите группу", "Выбор пользователя", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                    
                string groupId = _groupIdMapping[selectedGroupName];
                string selectedUserName = UsersListBox.SelectedItem as string;
                if (string.IsNullOrEmpty(selectedUserName) || !_groupIdMapping.ContainsKey(selectedUserName))
                    return;

                var group = await _groupService.GetGroupByIdAsync(groupId);
                var currentUser = await _userService.GetUserByEmailAsync(_email);
                var selectedUser = await Task.Run(async () =>
                {
                    foreach (var userId in group.UserIds)
                    {
                        var user = await _userService.GetUserByDocumentIdAsync(userId);
                        if (user.Name == selectedUserName)
                        {
                            return userId;
                        }
                    }
                    return null;
                });

                if (selectedUser == null)
                {
                    MessageBox.Show("Пользователь не найден.", "Ошибка выбора пользователя", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var selectedUserData = await _userService.GetUserByDocumentIdAsync(selectedUser);

                // Определяем роли
                bool isCreator = group.IdCreator == currentUser.DocumentId;
                bool isAdmin = group.AdminIds.Contains(currentUser.DocumentId);
                bool isSelf = currentUser.DocumentId == selectedUserData.Id;

                var userDetailsWindow = new UserDetails();
                userDetailsWindow.txtId.Text = selectedUserData.Id;
                userDetailsWindow.txtName.Text = selectedUserData.Name;
                userDetailsWindow.txtEmail.Text = selectedUserData.Email;
                userDetailsWindow.chkAdmin.IsChecked = group.AdminIds.Contains(selectedUser);
                userDetailsWindow.chkBan.IsChecked = group.BannedIds.Contains(selectedUser);

                // Ограничиваем доступ к изменениям в зависимости от роли и выбора
                if (isSelf)
                {
                    userDetailsWindow.chkAdmin.IsEnabled = false;
                    userDetailsWindow.chkBan.IsEnabled = false;
                    userDetailsWindow.btnSave.IsEnabled = false;
                }
                else if (isCreator)
                {
                    userDetailsWindow.chkAdmin.IsEnabled = true;
                    userDetailsWindow.chkBan.IsEnabled = true;
                    userDetailsWindow.btnSave.IsEnabled = true;
                }
                else
                {
                    userDetailsWindow.chkAdmin.IsEnabled = false;
                    userDetailsWindow.chkBan.IsEnabled = false;
                    userDetailsWindow.btnSave.IsEnabled = false;
                }

                userDetailsWindow.btnSave.Click += async (s, args) =>
                {
                    userDetailsWindow.btnSave.IsEnabled = false;
                    if (isCreator)
                    {
                        // Обновление прав администратора
                        bool isNowAdmin = userDetailsWindow.chkAdmin.IsChecked == true;
                        if (isNowAdmin && !group.AdminIds.Contains(selectedUser))
                        {
                            group.AdminIds.Add(selectedUser);
                        }
                        else if (!isNowAdmin && group.AdminIds.Contains(selectedUser))
                        {
                            group.AdminIds.Remove(selectedUser);
                        }

                        // Обновление блокировки
                        bool isNowBanned = userDetailsWindow.chkBan.IsChecked == true;
                        if (isNowBanned && !group.BannedIds.Contains(selectedUser))
                        {
                            group.BannedIds.Add(selectedUser);
                        }
                        else if (!isNowBanned && group.BannedIds.Contains(selectedUser))
                        {
                            group.BannedIds.Remove(selectedUser);
                        }

                        await _groupService.UpdateGroupAsync(group, groupId);
                        MessageBox.Show("Изменения сохранены.", "Изменение данных пользователя", MessageBoxButton.OK, MessageBoxImage.Information);
                        userDetailsWindow.Close();
                    }
                };

                userDetailsWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка выбора пользователя", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                UsersListBox.IsEnabled = true;
            }
            
        }
    }
}
