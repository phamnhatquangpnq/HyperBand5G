// Standardized to production level
// Purpose: Main Window code-behind binding ViewModel and managing minimization to System Tray
// Dependencies: System.ComponentModel, System.Windows, WifiBandLockPro.Core.ViewModels

namespace WifiBandLockPro.App;

using System.ComponentModel;
using System.Windows;
using WifiBandLockPro.Core.ViewModels;

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
    }

    private void OnWindowClosing(object? sender, CancelEventArgs e)
    {
        // Cancel close and hide to tray so auto-switch engine continues monitoring in background
        e.Cancel = true;
        Hide();
    }
}