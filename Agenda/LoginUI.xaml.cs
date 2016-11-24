using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

using MahApps.Metro.Controls;

using Task = Google.Apis.Tasks.v1.Data.Task;
using System.Xml;

namespace Agenda
{
	/// <summary>
	/// Interaktionslogik für LoginUI.xaml
	/// </summary>
	public partial class LoginUI : MetroWindow
	{

		public LoginUI()
		{
			InitializeComponent();
		}
		
		private void Login_btnLogin_Click(object sender, RoutedEventArgs e)
		{
			new MainUI(App.autenthicate()).Show();
			this.Close();
		}
	}
}
