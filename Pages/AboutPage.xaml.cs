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

namespace desktop.Pages
{
    /// <summary>
    /// Interaction logic for AboutPage.xaml
    /// </summary>
    internal partial class AboutPage : Page
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        private void BackButtonClick(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private void WebsiteClick(object sender, MouseButtonEventArgs e)
        {
            ContentControl obj = sender as ContentControl;
            System.Diagnostics.Process.Start(obj.Content.ToString());
        }
    }
}
