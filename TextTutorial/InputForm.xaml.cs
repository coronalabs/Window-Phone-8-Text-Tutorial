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
    public partial class InputForm : UserControl
    {
		public event EventHandler<EventArgs> Submitted;
		public event EventHandler<EventArgs> Canceled;

        public InputForm()
        {
            InitializeComponent();

			this.MinWidth = System.Windows.Application.Current.Host.Content.ActualWidth * 0.8;
        }

		public string Username
		{
			get { return fUsernameTextBox.Text; }
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				fUsernameTextBox.Text = value;
			}
		}

		public string Password
		{
			get { return fPasswordTextBox.Password; }
			set
			{
				if (value == null)
				{
					value = string.Empty;
				}
				fPasswordTextBox.Password = value;
			}
		}

        private void OkayButton_Click(object sender, RoutedEventArgs e)
        {
			if (this.Submitted != null)
			{
				this.Submitted.Invoke(this, EventArgs.Empty);
			}
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
			if (this.Canceled != null)
			{
				this.Canceled.Invoke(this, EventArgs.Empty);
			}
		}
    }
}
