using MaterialDesignThemes.Wpf;
using Provider.classes;
using Provider.help_windows;
using Provider.windows_employee;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Provider.windows
{
    public partial class EmployeeWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private readonly PaletteHelper paletteHelper = new PaletteHelper();

        public EmployeeWindow()
        {
            InitializeComponent();

            var workArea = SystemParameters.WorkArea;
            MaxHeight = workArea.Height + 15;

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };

            _timer.Tick += UpdateLocalTime;

            _timer.Start();
            UpdateLocalTime(null, null);

            checkTheme();
            loadEmployee();

            switch ((uint)Manager.user[5])
            {
                case 1:
                    workFrame.Navigate(new pages_admin.ServicesTable(this));
                    tableNameLabel.Content = "Таблица услуг";
                    break;
                case 2:
                    workFrame.Navigate(new pages_manager.Services(this));
                    tableNameLabel.Content = "Таблица услуг";
                    break;
            }
        }
        private void UpdateLocalTime(object sender, EventArgs e)
        {
            var now = DateTime.Now;

            timeLabel.Content = now.ToString("HH:mm");
            dateLabel.Content = now.ToString("dd.MM.yyyy");
        }

        protected override void OnClosed(EventArgs e)
        {
            _timer.Stop();
            base.OnClosed(e);
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            Border border = sender as Border;

            var clip = new RectangleGeometry
            {
                RadiusX = border.CornerRadius.TopLeft,
                RadiusY = border.CornerRadius.TopLeft,
                Rect = new Rect(0, 0, border.ActualWidth, border.ActualHeight)
            };
            border.Clip = clip;

            border.SizeChanged += (s, args) =>
            {
                clip.Rect = new Rect(0, 0, border.ActualWidth, border.ActualHeight);
            };
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(this);
            var screenPosition = this.PointToScreen(mousePosition);
            double relativeX = mousePosition.X / ActualWidth;

            if (mousePosition.Y <= 26)
            {
                if (WindowState == WindowState.Maximized)
                {
                    WindowState = WindowState.Normal;

                    var point = PointToScreen(mousePosition);

                    
                    Left = screenPosition.X - (relativeX * ActualWidth);
                    Top = screenPosition.Y - 13;

                    DragMove();
                }
                else
                {
                    DragMove();
                }
            }
        }

        private void checkTheme()
        {
            ITheme theme = paletteHelper.GetTheme();

            if (Manager.theme == "light")
            {
                theme.SetBaseTheme(Theme.Light);
                themeToggle.IsChecked = false;
            }
            else
            {
                theme.SetBaseTheme(Theme.Dark);
                themeToggle.IsChecked = true;
            }

            paletteHelper.SetTheme(theme);
        }

        private void themeToggle_Click(object sender, RoutedEventArgs e)
        {
            ITheme theme = paletteHelper.GetTheme();

            if (theme.GetBaseTheme() == BaseTheme.Dark)
            {
                theme.SetBaseTheme(Theme.Light);
                Manager.theme = "light";
                File.WriteAllText("resources/theme.dat", "light");
            }
            else
            {
                theme.SetBaseTheme(Theme.Dark);
                Manager.theme = "dark";
                File.WriteAllText("resources/theme.dat", "dark");
            }

            paletteHelper.SetTheme(theme);
        }

        private CancellationTokenSource _cancellationTokenSource;
        public async void ShowAndHideError(string errorName)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            var showError = (Storyboard)FindResource("ShowError");
            var hideError = (Storyboard)FindResource("HideError");

            errorText.Text = errorName;
            errorLabel.Visibility = Visibility.Visible;
            showError.Begin(errorLabel);

            try
            {
                await Task.Delay(2500, cancellationToken);
                hideError.Begin(errorLabel);

                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource = new CancellationTokenSource();
                cancellationToken = _cancellationTokenSource.Token;
                await Task.Delay(2500, cancellationToken);
                errorLabel.Visibility = Visibility.Collapsed;
            }
            catch
            {
            }
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void maximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                var workArea = SystemParameters.WorkArea;
                MaxHeight = workArea.Height + 15;
                WindowState = WindowState.Maximized;
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            var workArea = SystemParameters.WorkArea;
            MaxHeight = workArea.Height + 15;

            if(WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private bool isFirstLoad = true;
        public void loadEmployee()
        {
            if (!isFirstLoad)
            {
                Manager.user = Manager.dataBase.getEmployee("id", Manager.user[0].ToString());

                if (Manager.user == null)
                {
                    new AuthorizationWindow().Show();
                    Close();
                    return;
                }
            }

            isFirstLoad = false;

            switch ((uint)Manager.user[5])
            {
                case 1:
                    loadAdmin();
                    break;
                case 2:
                    loadManager();
                    break;
            }
        }

        private void loadAdmin()
        {
            adminMenuItem.Visibility = Visibility.Visible;
            managerMenuItem.Visibility = Visibility.Collapsed;

            List<string> fullName = Manager.user[4].ToString().Split(' ').ToList();
            adminNameLabel.Content = $"{fullName[0]} {fullName[1][0]}. {fullName[2][0]}.";
        }

        private void loadManager()
        {
            adminMenuItem.Visibility = Visibility.Collapsed;
            managerMenuItem.Visibility = Visibility.Visible;

            List<string> fullName = Manager.user[4].ToString().Split(' ').ToList();
            managerNameLabel.Content = $"{fullName[0]} {fullName[1][0]}. {fullName[2][0]}.";
        }

        private void exitProileButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("resources/data.sav"))
                File.Delete("resources/data.sav");

            Manager.user = null;
            new AuthorizationWindow().Show();
            Close();
        }

        private void adminAccountSettings(object sender, RoutedEventArgs e)
        {
            workFrame.Navigate(new pages_admin.AccountSettings(this));
            tableNameLabel.Content = "Настройки профиля";
        }

        private void managerAccountSettings(object sender, RoutedEventArgs e)
        {
            workFrame.Navigate(new pages_manager.AccountSettings(this));
            tableNameLabel.Content = "Настройки профиля";
        }

        private void employeeLog(object sender, RoutedEventArgs e)
        {
            workFrame.Navigate(new pages_admin.LogEmployee(this));
            tableNameLabel.Content = "Журнал активности";
        }

        private void clientLog(object sender, RoutedEventArgs e)
        {
            workFrame.Navigate(new pages_admin.LogClient(this));
            tableNameLabel.Content = "Журнал активности";
        }

        private void employeeTable(object sender, RoutedEventArgs e)
        {
            workFrame.Navigate(new pages_admin.Employee(this));
            tableNameLabel.Content = "Таблица сотрудников";
        }

        private void clientTableAdmin(object sender, RoutedEventArgs e)
        {
            workFrame.Navigate(new pages_admin.Client(this));
            tableNameLabel.Content = "Таблица клиентов";
        }

        private void clientTableManager(object sender, RoutedEventArgs e)
        {
            workFrame.Navigate(new pages_manager.Client(this));
            tableNameLabel.Content = "Таблица клиентов";
        }

        private void concludedServicesAdmin(object sender, RoutedEventArgs e)
        {
            workFrame.Navigate(new pages_admin.ConcludedServices(this));
            tableNameLabel.Content = "Таблица подключённых услуг";
        }

        private void concludedServicesManager(object sender, RoutedEventArgs e)
        {
            workFrame.Navigate(new pages_manager.ConcludedServices(this));
            tableNameLabel.Content = "Таблица подключённых услуг";
        }

        private void servicesAdmin(object sender, RoutedEventArgs e)
        {
            workFrame.Navigate(new pages_admin.ServicesTable(this));
            tableNameLabel.Content = "Таблица услуг";
        }

        private void servicesManager(object sender, RoutedEventArgs e)
        {
            workFrame.Navigate(new pages_manager.Services(this));
            tableNameLabel.Content = "Таблица услуг";
        }

        private void walletReplenishmentReport(object sender, RoutedEventArgs e)
        {
            new ReplenishmentsDataSelect().ShowDialog();
        }

        private void aboutUsButton_Click(object sender, RoutedEventArgs e)
        {
            new AboutUsWindow().ShowDialog();
        }

        private void aboutProgramButton_Click(object sender, RoutedEventArgs e)
        {
            new AboutProgramWindow().ShowDialog();
        }
    }
}
