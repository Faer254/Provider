using Provider.classes;
using Provider.windows;
using System.IO;
using System.Windows;

namespace Provider
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Manager.dataBase = new DataBase();
            Manager.reports = new Reports();

            if (!Directory.Exists("resources"))
                Directory.CreateDirectory("resources");

            InitializeTheme();
            SelectStartupWindow();
        }

        private void InitializeTheme()
        {
            const string themeFilePath = "resources/theme.dat";
            Manager.theme = File.Exists(themeFilePath) ? File.ReadAllText(themeFilePath) : "light";

            if (!File.Exists(themeFilePath))
                File.WriteAllText(themeFilePath, Manager.theme);
        }

        private void SelectStartupWindow()
        {
            const string userFilePath = "resources/data.sav";

            if (!File.Exists(userFilePath))
            {
                new AuthorizationWindow().Show();
                return;
            }

            List<string>? savedUser = Encrypter.DecryptListFromBytes_AES(File.ReadAllBytes(userFilePath));
            if (savedUser == null)
            {
                File.Delete(userFilePath);
                new AuthorizationWindow().Show();
                return;
            }

            switch (savedUser[0])
            {
                case "client":
                    Manager.user = Manager.dataBase.getClient("id", savedUser[1]);

                    if (Manager.user == null)
                    {
                        new AuthorizationWindow().Show();
                        return;
                    }

                    new ClientWindow().Show();
                    break;

                case "employee":
                    Manager.user = Manager.dataBase.getEmployee("id", savedUser[1]);

                    if (Manager.user == null)
                    {
                        new AuthorizationWindow().Show();
                        return;
                    }

                    if ((uint)Manager.user[3] == 2)
                    {
                        File.Delete(userFilePath);
                        Manager.user = [];
                        new AuthorizationWindow().Show();
                        return;
                    }

                    new EmployeeWindow().Show();
                    break;
            }
        }
    }
}
