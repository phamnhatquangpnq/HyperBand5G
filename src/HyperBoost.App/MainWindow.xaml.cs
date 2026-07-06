// Standardized to production level
// Purpose: Main Window code-behind binding ViewModel and managing minimization to System Tray
// Dependencies: System.ComponentModel, System.Windows, HyperBoost.Core.ViewModels

namespace HyperBoost.App;

using System.ComponentModel;
using System.Windows;
using HyperBoost.Core.ViewModels;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        // When user closes window, minimize to System Tray instead of exiting
        Closing += OnWindowClosing;

        // Handle Show App Info requests from ViewModel
        _viewModel.OnShowAppInfoRequest += (sender, app) =>
        {
            if (app == null) return;
            string info = $"📱 Tên ứng dụng: {app.DisplayName}\n" +
                          $"🏷️ Phiên bản: {app.DisplayVersion}\n" +
                          $"🏢 Nhà phát hành: {app.Publisher}\n" +
                          $"📅 Ngày cài đặt: {app.InstallDate}\n" +
                          $"💾 Dung lượng ước tính: {app.SizeDisplay}\n\n" +
                          $"📂 Thư mục cài đặt:\n{app.InstallLocation}\n\n" +
                          $"🗑️ Lệnh gỡ cài đặt:\n{app.UninstallString}";
            MessageBox.Show(info, $"Thông tin chi tiết: {app.DisplayName}", MessageBoxButton.OK, MessageBoxImage.Information);
        };
    }

    private void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        // Cancel close and hide to tray so auto-switch engine continues monitoring in background
        e.Cancel = true;
        Hide();
    }
}