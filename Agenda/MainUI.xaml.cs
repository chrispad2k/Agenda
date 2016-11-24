using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Plus.v1;
using Google.Apis.Plus.v1.Data;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

using MahApps.Metro.Controls;

using Task = Google.Apis.Tasks.v1.Data.Task;
using MaterialDesignThemes.Wpf;
using System.Windows.Controls;
using ControlzEx;
using System.Xml;

namespace Agenda
{
	/// <summary>
	/// Interaktionslogik für MainUI.xaml
	/// </summary>
	public partial class MainUI : MetroWindow
	{
		List<ETasklist> eTasklists;

		public MainUI(DualDifferentReturn<TasksService, PlusService> services)
		{
			InitializeComponent();

			try
			{
				TasksService tasksService = services.returnValue1;
				PlusService plusService = services.returnValue2;

				var me = plusService.People.Get("me").Execute();
				string email = me.Emails.FirstOrDefault().Value;
				string name = me.DisplayName;

				MainUI_lblAccountInfo.Content = name + " (" + email + ")"; 

				TasklistsResource.ListRequest tasklistRequest = tasksService.Tasklists.List();
				IList<TaskList> taskLists = tasklistRequest.Execute().Items;

				if (taskLists != null && taskLists.Count > 0)
				{
					eTasklists = new List<ETasklist>();
					foreach (TaskList taskList in taskLists)
					{
						TasksResource.ListRequest taskRequest = tasksService.Tasks.List(taskList.Id);
						IList<Task> tasks = taskRequest.Execute().Items;

						ETasklist eTasklist = new ETasklist(taskList, tasks);
						eTasklists.Add(eTasklist);

					}

					listTasks(eTasklists);
				}
				else
				{
					Console.WriteLine("No task lists found.");
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.ToString() + " " + e.StackTrace);
			}

			this.BringIntoView();
			this.Activate();
		}

		public void listTasks(List<ETasklist> eTasklists)
		{
			int counter = 1;
			foreach (ETasklist eTasklist in eTasklists)
			{
				Card cardTaskslist = new Card();
				cardTaskslist.Margin = new Thickness(4);

				StackPanel spInside = new StackPanel();

				TextBlock tbTitle = new TextBlock();
				tbTitle.Text = eTasklist.tasklist.Title;
				tbTitle.Margin = new Thickness(16, 16, 12, 8);
				tbTitle.FontSize = 16;

				StackPanel spClearAll = new StackPanel();
				spClearAll.Margin = new Thickness(8, 0, 8, 8);
				spClearAll.Orientation = Orientation.Horizontal;
				spClearAll.HorizontalAlignment = HorizontalAlignment.Right;

				Button btnClearAll = new Button();
				btnClearAll.Name = "MainUI_btnTask" + counter + "ClearAll";
				btnClearAll.HorizontalAlignment = HorizontalAlignment.Right;
				btnClearAll.Style = this.FindResource("MaterialDesignToolForegroundButton") as Style;
				btnClearAll.Width = 30;
				btnClearAll.Padding = new Thickness(2, 0, 0, 0);
				RippleAssist.SetIsCentered(btnClearAll, true);
				btnClearAll.Content = new PackIcon() { Kind = PackIconKind.CheckAll };

				spClearAll.Children.Add(btnClearAll);
				spInside.Children.Add(tbTitle);
				spInside.Children.Add(spClearAll);
				cardTaskslist.Content = spInside;
				MainUI_spTasklists.Children.Add(cardTaskslist);
				
				foreach (Task task in eTasklist.tasks)
				{
					if (task.Title != "")
					{
						CheckBox cbTask = new CheckBox();
						cbTask.Margin = new Thickness(16, 4, 16, 0);
						cbTask.Style = this.FindResource("MaterialDesignUserForegroundCheckBox") as Style;
						cbTask.Content = task.Title;
						spInside.Children.Insert(spInside.Children.Count - 1, cbTask);
						Separator spr = new Separator();
						spr.Style = this.FindResource("MaterialDesignLightSeparator") as Style;
						spInside.Children.Insert(spInside.Children.Count - 1, spr);
					}
				}
				++counter;
			}
		}

		private void MainUI_btnDisconnect_Click(object sender, RoutedEventArgs e)
		{
			string path = Path.Combine(Directory.GetParent(
				Environment.GetFolderPath(
					Environment.SpecialFolder.ApplicationData)).FullName, 
				@"Local\Agenda\data\credentials\Google.Apis.Auth.OAuth2.Responses.TokenResponse-user");
			if (File.Exists(path))
			{
				File.Delete(path);
			}

			XmlDocument xdoc = new XmlDocument();
			xdoc.Load("config.xml");
			XmlNodeList xnodelist = xdoc.SelectNodes("//config/loggedIn");

			foreach (XmlNode node in xnodelist)
			{
				if (node.Name == "loggedIn")
					node.InnerText = "false";
			}
			xdoc.Save("config.xml");

			new LoginUI().Show();
			this.Close();
		}
	}
}
