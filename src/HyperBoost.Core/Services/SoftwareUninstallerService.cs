using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Win32;
using HyperBoost.Core.Models;

namespace HyperBoost.Core.Services;

public class SoftwareUninstallerService : ISoftwareUninstallerService
{
	public Task<List<InstalledAppItem>> GetInstalledAppsAsync()
	{
		return Task.Run(delegate
		{
			Dictionary<string, InstalledAppItem> dictionary = new Dictionary<string, InstalledAppItem>(StringComparer.OrdinalIgnoreCase);
			string[] array = new string[2] { "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", "SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Uninstall" };
			foreach (string subKeyPath in array)
			{
				ScanRegistryKey(Registry.LocalMachine, subKeyPath, dictionary);
			}
			ScanRegistryKey(Registry.CurrentUser, "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall", dictionary);
			return dictionary.Values.OrderBy((InstalledAppItem x) => x.DisplayName).ToList();
		});
	}

	private void ScanRegistryKey(RegistryKey rootKey, string subKeyPath, Dictionary<string, InstalledAppItem> apps)
	{
		try
		{
			using RegistryKey registryKey = rootKey.OpenSubKey(subKeyPath);
			if (registryKey == null)
			{
				return;
			}
			string[] subKeyNames = registryKey.GetSubKeyNames();
			foreach (string name in subKeyNames)
			{
				try
				{
					using RegistryKey registryKey2 = registryKey.OpenSubKey(name);
					if (registryKey2 == null)
					{
						continue;
					}
					string text = registryKey2.GetValue("DisplayName") as string;
					if (string.IsNullOrWhiteSpace(text) || text.StartsWith("KB", StringComparison.OrdinalIgnoreCase) || text.StartsWith("Security Update", StringComparison.OrdinalIgnoreCase) || text.StartsWith("Update for Windows", StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}
					int num = 0;
					try
					{
						num = Convert.ToInt32(registryKey2.GetValue("SystemComponent") ?? ((object)0));
					}
					catch
					{
					}
					if (num == 1 || !string.IsNullOrWhiteSpace(registryKey2.GetValue("ParentKeyName") as string))
					{
						continue;
					}
					string text2 = (registryKey2.GetValue("DisplayVersion") as string) ?? "-";
					string text3 = (registryKey2.GetValue("Publisher") as string) ?? "-";
					string text4 = (registryKey2.GetValue("InstallDate") as string) ?? "-";
					string text5 = (registryKey2.GetValue("InstallLocation") as string) ?? "";
					string text6 = (registryKey2.GetValue("UninstallString") as string) ?? "";
					string text7 = (registryKey2.GetValue("QuietUninstallString") as string) ?? "";
					string displayIcon = registryKey2.GetValue("DisplayIcon") as string;
					if (!string.IsNullOrWhiteSpace(text6))
					{
						string iconPath = ResolveIconPath(displayIcon, text5, text6);
						long appSizeKb = GetAppSizeKb(registryKey2, text5);
						string text8 = text + "_" + text2;
						if (!apps.ContainsKey(text8))
						{
							apps[text8] = new InstalledAppItem(text8, text.Trim(), text2.Trim(), text3.Trim(), text4.Trim(), text5.Trim(), text6.Trim(), text7.Trim(), iconPath, appSizeKb);
						}
					}
				}
				catch
				{
				}
			}
		}
		catch
		{
		}
	}

	private string? ResolveIconPath(string? displayIcon, string? installLocation, string? uninstallString)
	{
		try
		{
			if (!string.IsNullOrWhiteSpace(displayIcon))
			{
				string text = displayIcon.Trim(new char[3] { '"', '\'', ' ' });
				int num = text.IndexOf(',');
				if (num > 0)
				{
					text = text.Substring(0, num).Trim(new char[3] { '"', '\'', ' ' });
				}
				if (File.Exists(text))
				{
					return text;
				}
			}
			if (!string.IsNullOrWhiteSpace(installLocation) && Directory.Exists(installLocation))
			{
				string[] files = Directory.GetFiles(installLocation, "*.exe", SearchOption.TopDirectoryOnly);
				if (files.Length != 0)
				{
					return files.OrderByDescending((string f) => new FileInfo(f).Length).First();
				}
			}
			if (!string.IsNullOrWhiteSpace(uninstallString))
			{
				string text2 = uninstallString.Trim(new char[3] { '"', '\'', ' ' });
				if (text2.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) && File.Exists(text2))
				{
					return text2;
				}
				int num2 = text2.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
				if (num2 > 0)
				{
					string text3 = text2.Substring(0, num2 + 4).Trim(new char[3] { '"', '\'', ' ' });
					if (File.Exists(text3))
					{
						return text3;
					}
				}
			}
		}
		catch
		{
		}
		return null;
	}

	public Task<bool> UninstallAppAsync(InstalledAppItem app)
	{
		return Task.Run(delegate
		{
			try
			{
				if (string.IsNullOrWhiteSpace(app.UninstallString))
				{
					return false;
				}
				Process.Start(new ProcessStartInfo
				{
					FileName = "cmd.exe",
					Arguments = "/c " + app.UninstallString,
					UseShellExecute = true,
					CreateNoWindow = false
				});
				return true;
			}
			catch
			{
				return false;
			}
		});
	}

	public Task<bool> OpenInstallLocationAsync(InstalledAppItem app)
	{
		return Task.Run(delegate
		{
			try
			{
				string text = app.InstallLocation;
				if ((string.IsNullOrWhiteSpace(text) || !Directory.Exists(text)) && !string.IsNullOrWhiteSpace(app.IconPath) && File.Exists(app.IconPath))
				{
					text = Path.GetDirectoryName(app.IconPath) ?? "";
				}
				if (!string.IsNullOrWhiteSpace(text) && Directory.Exists(text))
				{
					Process.Start(new ProcessStartInfo
					{
						FileName = "explorer.exe",
						Arguments = "\"" + text + "\"",
						UseShellExecute = true
					});
					return true;
				}
			}
			catch
			{
			}
			return false;
		});
	}

	private static long GetAppSizeKb(RegistryKey subKey, string installLocation)
	{
		long num = 0L;
		try
		{
			object value = subKey.GetValue("EstimatedSize");
			if (value != null)
			{
				num = Convert.ToInt64(value);
			}
		}
		catch
		{
		}
		if (num <= 0 && !string.IsNullOrWhiteSpace(installLocation))
		{
			try
			{
				string fullPath = Path.GetFullPath(installLocation.Trim());
				if (Directory.Exists(fullPath) && !IsSystemOrRootFolder(fullPath))
				{
					long num2 = 0L;
					IEnumerable<string> enumerable = Directory.EnumerateFiles(fullPath, "*", SearchOption.AllDirectories);
					int num3 = 0;
					foreach (string item in enumerable)
					{
						if (++num3 <= 20000)
						{
							try
							{
								num2 += new FileInfo(item).Length;
							}
							catch
							{
							}
							continue;
						}
						break;
					}
					num = num2 / 1024;
				}
			}
			catch
			{
			}
		}
		return num;
	}

	private static bool IsSystemOrRootFolder(string dir)
	{
		try
		{
			string text = Path.GetPathRoot(dir) ?? "";
			if (string.Equals(dir.TrimEnd(new char[2] { '\\', '/' }), text.TrimEnd(new char[2] { '\\', '/' }), StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			string folderPath2 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);
			string folderPath3 = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
			string folderPath4 = Environment.GetFolderPath(Environment.SpecialFolder.System);
			string folderPath5 = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);
			string folderPath6 = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
			string a = dir.TrimEnd(new char[2] { '\\', '/' });
			if (string.Equals(a, folderPath.TrimEnd(new char[2] { '\\', '/' }), StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			if (string.Equals(a, folderPath2.TrimEnd(new char[2] { '\\', '/' }), StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			if (string.Equals(a, folderPath3.TrimEnd(new char[2] { '\\', '/' }), StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			if (string.Equals(a, folderPath4.TrimEnd(new char[2] { '\\', '/' }), StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			if (string.Equals(a, folderPath5.TrimEnd(new char[2] { '\\', '/' }), StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			if (string.Equals(a, folderPath6.TrimEnd(new char[2] { '\\', '/' }), StringComparison.OrdinalIgnoreCase))
			{
				return true;
			}
			return false;
		}
		catch
		{
			return true;
		}
	}
}
