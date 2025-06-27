using Provider.classes;
using Provider.help_windows;
using Provider.windows;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Provider.pages_client
{
    public partial class AddSecurity : Page
    {
        private readonly ClientWindow clientWindow;

        public AddSecurity(ClientWindow window)
        {
            InitializeComponent();
            clientWindow = window;
        }

        private readonly string pattern = "[.\\-_a-z0-9]+@([a-z0-9][\\-a-z0-9]+\\.)+[a-z]{2,6}";
        private string lastEmail;
        private bool isAcceptCodeExist = false;
        private int fourDigitNumber;

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            acceptCodeTB.IsEnabled = false;

            if (emailTB.Text == string.Empty || passwordPB.Password == string.Empty || acceptPasswordPB.Password == string.Empty)
            {
                clientWindow.ShowAndHideError("Необходимо заполнить все поля!");
                return;
            }

            if (!Regex.Match(emailTB.Text, pattern, RegexOptions.IgnoreCase).Success)
            {
                clientWindow.ShowAndHideError("Некорректный e-mail адрес!");
                return;
            }

            if (passwordPB.Password.Length <= 5)
            {
                clientWindow.ShowAndHideError("Пароль должен быть не меньше 6 символов!");
                return;
            }

            if (passwordPB.Password.Contains(' '))
            {
                clientWindow.ShowAndHideError("В пароле не должно быть пробелов!");
                return;
            }

            if (passwordPB.Password != acceptPasswordPB.Password)
            {
                clientWindow.ShowAndHideError("Подтверждение пароля не совпадает!");
                return;
            }

            string isEmailRegistered = Manager.dataBase.isRecordExists("client", "email", emailTB.Text);

            if (isEmailRegistered == "true")
            {
                clientWindow.ShowAndHideError("Данная электронная почта уже зарегистрирована!");
                return;
            }
            else if (isEmailRegistered == "error")
            {
                clientWindow.loadCLient();
                return;
            }

            isEmailRegistered = Manager.dataBase.isRecordExists("employee", "email", emailTB.Text);

            if (isEmailRegistered == "true")
            {
                clientWindow.ShowAndHideError("Данная электронная почта уже зарегистрирована!");
                return;
            }
            else if (isEmailRegistered == "error")
            {
                clientWindow.loadCLient();
                return;
            }

            acceptCodeTB.IsEnabled = true;
            acceptCodeTB.Focus();

            if (!isAcceptCodeExist)
            {
                lastEmail = emailTB.Text;

                Random random = new Random();
                fourDigitNumber = random.Next(1000, 10000);
                MessageBox.Show(Convert.ToString(fourDigitNumber), "Код подтверждения", MessageBoxButton.OK, MessageBoxImage.Information);
                isAcceptCodeExist = true;

                return;
            }

            if (lastEmail != emailTB.Text)
            {
                lastEmail = emailTB.Text;

                Random random = new Random();
                fourDigitNumber = random.Next(1000, 10000);
                MessageBox.Show(Convert.ToString(fourDigitNumber), "Код подтверждения", MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }

            if (acceptCodeTB.Text != fourDigitNumber.ToString())
            {
                clientWindow.ShowAndHideError("Введён неверный код подтверждения!");

                Random random = new Random();
                fourDigitNumber = random.Next(1000, 10000);
                MessageBox.Show(Convert.ToString(fourDigitNumber), "Код подтверждения", MessageBoxButton.OK, MessageBoxImage.Information);

                return;
            }

            if (!Manager.dataBase.updateEmail("client", (uint)Manager.user[0], emailTB.Text) ||
                !Manager.dataBase.updatePassword("client", (uint)Manager.user[0], passwordPB.Password))
            {
                clientWindow.loadCLient();
                return;
            }

            Manager.dataBase.addClientAction((uint)Manager.user[0], "Добавил почту и пароль", DateTime.Now);

            new SuccesWindow("Электронная почта и пароль успешно добавлены", Manager.theme).ShowDialog();

            clientWindow.loadCLient();
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            clientWindow.loadCLient();
        }

        private void enter_Click(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Return when emailTB.IsFocused:
                    passwordPB.Focus();
                    break;

                case Key.Down when emailTB.IsFocused:
                    passwordPB.Focus();
                    break;

                case Key.Return when passwordPB.IsFocused:
                    acceptPasswordPB.Focus();
                    break;

                case Key.Down when passwordPB.IsFocused:
                    acceptPasswordPB.Focus();
                    break;

                case Key.Up when passwordPB.IsFocused:
                    emailTB.Focus();
                    break;

                case Key.Return when acceptPasswordPB.IsFocused:
                    addButton_Click(sender, e);
                    break;

                case Key.Up when acceptPasswordPB.IsFocused:
                    passwordPB.Focus();
                    break;

                case Key.Return when acceptCodeTB.IsFocused:
                    addButton_Click(sender, e);
                    break;
            }
        }
    }
}
