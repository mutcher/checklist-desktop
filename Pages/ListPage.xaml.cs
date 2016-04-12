using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    internal sealed class MainList : LoadableList
    {
        public MainList(ServerConnector connector)
            : base(connector, 0)
        {
        }
    }

    /// <summary>
    /// Interaction logic for ListPage.xaml
    /// </summary>
    internal partial class ListPage : Page, IUpdatablePage
    {
        private ServerConnector mConnector = null;
        private MainList mMainList = null;
        private string mAddItemText = "Insert list name here";

        public ListPage(ServerConnector connector)
        {
            mConnector = connector;
            mMainList = new MainList(connector);
            InitializeComponent();
            _mainListBox.ItemsSource = mMainList;
            showAddItemTip();
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(_addItemTextBox.Text) && _addItemTextBox.Text != mAddItemText)
            {
                mMainList.Add(_addItemTextBox.Text);
                showAddItemTip();
            }
        }

        private void showAddItemTip()
        {
            _addItemTextBox.Text = mAddItemText;
            _addItemTextBox.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFA3A3A3"));
            _addItemTextBox.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
        }

        private void removeAddItemTip()
        {
            Style st = new System.Windows.Style();
            _addItemTextBox.Text = String.Empty;
            _addItemTextBox.Foreground = (Brush)FindResource("textbox_textColor");
            _addItemTextBox.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left;
        }

        private void _addItemTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_addItemTextBox.Text == mAddItemText)
            {
                removeAddItemTip();
            }
        }

        private void _addItemTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrEmpty(_addItemTextBox.Text))
            {
                showAddItemTip();
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            mConnector.disconnect();
            NavigationService.Navigate(new LoginPage(mConnector));
        }

        private void DeleteListButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult ans = MessageBox.Show("Remove selected list?", "Remove list", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (ans == MessageBoxResult.Yes)
            {
                mMainList.Remove((String)_mainListBox.SelectedItem);
            }
        }

        private void _mainListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int selectedIndex = _mainListBox.SelectedIndex;
            if (selectedIndex != -1)
            {
                SublistItemList list = new SublistItemList(mConnector, selectedIndex);
                NavigationService.Navigate(new SubListPage(list, (String)_mainListBox.SelectedValue));
            }
        }

        public void AttachUpdateEvent()
        {
            mMainList.AttachActivatedEvent();
        }

        public void DetachUpdateEvent()
        {
            mMainList.DetachActivatedEvent();
        }

        public void Update()
        {
            mMainList.LoadList();
        }
    }
}
