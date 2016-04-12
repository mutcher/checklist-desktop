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
    internal sealed class SublistItemList : LoadableList
    {
        public SublistItemList(ServerConnector connector, int listID)
            :base(connector, Convert.ToByte(listID + 1))
        {
        }
    }

    /// <summary>
    /// Interaction logic for SubListPage.xaml
    /// </summary>
    internal partial class SubListPage : Page, IUpdatablePage
    {
        private SublistItemList mItemList = null;
        private string mAddItemText = "Insert item name here";

        public SubListPage(SublistItemList list, string listName)
        {
            mItemList = list;
            InitializeComponent();
            _mainListBox.ItemsSource = list;
            this.Title = listName;
            showAddItemTip();
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrEmpty(_addItemTextBox.Text) && _addItemTextBox.Text != mAddItemText)
            {
                mItemList.Add(_addItemTextBox.Text);
                showAddItemTip();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.GoBack();
        }

        private void DeleteItemButton_Click(object sender, RoutedEventArgs e)
        {
            mItemList.Remove((String)_mainListBox.SelectedValue);
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

        public void AttachUpdateEvent()
        {
            mItemList.AttachActivatedEvent();
        }

        public void DetachUpdateEvent()
        {
            mItemList.DetachActivatedEvent();
        }

        public void Update()
        {
            mItemList.LoadList();
        }
    }
}
