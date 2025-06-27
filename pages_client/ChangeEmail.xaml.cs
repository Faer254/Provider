using Provider.classes;
using Provider.help_windows;
using Provider.windows;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Provider.pages_client
{
    public partial class ChangeEmail : Page
    {
        private readonly ClientWindow clientWindow;
        private int phoneFourDigitNumber;

        public ChangeEmail(ClientWindow window)
        {
            InitializeComponent();
            clientWindow = window;

            Random random = new Random();
            phoneFourDigitNumber = random.Next(1000, 10000);
            MessageBox.Show(Convert.ToString(phoneFourDigitNumber), "SMS подтверждение", MessageBoxButton.OK, MessageBoxImage.Information);

            smsAcceptCodeTB.Focus();
        }

        private readonly string pattern = "[.\\-_a-z0-9]+@([a-z0-9][\\-a-z0-9]+\\.)+[a-z]{2,6}";
        private string lastEmail;
        private bool isAcceptCodeExist = false;
        private int fourDigitNumber;

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            acceptCodeTB.IsEnabled = false;

            if (smsAcceptCodeTB.Text != phoneFourDigitNumber.ToString())
            {
                clientWindow.ShowAndHideError("Введён неверный SMS код!");
                return;
            }

            if (emailTB.Text == string.Empty)
            {
                clientWindow.ShowAndHideError("Необходимо указать новый e-mail адрес!");
                return;
            }

            if (!Regex.Match(emailTB.Text, pattern, RegexOptions.IgnoreCase).Success || emailTB.Text.Contains(' '))
            {
                clientWindow.ShowAndHideError("Некорректный e-mail адрес!");
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

            if (!Manager.dataBase.updateEmail("client", (uint)Manager.user[0], emailTB.Text))
            {
                clientWindow.loadCLient();
                return;
            }

            Manager.dataBase.addClientAction((uint)Manager.user[0], "Изменил почту", DateTime.Now);

            new SuccesWindow("Электронная почта успешно изменена", Manager.theme).ShowDialog();

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
                case Key.Return when smsAcceptCodeTB.IsFocused:
                    emailTB.Focus();
                    break;

                case Key.Down when smsAcceptCodeTB.IsFocused:
                    emailTB.Focus();
                    break;

                case Key.Return when emailTB.IsFocused && !acceptCodeTB.IsEnabled:
                    saveButton_Click(sender, e);
                    break;

                case Key.Return when emailTB.IsFocused && acceptCodeTB.IsEnabled:
                    acceptCodeTB.Focus();
                    break;
                
                case Key.Down when emailTB.IsFocused && acceptCodeTB.IsEnabled:
                    acceptCodeTB.Focus();
                    break;

                case Key.Up when emailTB.IsFocused:
                    smsAcceptCodeTB.Focus();
                    break;

                case Key.Up when acceptCodeTB.IsFocused:
                    emailTB.Focus();
                    break;

                case Key.Return when acceptCodeTB.IsFocused:
                    saveButton_Click(sender, e);
                    break;
            }
        }
    }
}
