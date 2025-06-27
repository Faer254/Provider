using Provider.classes;
using Provider.help_windows;
using Provider.windows;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Provider.pages_manager
{
    public partial class AccountSettings : Page
    {
        private readonly EmployeeWindow employeeWindow;
        public AccountSettings(EmployeeWindow window)
        {
            DataContext = new
            {
                WindowData = window.DataContext,
                ThemeToggle = window.themeToggle
            };
            InitializeComponent();
            employeeWindow = window;

            loadData();
        }

        private void loadData()
        {
            Manager.user = Manager.dataBase.getEmployee("id", Manager.user[0].ToString());

            if (Manager.user == null)
            {
                employeeWindow.loadEmployee();
            }

            loginLabel.Content = Manager.user[1];
            emailLabel.Content = Manager.user[2];
            fullNameLabel.Content = Manager.user[4];
            phoneLabel.Content = Manager.user[6];
        }

        private void editLoginButton_Click(object sender, RoutedEventArgs e)
        {
            newLoginTB.IsEnabled = true;
            newEmailTB.IsEnabled = false;
            acceptCodeTB.IsEnabled = false;
            newPhoneTB.IsEnabled = false;
            newPhoneTB.Mask = "";
            acceptCodePhoneTB.IsEnabled = false;

            newLoginTB.Text = (string)Manager.user[1];
            newEmailTB.Text = string.Empty;
            acceptCodeTB.Text = string.Empty;
            acceptCodePhoneTB.Text = string.Empty;

            saveLoginButton.IsEnabled = true;
            saveEmailButton.IsEnabled = false;
            savePhonenButton.IsEnabled = false;

            newLoginTB.Focus();
            newLoginTB.CaretIndex = newLoginTB.Text.Length;
        }

        private void editEmailButton_Click(object sender, RoutedEventArgs e)
        {
            newLoginTB.IsEnabled = false;
            newEmailTB.IsEnabled = true;
            isAcceptCodeExist = false;
            acceptCodeTB.IsEnabled = false;
            newPhoneTB.IsEnabled = false;
            newPhoneTB.Mask = "";
            acceptCodePhoneTB.IsEnabled = false;

            newLoginTB.Text = string.Empty;
            newEmailTB.Text = (string)Manager.user[2];
            acceptCodeTB.Text = string.Empty;
            acceptCodePhoneTB.Text = string.Empty;

            saveLoginButton.IsEnabled = false;
            saveEmailButton.IsEnabled = true;
            savePhonenButton.IsEnabled = false;

            newEmailTB.Focus();
            newEmailTB.CaretIndex = newEmailTB.Text.Length;
        }

        private void editPhoneButton_Click(object sender, RoutedEventArgs e)
        {
            newLoginTB.IsEnabled = false;
            newEmailTB.IsEnabled = false;
            acceptCodeTB.IsEnabled = false;
            newPhoneTB.IsEnabled = true;
            newPhoneTB.Mask = "+7 (\\900) 000-00-00";
            isAcceptCodePhoneExist = false;
            acceptCodePhoneTB.IsEnabled = false;

            newLoginTB.Text = string.Empty;
            newEmailTB.Text = string.Empty;
            acceptCodeTB.Text = string.Empty;
            newPhoneTB.Text = (string)Manager.user[6];
            acceptCodePhoneTB.Text = string.Empty;

            saveLoginButton.IsEnabled = false;
            saveEmailButton.IsEnabled = false;
            savePhonenButton.IsEnabled = true;

            newPhoneTB.Focus();
        }

        private void saveLoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (newLoginTB.Text == Manager.user[1] || newLoginTB.Text == string.Empty)
            {
                newLoginTB.IsEnabled = false;
                newLoginTB.Text = string.Empty;
                saveLoginButton.IsEnabled = false;
                return;
            }

            if (newLoginTB.Text.Contains(' '))
            {
                employeeWindow.ShowAndHideError("В логине не должно быть пробелов!");
                return;
            }

            string isLoginRegistered = Manager.dataBase.isRecordExists("employee", "login", newLoginTB.Text);

            if (isLoginRegistered == "true")
            {
                employeeWindow.ShowAndHideError("Данный логин уже занят!");
                return;
            }

            if (isLoginRegistered == "error")
            {
                employeeWindow.loadEmployee();
                return;
            }

            if (!Manager.dataBase.updateEmployee((uint)Manager.user[0], "login", newLoginTB.Text))
            {
                employeeWindow.loadEmployee();
                return;
            }

            Manager.dataBase.addEmployeeAction((uint)Manager.user[0], "Изменил логин", DateTime.Now);
            loadData();
            employeeWindow.loadEmployee();

            newLoginTB.IsEnabled = false;
            newLoginTB.Text = string.Empty;
            saveLoginButton.IsEnabled = false;
        }

        private readonly string pattern = "[.\\-_a-z0-9]+@([a-z0-9][\\-a-z0-9]+\\.)+[a-z]{2,6}";
        private string lastEmail;
        private bool isAcceptCodeExist = false;
        private int fourDigitNumber;
        private void saveEmailButton_Click(object sender, RoutedEventArgs e)
        {
            if (newEmailTB.Text == Manager.user[2] || newEmailTB.Text == string.Empty)
            {
                newEmailTB.IsEnabled = false;
                newEmailTB.Text = string.Empty;
                acceptCodeTB.IsEnabled = false;
                acceptCodeTB.Text = string.Empty;
                saveEmailButton.IsEnabled = false;
                return;
            }

            acceptCodeTB.IsEnabled = false;

            if (!Regex.Match(newEmailTB.Text, pattern, RegexOptions.IgnoreCase).Success || newEmailTB.Text.Contains(' '))
            {
                employeeWindow.ShowAndHideError("Некорректный e-mail адрес!");
                return;
            }

            string isEmailRegistered = Manager.dataBase.isRecordExists("employee", "email", newEmailTB.Text);

            if (isEmailRegistered == "true")
            {
                employeeWindow.ShowAndHideError("Данная электронная почта уже зарегистрирована!");
                return;
            }
            else if (isEmailRegistered == "error")
            {
                employeeWindow.loadEmployee();
                return;
            }

            isEmailRegistered = Manager.dataBase.isRecordExists("client", "email", newEmailTB.Text);

            if (isEmailRegistered == "true")
            {
                employeeWindow.ShowAndHideError("Данная электронная почта уже зарегистрирована!");
                return;
            }
            else if (isEmailRegistered == "error")
            {
                employeeWindow.loadEmployee();
                return;
            }

            acceptCodeTB.IsEnabled = true;
            acceptCodeTB.Focus();

            if (!isAcceptCodeExist)
            {
                lastEmail = newEmailTB.Text;

                Random random = new Random();
                fourDigitNumber = random.Next(1000, 10000);
                MessageBox.Show(Convert.ToString(fourDigitNumber), "Код подтверждения", MessageBoxButton.OK, MessageBoxImage.Information);
                isAcceptCodeExist = true;

                return;
            }

            if (lastEmail != newEmailTB.Text)
            {
                lastEmail = newEmailTB.Text;

                Random random = new Random();
                fourDigitNumber = random.Next(1000, 10000);
                MessageBox.Show(Convert.ToString(fourDigitNumber), "Код подтверждения", MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }

            if (acceptCodeTB.Text != fourDigitNumber.ToString())
            {
                employeeWindow.ShowAndHideError("Введён неверный код подтверждения!");

                Random random = new Random();
                fourDigitNumber = random.Next(1000, 10000);
                MessageBox.Show(Convert.ToString(fourDigitNumber), "Код подтверждения", MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }

            if (!Manager.dataBase.updateEmail("employee", (uint)Manager.user[0], newEmailTB.Text))
            {
                employeeWindow.loadEmployee();
                return;
            }

            Manager.dataBase.addEmployeeAction((uint)Manager.user[0], "Изменил почту", DateTime.Now);
            loadData();
            employeeWindow.loadEmployee();

            newEmailTB.IsEnabled = false;
            newEmailTB.Text = string.Empty;
            acceptCodeTB.IsEnabled = false;
            acceptCodeTB.Text = string.Empty;
            saveEmailButton.IsEnabled = false;
        }

        private string lastPhone;
        private bool isAcceptCodePhoneExist = false;
        private int fourDigitNumberPhone;

        private void savePhonenButton_Click(object sender, RoutedEventArgs e)
        {
            if (newPhoneTB.Text == Manager.user[6])
            {
                newPhoneTB.IsEnabled = false;
                newPhoneTB.Mask = "";
                savePhonenButton.IsEnabled = false;
                return;
            }

            acceptCodePhoneTB.IsEnabled = false;

            if (newPhoneTB.Text.Contains('_'))
            {
                employeeWindow.ShowAndHideError("Необходимо указать новый номер телефона!");
                return;
            }

            string isPhoneRegistered = Manager.dataBase.isRecordExists("employee", "phone_number", newPhoneTB.Text);

            if (isPhoneRegistered == "true")
            {
                employeeWindow.ShowAndHideError("Данный номер телефона уже занят!");
                return;
            }

            if (isPhoneRegistered == "error")
            {
                employeeWindow.loadEmployee();
                return;
            }

            acceptCodePhoneTB.IsEnabled = true;
            acceptCodePhoneTB.Focus();

            if (!isAcceptCodePhoneExist)
            {
                lastPhone = newPhoneTB.Text;

                Random random = new Random();
                fourDigitNumberPhone = random.Next(1000, 10000);
                MessageBox.Show(Convert.ToString(fourDigitNumberPhone), "Код подтверждения", MessageBoxButton.OK, MessageBoxImage.Information);
                isAcceptCodePhoneExist = true;

                return;
            }

            if (lastPhone != newPhoneTB.Text)
            {
                lastPhone = newPhoneTB.Text;

                Random random = new Random();
                fourDigitNumberPhone = random.Next(1000, 10000);
                MessageBox.Show(Convert.ToString(fourDigitNumberPhone), "Код подтверждения", MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }

            if (acceptCodePhoneTB.Text != fourDigitNumberPhone.ToString())
            {
                employeeWindow.ShowAndHideError("Введён неверный код подтверждения!");

                Random random = new Random();
                fourDigitNumberPhone = random.Next(1000, 10000);
                MessageBox.Show(Convert.ToString(fourDigitNumberPhone), "Код подтверждения", MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }

            if (!Manager.dataBase.updateEmployee((uint)Manager.user[0], "phone_number", newPhoneTB.Text))
            {
                employeeWindow.loadEmployee();
                return;
            }

            Manager.dataBase.addEmployeeAction((uint)Manager.user[0], "Изменил номер телефона", DateTime.Now);
            loadData();
            employeeWindow.loadEmployee();

            newPhoneTB.IsEnabled = false;
            newPhoneTB.Mask = "";
            acceptCodePhoneTB.IsEnabled = false;
            acceptCodePhoneTB.Text = string.Empty;
            savePhonenButton.IsEnabled = false;
        }

        private void saveSafetySettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (oldPasswordPB.Password == string.Empty || newPasswordPB.Password == string.Empty || newPasswordAgainPB.Password == string.Empty)
            {
                employeeWindow.ShowAndHideError("Необходимо заполнить все поля!");
                return;
            }

            if (newPasswordPB.Password.Length <= 5)
            {
                employeeWindow.ShowAndHideError("Пароль должен быть не меньше 6 символов!");
                return;
            }

            if (newPasswordPB.Password.Contains(' '))
            {
                employeeWindow.ShowAndHideError("В пароле не должно быть пробелов!");
                return;
            }

            if (newPasswordPB.Password != newPasswordAgainPB.Password)
            {
                employeeWindow.ShowAndHideError("Подтверждение пароля не совпадает!");
                return;
            }

            string isPasswordCorrect = Manager.dataBase.checkPassword("employee", (uint)Manager.user[0], oldPasswordPB.Password);

            if (isPasswordCorrect == "false")
            {
                employeeWindow.ShowAndHideError("Введён неверный пароль!");
                return;
            }
            else if (isPasswordCorrect == "error")
            {
                employeeWindow.loadEmployee();
                return;
            }

            if (!Manager.dataBase.updatePassword("employee", (uint)Manager.user[0], newPasswordPB.Password))
            {
                employeeWindow.loadEmployee();
                return;
            }

            Manager.dataBase.addEmployeeAction((uint)Manager.user[0], "Изменил пароль", DateTime.Now);

            new SuccesWindow("Пароль успешно изменён", Manager.theme).ShowDialog();

            oldPasswordPB.Password = string.Empty;
            newPasswordPB.Password = string.Empty;
            newPasswordAgainPB.Password = string.Empty;

            saveSafetySettingsButton.Focus();

            loadData();
            employeeWindow.loadEmployee();
        }

        private void Page_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Return when newLoginTB.IsFocused:
                    saveLoginButton_Click(sender, e);
                    break;

                case Key.Return when newEmailTB.IsFocused:
                    saveEmailButton_Click(sender, e);
                    break;

                case Key.Return when acceptCodeTB.IsFocused:
                    saveEmailButton_Click(sender, e);
                    break;

                case Key.Return when newPhoneTB.IsFocused:
                    savePhonenButton_Click(sender, e);
                    break;

                case Key.Return when acceptCodePhoneTB.IsFocused:
                    savePhonenButton_Click(sender, e);
                    break;



                case Key.Return when oldPasswordPB.IsFocused:
                    newPasswordPB.Focus();
                    break;

                case Key.Down when oldPasswordPB.IsFocused:
                    newPasswordPB.Focus();
                    break;

                case Key.Return when newPasswordPB.IsFocused:
                    newPasswordAgainPB.Focus();
                    break;

                case Key.Down when newPasswordPB.IsFocused:
                    newPasswordAgainPB.Focus();
                    break;

                case Key.Up when newPasswordPB.IsFocused:
                    oldPasswordPB.Focus();
                    break;

                case Key.Return when newPasswordAgainPB.IsFocused:
                    saveSafetySettingsButton_Click(sender, e);
                    break;

                case Key.Up when newPasswordAgainPB.IsFocused:
                    newPasswordPB.Focus();
                    break;
            }
        }
    }
}
