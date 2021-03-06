﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CRTE.Annotations;

namespace CRTE
{
    class UserList : INotifyPropertyChanged
    {
        private ObservableCollection<string> _userList = new ObservableCollection<string>();

        public ObservableCollection<string> UserLists
        {
            get { return _userList; }
            set { _userList = value; OnPropertyChanged(nameof(UserLists)); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
