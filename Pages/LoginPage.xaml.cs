using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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

namespace desktop.Pages
{
    /// <summary>
    /// Interaction logic for LoginPage.xaml
    /// </summary>
    internal partial class LoginPage : Page
    {
        ServerConnector mConnector = null;
        public LoginPage(ServerConnector connector)
        {
            mConnector = connector;
            InitializeComponent();
        }

        private void _loginButton_Click(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(_loginBox.Text) || String.IsNullOrEmpty(_passwordBox.Password))
            {
                MessageBox.Show("Login/password cannot be empty");

                if (String.IsNullOrEmpty(_loginBox.Text))
                {
                    _loginBox.Focus();
                }
                else
                {
                    _passwordBox.Focus();
                }
                return;
            }

            try
            {
                mConnector.connect();
                mConnector.login(_loginBox.Text, _passwordBox.Password);
                this.NavigationService.Navigate(new ListPage(mConnector));
            }
            catch (IncorrectLoginPasswordException ex)
            {
                MessageBox.Show(ex.Message);
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void _exitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void _passwordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                _loginButton_Click(sender, e);
            }
        }

        private void _aboutButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new AboutPage());
        }
    }
}
