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
using Google.Apis.Plus.v1;
using Google.Apis.Plus.v1.Data;
using Google.Apis.Tasks.v1;
using Google.Apis.Tasks.v1.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.Xml;

namespace Agenda
{
	/// <summary>
	/// Interaktionslogik für "App.xaml"
	/// </summary>
	public partial class App : Application
	{
		static string[] Scopes = { TasksService.Scope.TasksReadonly,
			Google.Apis.Plus.v1.PlusService.Scope.UserinfoEmail,
			Google.Apis.Plus.v1.PlusService.Scope.UserinfoProfile};

		public void generateConfig()
		{
			XmlDocument xdoc = new XmlDocument();

			XmlNode xnodeDeclaration = xdoc.CreateXmlDeclaration("1.0", "UTF-8", null);

			XmlNode xnodeConfig = xdoc.CreateElement("config");

			XmlNode xnodeConfig_loggedIn = xdoc.CreateElement("loggedIn");
			xnodeConfig_loggedIn.InnerText = "false";


			xdoc.AppendChild(xnodeDeclaration);
			xdoc.AppendChild(xnodeConfig);
			xnodeConfig.AppendChild(xnodeConfig_loggedIn);

			xdoc.Save("config.xml");
		}

		public static DualDifferentReturn<TasksService, PlusService> autenthicate()
		{
			DualDifferentReturn<TasksService, PlusService> returnValue = null;

			Thread tLogin = new Thread(() =>
			{
				UserCredential credential;

				using (var stream =
					new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
				{

					string credPath = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)).FullName;
					credPath = Path.Combine(credPath, @"Local\Agenda\data\credentials");

					credential = GoogleWebAuthorizationBroker.AuthorizeAsync(GoogleClientSecrets.Load(stream).Secrets,
						Scopes,
						"user",
						CancellationToken.None,
						new FileDataStore(credPath, true)).Result;
				}

				XmlDocument xdoc = new XmlDocument();
				xdoc.Load("config.xml");
				XmlNodeList xnodelist = xdoc.SelectNodes("//config/loggedIn");

				foreach (XmlNode node in xnodelist)
				{
					if (node.Name == "loggedIn")
						node.InnerText = "true";
				}
				xdoc.Save("config.xml");

				TasksService tasksService = new TasksService(new BaseClientService.Initializer()
				{
					HttpClientInitializer = credential,
					ApplicationName = "Agenda",
				});

				PlusService plusService = new PlusService(new BaseClientService.Initializer()
				{
					HttpClientInitializer = credential,
					ApplicationName = "Agenda",
				});

				returnValue = new DualDifferentReturn<TasksService, PlusService>(tasksService, plusService);
			});
			tLogin.Start();
			tLogin.Join();

			return returnValue;
		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			if (!File.Exists("config.xml"))
				generateConfig();

			XmlDocument xdoc = new XmlDocument();
			xdoc.Load("config.xml");
			XmlNodeList xnodelist = xdoc.SelectNodes("//config/loggedIn");
			XmlNode correctNode = null;

			foreach (XmlNode node in xnodelist)
			{
				if (node.Name == "loggedIn")
					correctNode = node;
			}
			if (correctNode == null)
				generateConfig();

			if (correctNode.InnerText == "false")
			{
				new LoginUI().Show();
			}
			else
			{
				new MainUI(App.autenthicate()).Show();
			}
		}
	}

	public class ETasklist
	{
		public Google.Apis.Tasks.v1.Data.TaskList tasklist;
		public IList<Google.Apis.Tasks.v1.Data.Task> tasks;

		public ETasklist(Google.Apis.Tasks.v1.Data.TaskList tasklist, IList<Google.Apis.Tasks.v1.Data.Task> tasks)
		{
			this.tasklist = tasklist;
			this.tasks = tasks;
		}
	}

	public class DualDifferentReturn<E, F>
	{
		public E returnValue1;
		public F returnValue2;

		public DualDifferentReturn(E returnValue1, F returnValue2)
		{
			this.returnValue1 = returnValue1;
			this.returnValue2 = returnValue2;
		}
	}
}
