﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Net;
// Asset
using ProgressDialogs;
using IniParser;
using IniParser.Model;

namespace ifme.hitoha
{
	public partial class frmAbout : Form
	{
		WebClient client = new WebClient();
		ProgressDialog progressDialog = new ProgressDialog();

		string tmp = System.IO.Path.GetTempPath();

		public frmAbout()
		{
			this.Icon = Properties.Resources.aruuie_ifme;

			client.DownloadProgressChanged += client_DownloadProgressChanged;
			client.DownloadFileCompleted += client_DownloadFileCompleted;

			InitializeComponent();
		}

		private void LoadLang()
		{
			var parser = new FileIniDataParser();
			IniData data = parser.ReadFile(Language.Path.Folder + "\\" + Language.Default + ".ini");

			lblUpdateInfo.Text = String.Format(lblUpdateInfo.Text, Globals.AppInfo.Name, data[Language.Section.Abt]["Latest"]);

			btnUpdate.Text = data[Language.Section.Abt]["btnUpdate"];
			lblInfo.Text = data[Language.Section.Abt]["Description"];
			lnkEndUser.Text = data[Language.Section.Abt]["EndUserRight"];
			lnkLicense.Text = data[Language.Section.Abt]["LicenseInfo"];
			lnkPrivacy.Text = data[Language.Section.Abt]["PrivacyPolicy"];
			lblMascot.Text = String.Format(lblMascot.Text, data[Language.Section.Abt]["ByWho"]);
		}

		private void frmAbout_Load(object sender, EventArgs e)
		{
			LoadLang();

			this.Text = String.Format(this.Text, "About", Globals.AppInfo.Name);
			lblTitle.Text = String.Format(lblTitle.Text, Globals.AppInfo.Name);
			lblVersion.Text = String.Format(lblVersion.Text, Globals.AppInfo.Version, Globals.AppInfo.CPU);

			if (!Globals.AppInfo.VersionEqual)
			{
				lblUpdateInfo.Visible = false;
				btnUpdate.Visible = true;
			}
		}

		private void lnkEndUser_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("http://ifme.sourceforge.net/?page/rights.html");
		}

		private void lnkLicense_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("http://opensource.org/licenses/GPL-2.0");
		}

		private void lnkPrivacy_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("http://ifme.sf.net/");
		}

		private void btnUpdate_Click(object sender, EventArgs e)
		{
			try
			{
				progressDialog.Show();
				progressDialog.AutoClose = true;
				progressDialog.Title = "Updating";
				timer.Start();

				string LATEST = client.DownloadString("http://ifme.sourceforge.net/update/version.txt");
				client.DownloadFileAsync(new Uri("http://master.dl.sourceforge.net/project/ifme/encoder-gui/" + LATEST + "/x265ui.7z"), tmp + "\\ifme\\saishin.jp");
			}
			catch (Exception ex)
			{
				progressDialog.Close();
				MessageBox.Show(ex.Message, "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}
		private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
		{
			progressDialog.Value = e.ProgressPercentage;

			progressDialog.Line1 = String.Format("Downloading {0} KB", Math.Round(Convert.ToDouble(e.TotalBytesToReceive / 1024)));
			progressDialog.Line2 = "From: master.dl.sourceforge.net";
		}

		private void client_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			if (!System.IO.Directory.Exists(tmp + "\\ifme"))
				System.IO.Directory.CreateDirectory(tmp + "\\ifme");

			System.IO.File.Copy("za.dll", tmp + "\\ifme\\7za.exe");
			System.IO.File.Copy("unins000.exe", tmp + "\\ifme\\unins000.exe");
			System.IO.File.Copy("unins000.dat", tmp + "\\ifme\\unins000.dat");

			foreach (var item in System.IO.Directory.GetDirectories(Globals.AppInfo.CurrentFolder))
			{
				System.IO.Directory.Delete(item, true);
			}

			System.Diagnostics.Process P = new System.Diagnostics.Process();
			P.StartInfo.FileName = "cmd.exe";
			P.StartInfo.Arguments = String.Format("/c TIMEOUT /T 3 /NOBREAK & del /F /S /Q *.* & {0}\\ifme\\7za.exe -x y {0}\\ifme\\saishin.jp & copy {0}\\ifme\\unins000.exe unins000.exe & copy {0}\\ifme\\unins000.dat unins000.dat & del /F /S /Q {0} & TIMEOUT /T 3 /NOBREAK & call ifme.exe", tmp);
			P.StartInfo.CreateNoWindow = true;
			P.StartInfo.WorkingDirectory = Globals.AppInfo.CurrentFolder;

			P.Start();
			Application.Exit();
		}
	}
}