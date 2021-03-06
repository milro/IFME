﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Net;
// Asset
using IniParser;
using IniParser.Model;

namespace ifme.hitoha
{
	public partial class frmAbout : Form
	{
		string tmp = System.IO.Path.GetTempPath();
		string Title = Globals.AppInfo.Name;
		string Version = Globals.AppInfo.Version;
		string CPU = Globals.AppInfo.CPU;
		string BuildDate = Globals.AppInfo.BuildDate;
		string Info = null;

		int cnt = 0;
		string[] Names = new string[100];

		public frmAbout()
		{
			this.Icon = Properties.Resources.ifme_green;
			InitializeComponent();

			// Fix Mono drawings
			if (OS.IsLinux)
			{
				pictIfme.Width = 250;
				pictIfme.Height = 800;
			}
			else
			{
				this.BackgroundImage = Properties.Resources.AboutBackground;
				this.BackgroundImageLayout = ImageLayout.None;

				btnUpdate.Left = 350;

				foreach (Control ctl in this.Controls)
				{
					if (ctl.GetType() == typeof(Label) || ctl.GetType() == typeof(LinkLabel))
					{
						ctl.Left = 256;
						ctl.Width = 372;
					}
				}
			}
		}

		private void LoadLang()
		{
			var parser = new FileIniDataParser();

			IniData data = parser.ReadFile(Path.Combine(Language.Folder, Language.Default + ".ini"));

			lblUpdateInfo.Text = String.Format(lblUpdateInfo.Text, Globals.AppInfo.Name, data[Language.Section.Abt]["Latest"]);

			btnUpdate.Text = data[Language.Section.Abt]["btnUpdate"];
			Info = data[Language.Section.Abt]["Description"];
			//lnkEndUser.Text = data[Language.Section.Abt]["EndUserRight"];
			//lnkLicense.Text = data[Language.Section.Abt]["LicenseInfo"];
			//lnkPrivacy.Text = data[Language.Section.Abt]["PrivacyPolicy"];
			//lblMascot.Text = String.Format(lblMascot.Text, data[Language.Section.Abt]["ByWho"]);

			lblInfo.Text = Info;
		}

		private void frmAbout_Load(object sender, EventArgs e)
		{
			LoadLang();
			lblTitle.Text = String.Format("{0} {1}",Title, Version);
			lblAuthorInfo.Text = String.Format("Compiled on: {0} ({1} build)\nCopyleft (ɔ) 2013 - {2} Anime4000, GNU GPL v2", BuildDate, CPU, DateTime.Today.Year.ToString());
			this.Text = String.Format(this.Text, "About", Globals.AppInfo.Name);

			if (!Globals.AppInfo.VersionEqual)
			{
				lblUpdateInfo.Visible = false;
				btnUpdate.Visible = true;
				lnkChangeLog.Visible = true;
			}

			// Get first
			Names[cnt++] = "People @ MulticoreWare";
			Names[cnt++] = "Pis Kawaii Neko";

			// Get Names for Langauge author
			for (int i = 0; i < Language.Installed.Data.GetLength(0); i++)
			{
				if (Language.Installed.Data[i, 0] == null)
					break;

				if (String.Equals(Language.Installed.Data[i, 2], "Anime4000"))
					continue;

				Names[cnt++] = Language.Installed.Data[i, 2];
			}

			// Get Names for Addons author
			for (int i = 0; i < Addons.Installed.Data.GetLength(0); i++)
			{
				if (Addons.Installed.Data[i, 3] == null)
					break;

				if (String.Equals(Addons.Installed.Data[i, 3], "MulticoreWare"))
					continue;

				if (Addons.Installed.Data[i, 3].Contains("Xiph"))
					continue;

				if (Addons.Installed.Data[i, 3].Contains("FFmpeg"))
					continue;

				if (Addons.Installed.Data[i, 3].Contains("\\"))
					continue;

				Names[cnt++] = Addons.Installed.Data[i, 3];
			}

			Names[cnt++] = "FFmpeg Team";
			Names[cnt++] = "Xiph.Org Foundation";
			tmrScroll.Start();
		}

		private void btnUpdate_Click(object sender, EventArgs e)
		{
			var txt = btnUpdate.Text;

			if (txt.Contains('&'))
				txt = txt.Remove(txt.Length - 1).Substring(1) + " ?";
			else
				txt = txt.Remove(txt.Length - 1) + " ?";

			var msg = MessageBox.Show(txt, "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

			if (msg == System.Windows.Forms.DialogResult.No)
				return;

			if (!Directory.Exists(Globals.AppInfo.TempFolder))
				Directory.CreateDirectory(Globals.AppInfo.TempFolder);

			string cmd = null, arg = null, file = null;

			if (OS.IsWindows)
			{
				cmd = "cmd.exe";
				arg = "/c START \"\" /B update.cmd \"{0}\" \"{1}\" \"ifme.exe\"";
				file = "x265ui-x64_win.7z";

				File.WriteAllText(Path.Combine(Globals.AppInfo.TempFolder, "update.cmd"), framework.ShellScript.ScriptWin);
				File.Copy(Path.Combine(Globals.AppInfo.CurrentFolder, "unpack.exe"), Path.Combine(Globals.AppInfo.TempFolder, "7za.exe"), true);
				File.Copy(Path.Combine(Globals.AppInfo.CurrentFolder, "wget.exe"), Path.Combine(Globals.AppInfo.TempFolder, "wget.exe"), true);
			}
			else
			{
				cmd = "sh";
				arg = "update.sh \"{0}\" \"{1}\" \"ifme.sh\"";
				file = "x265ui-x64_linux.tar.gz";

				File.WriteAllText(Path.Combine(Globals.AppInfo.TempFolder, "update.sh"), framework.ShellScript.ScriptLinux);
				File.Copy(Path.Combine(Globals.AppInfo.CurrentFolder, "unpack"), Path.Combine(Globals.AppInfo.TempFolder, "7za"), true);
			}

			Process P = new Process();
			var SI = P.StartInfo;
			SI.FileName = cmd;
			SI.Arguments = String.Format(arg, "http://sourceforge.net/projects/ifme/files/encoder-gui/" + Globals.AppInfo.VersionNew + "/" + file + "/download", Globals.AppInfo.CurrentFolder);
			SI.WorkingDirectory = Globals.AppInfo.TempFolder;
			SI.UseShellExecute = false;

			P.Start();
			Application.ExitThread();
		}

		private void lnkChangeLog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			System.Diagnostics.Process.Start("https://raw.githubusercontent.com/Anime4000/IFME/master/installer/text_changelog.txt");
		}

		private void tmrScroll_Tick(object sender, EventArgs e)
		{
			if (Names[cnt] == null)
				cnt = 0;
			else
				lblNames.Text = Names[cnt++];
		}

		private void lblWhoChar_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("http://www.pixiv.net/member.php?id=6206705");
		}

		private void lblWhoDraw_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("http://ray-en.deviantart.com/");
		}

		private void panel2_Click(object sender, EventArgs e)
		{
			System.Diagnostics.Process.Start("http://ifme.sourceforge.net/");
		}
	}
}
