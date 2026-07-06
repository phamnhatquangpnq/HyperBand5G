namespace HyperBoost.Core.Models;

public record InstalledAppItem(string Id, string DisplayName, string DisplayVersion, string Publisher, string InstallDate, string InstallLocation, string UninstallString, string QuietUninstallString, string? IconPath, long EstimatedSizeKb = 0L)
{
	public string SizeDisplay
	{
		get
		{
			long estimatedSizeKb = EstimatedSizeKb;
			if (estimatedSizeKb < 1024)
			{
				if (estimatedSizeKb > 0)
				{
					return $"{EstimatedSizeKb} KB";
				}
				return "-";
			}
			if (estimatedSizeKb < 1048576)
			{
				return $"{(double)EstimatedSizeKb / 1024.0:F1} MB";
			}
			return $"{(double)EstimatedSizeKb / 1048576.0:F2} GB";
		}
	}
}
