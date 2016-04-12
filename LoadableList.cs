using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace desktop
{
    internal class LoadableList : ObservableCollection<string>
    {
        private ServerConnector _Connector = null;
        protected ServerConnector Connector
        {
            get
            {
                return _Connector;
            }
        }

        private byte _listID;
        private bool _sendToServer = true;

        public delegate void ItemTroubles(int index, string item);
        public event ItemTroubles onItemNotAdded;
        public event ItemTroubles onItemNotDeleted;

        public void LoadList()
        {
            _sendToServer = false;
            this.Clear();
            if (_Connector.IsAvailable)
            {
                var list = _Connector.getList(_listID);
                foreach (var item in list)
                {
                    this.Add(item);
                }
            }
            _sendToServer = true;
        }

        public LoadableList(ServerConnector connector, byte listID)
        {
            _Connector = connector;
            _listID = listID;
        }

        public void AttachActivatedEvent()
        {
            Application.Current.Activated += Application_Activated;
        }

        public void DetachActivatedEvent()
        {
            Application.Current.Activated -= Application_Activated;
        }

        void Application_Activated(object sender, EventArgs e)
        {
            LoadList();
        }

        protected override void InsertItem(int index, string item)
        {
            if (_sendToServer)
            {
                bool isAdded = false;
                if (_listID == 0)
                {
                    isAdded = _Connector.addList(item);
                }
                else
                {
                    isAdded = _Connector.addListItem(_listID, item);
                }
                if (!isAdded && onItemNotAdded != null)
                {
                    onItemNotAdded(index, item);
                }

                LoadList();
            }
            else
            {
                base.InsertItem(index, item);
            }
        }

        protected override void RemoveItem(int index)
        {
            if (_sendToServer)
            {
                bool isRemoved = false;
                string item = this[index];
                if (_listID == 0)
                {
                    isRemoved = _Connector.removeList(item);
                }
                else
                {
                    isRemoved = _Connector.removeListItem(_listID, item);
                }
                if (!isRemoved && onItemNotDeleted != null)
                {
                    onItemNotDeleted(index, item);
                }

                LoadList();
            }
            else
            {
                base.RemoveItem(index);
            }
        }
    }
}
