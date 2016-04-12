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
using desktop.Pages;

namespace desktop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal partial class MainWindow : Window
    {
        private ServerConnector mConnector = null;

        private IUpdatablePage _currentPage = null;

        public MainWindow()
        {
            InitializeComponent();
            string url = Properties.Settings.Default["host"].ToString();
            int port = Convert.ToInt32(Properties.Settings.Default["port"]);
            mConnector = new ServerConnector(url, port);
            _pageViewer.NavigationService.Navigate(new LoginPage(mConnector));
            _pageViewer.NavigationService.Navigated += NavigationCompleted;
        }

        void NavigationCompleted(object sender, NavigationEventArgs e)
        {
            IUpdatablePage item = e.Content as IUpdatablePage;
            if (item != null)
            {
                if (_currentPage != null)
                {
                    _currentPage.DetachUpdateEvent();
                }
                item.AttachUpdateEvent();
                item.Update();
                _currentPage = item;
            }

            if (e.Content.GetType() == typeof(LoginPage))
            {
                //Need to cleanup the memory
                //We are remove all history
                JournalEntry entry = null;
                do
                {
                    entry = _pageViewer.NavigationService.RemoveBackEntry();
                }
                while (entry != null);
            }
        }
    }
}
