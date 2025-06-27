using Provider.classes;
using Provider.help_windows;
using Provider.windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Provider.pages_client
{
    public partial class ChangePassword : Page
    {
        private readonly ClientWindow clientWindow;

        public ChangePassword(ClientWindow window)
        {
            InitializeComponent();
            clientWindow = window;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            if (oldPasswordPB.Password == string.Empty || passwordPB.Password == string.Empty || acceptPasswordPB.Password == string.Empty)
            {
                clientWindow.ShowAndHideError("Необходимо заполнить все поля!");
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

            string isPasswordCorrect = Manager.dataBase.checkPassword("client", (uint)Manager.user[0], oldPasswordPB.Password);

            if (isPasswordCorrect == "false")
            {
                clientWindow.ShowAndHideError("Введён неверный пароль!");
                return;
            }
            else if (isPasswordCorrect == "error")
            {
                clientWindow.loadCLient();
                return;
            }

            if (!Manager.dataBase.updatePassword("client", (uint)Manager.user[0], passwordPB.Password))
            {
                clientWindow.loadCLient();
                return;
            }

            Manager.dataBase.addClientAction((uint)Manager.user[0], "Изменил пароль", DateTime.Now);

            new SuccesWindow("Пароль успешно изменён", Manager.theme).ShowDialog();

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
                case Key.Return when oldPasswordPB.IsFocused:
                    passwordPB.Focus();
                    break;

                case Key.Down when oldPasswordPB.IsFocused:
                    passwordPB.Focus();
                    break;

                case Key.Return when passwordPB.IsFocused:
                    acceptPasswordPB.Focus();
                    break;

                case Key.Down when passwordPB.IsFocused:
                    acceptPasswordPB.Focus();
                    break;

                case Key.Up when passwordPB.IsFocused:
                    oldPasswordPB.Focus();
                    break;

                case Key.Return when acceptPasswordPB.IsFocused:
                    saveButton_Click(sender, e);
                    break;

                case Key.Up when acceptPasswordPB.IsFocused:
                    passwordPB.Focus();
                    break;
            }
        }
    }
}
