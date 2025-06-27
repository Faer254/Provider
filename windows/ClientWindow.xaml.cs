using MaterialDesignThemes.Wpf;
using Provider.classes;
using Provider.help_windows;
using Provider.pages_client;
using Provider.windows_client;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Provider.windows
{
    public partial class ClientWindow : Window
    {
        private readonly DispatcherTimer _timer;
        private readonly PaletteHelper paletteHelper = new PaletteHelper();

        public ClientWindow()
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
            loadCLient();
        }

        private void UpdateLocalTime(object sender, EventArgs e)
        {
            var now = DateTime.Now;

            timeLabel.Content = now.ToString("HH:mm");
            dateLabel.Content = now.ToString("dd.MM.yyyy");

            greetingLabel.Content = GetTimeBasedGreeting(now);
        }

        private string GetTimeBasedGreeting(DateTime time)
        {
            int hour = time.Hour;

            if (hour >= 5 && hour < 12) return "Доброе утро,";
            if (hour >= 12 && hour < 17) return "Добрый день,";
            if (hour >= 17 && hour < 23) return "Добрый вечер,";
            return "Доброй ночи,";
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

        private bool isFirstLoad = true;
        public void loadCLient()
        {
            if (!isFirstLoad)
            {
                Manager.user = Manager.dataBase.getClient("id", Manager.user[0].ToString());

                if (Manager.user == null)
                {
                    new AuthorizationWindow().Show();
                    Close();
                    return;
                }
            }
            
            Manager.user.Add(true);

            isFirstLoad = false;

            nextPayentPanel.Visibility = Visibility.Collapsed;

            phoneLabel.Content = Manager.user[1];
            moneyLabel.Content = Manager.user[5];

            if ((int)Manager.user[6] < 0)
                internetLabel.Content = "Безлимит";
            else
                internetLabel.Content = Manager.user[6];

            if ((int)Manager.user[7] < 0)
                phoneMinLabel.Content = "Безлимит";
            else
                phoneMinLabel.Content = Manager.user[7];

            if ((int)Manager.user[8] < 0)
                smsLabel.Content = "Безлимит";
            else
                smsLabel.Content = Manager.user[8];
            
            if (Manager.user[2] == null)
            {
                securityFrame.Navigate(new AskSecurity(this, securityFrame));
            }
            else
            {
                securityFrame.Navigate(new AskSecurityChange(this, securityFrame));
            }

            List<string> fullName = Manager.user[4].ToString().Split(' ').ToList();
            nameGreetingLabel.Content = fullName[1];
            fullNameLabel.Content = $"{fullName[0]} {fullName[1][0]}. {fullName[2][0]}.";

            List<List<object>> clientServices = Manager.dataBase.getClientServices((uint)Manager.user[0]);
            tarifNameLabel.Content = "Отсутствует";
            tarifCostLabel.Content = "0";

            float costToPay = 0;
            DateTime nearestDate = new DateTime(9999, 12, 31);
            bool isTarifWrited = false;

            if ((uint)Manager.user[3] == 2)
            {
                Manager.user[9] = false;
                foreach (var service in clientServices)
                {
                    if ((uint)service[4] == 1 && !isTarifWrited)
                    {
                        isTarifWrited = true;
                        tarifNameLabel.Content = service[5].ToString();
                        tarifCostLabel.Content = service[6].ToString();
                    }
                    else if ((uint)service[4] != 1 && !isTarifWrited)
                    {
                        tarifNameLabel.Content = "Отсутствует";
                        tarifCostLabel.Content = "0";
                    }

                    if (service[2] == null)
                        continue;

                    if (DateTime.Now >= (DateTime)service[2])
                    {
                        costToPay += (float)service[6];
                    }
                }

                costToPay -= (float)Manager.user[5];

                if (costToPay > 0)
                {
                    nextPayentPanel.Visibility = Visibility.Visible;
                    nextPaymentLabel.Content = $"Пополните счёт на {costToPay}";

                    Manager.dataBase.completeCommand($"UPDATE `client` SET `status_id` = '2' WHERE `client`.`id` = {Manager.user[0]};");

                    ShowAndHideError("Недостаточно средств на счету!\nПополните счёт или отключите ненужные услуги");
                    return;
                }

                foreach (var service in clientServices)
                {
                    if (service[2] == null)
                        continue;

                    if (DateTime.Now >= (DateTime)service[2])
                    {
                        float currentBalance = (float)Manager.user[5];
                        currentBalance -= (float)service[6];
                        Manager.user[5] = currentBalance;
                        Manager.dataBase.completeCommand($"UPDATE `concluded_services` SET `next_payment`='{DateTime.Today.AddMonths(1).ToString("yyyy.MM.dd")}' WHERE `id`={service[0]}");
                    }
                }

                Manager.dataBase.completeCommand($"UPDATE `client` SET `status_id`=1, `wallet_account`={Manager.user[5]} WHERE `id`={Manager.user[0]}");
                loadCLient();
                return;
            }

            foreach (var service in clientServices)
            {
                if (service[2] == null)
                    continue;

                if (DateTime.Now >= (DateTime)service[2])
                {
                    costToPay += (float)service[6];
                }
            }

            if (costToPay > 0)
            {
                Manager.dataBase.completeCommand($"UPDATE `client` SET `status_id`='2' WHERE `client`.`id` = {Manager.user[0]};");

                loadCLient();
                return;
            }

            foreach (var service in clientServices)
            {
                if ((uint)service[4] == 1 && !isTarifWrited)
                {
                    isTarifWrited = true;
                    tarifNameLabel.Content = service[5].ToString();
                    tarifCostLabel.Content = service[6].ToString();
                }
                else if ((uint)service[4] != 1 && !isTarifWrited)
                {
                    tarifNameLabel.Content = "Отсутствует";
                    tarifCostLabel.Content = "0";
                }

                if (service[2] == null)
                    continue;

                DateTime nextPaymentDate = (DateTime)service[2];

                if (nearestDate.ToString("dd.MM.yy") == "31.12.99" || nextPaymentDate < nearestDate)
                {
                    nearestDate = nextPaymentDate;
                } 
            }

            foreach (var service in clientServices)
            {
                if (nearestDate.ToString("dd.MM.yy") == "31.12.99")
                    break;

                if (service[2] == null)
                    continue;

                DateTime date = (DateTime)service[2];

                if (nearestDate == date)
                {
                    costToPay += (float)service[6];
                }
            }

            if (costToPay != 0)
            {
                nextPayentPanel.Visibility = Visibility.Visible;
                nextPaymentLabel.Content = $"Списание {nearestDate.ToString("d MMMM")}, {costToPay}";
            }
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

            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Maximized;
            }
        }

        private void minimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void exitProileButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists("resources/data.sav"))
                File.Delete("resources/data.sav");

            Manager.user = null;
            new AuthorizationWindow().Show();
            Close();
        }

        private void addMoneyButton_Click(object sender, RoutedEventArgs e)
        {
            new AddMoneyWindow().ShowDialog();
            loadCLient();
        }

        private int animationDuration = 300;
        public async void showWorkFrame()
        {
            var showElement = (Storyboard)FindResource("ShowElement");
            var hideElement = (Storyboard)FindResource("HideElement");

            workFrame.Visibility = Visibility.Visible;
            showElement.Begin(workFrame);
            hideElement.Begin(workButtonsGrid);

            await Task.Delay(animationDuration);

            workButtonsGrid.Visibility = Visibility.Collapsed;
        }

        public async void hideWorkFrame()
        {
            var showElement = (Storyboard)FindResource("ShowElement");
            var hideElement = (Storyboard)FindResource("HideElement");

            workButtonsGrid.Visibility = Visibility.Visible;
            showElement.Begin(workButtonsGrid);
            hideElement.Begin(workFrame);

            await Task.Delay(animationDuration);

            workFrame.Visibility = Visibility.Collapsed;
        }

        private void changeTarifButton_Click(object sender, RoutedEventArgs e)
        {
            workFrame.Navigate(new Tarifs(this));
            showWorkFrame();
        }

        private void homeInternetButton_Click(object sender, RoutedEventArgs e)
        {
            workFrame.Navigate(new HomeInternet(this));
            showWorkFrame();
        }

        private void homeTVButton_Click(object sender, RoutedEventArgs e)
        {
            workFrame.Navigate(new HomeTV(this));
            showWorkFrame();
        }

        private void freeServicesButton_Click(object sender, RoutedEventArgs e)
        {
            workFrame.Navigate(new FreeServices(this));
            showWorkFrame();
        }

        private void paidServicesButton_Click(object sender, RoutedEventArgs e)
        {
            workFrame.Navigate(new PaidServices(this));
            showWorkFrame();
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
