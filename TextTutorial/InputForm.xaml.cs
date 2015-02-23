using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace TextTutorial
{
    public delegate void OnCompleteDelegate(object sender, RoutedEventArgs e, string username, string password);

    public partial class InputForm : UserControl
    {
        public string username;
        public string password;

        public OnCompleteDelegate OnCompleteCallback;
        public InputForm(string defaultValue)
        {
            InitializeComponent();
            Username.Text = defaultValue;
            username = defaultValue;
        }

        private void OkayButton_Click(object sender, RoutedEventArgs e)
        {
            username = Username.Text;
            password = Password.Text;
            OnCompleteCallback(sender, e, username, password);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            OnCompleteCallback(sender, e, username, password);
        }

    }
}
