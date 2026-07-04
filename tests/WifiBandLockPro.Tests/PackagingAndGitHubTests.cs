// Standardized to production level
// Purpose: TDD Unit Tests verifying assembly packaging metadata, branding consistency, and release readiness
// Dependencies: xUnit, WifiBandLockPro.Core

namespace WifiBandLockPro.Tests;

using System;
using System.IO;
using System.Reflection;
using Xunit;
using WifiBandLockPro.Core.Services;

public class PackagingAndGitHubTests
{
    private static string GetSolutionRoot()
    {
        string dir = AppDomain.CurrentDomain.BaseDirectory;
        while (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
        {
            if (Directory.Exists(Path.Combine(dir, "src")) && Directory.Exists(Path.Combine(dir, "tests")))
            {
                return dir;
            }
            var parent = Directory.GetParent(dir);
            if (parent == null) break;
            dir = parent.FullName;
        }
        return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../../.."));
    }

    [Fact]
    public void CoreAssembly_ShouldHaveCorrectBrandingMetadata()
    {
        var asm = typeof(LocalizationService).Assembly;
        Assert.NotNull(asm);
        Assert.StartsWith("WifiBandLockPro.Core", asm.GetName().Name);
    }

    [Fact]
    public void AppIcon_ShouldExistInAppProjectDirectory()
    {
        string root = GetSolutionRoot();
        string icoPath = Path.Combine(root, "src", "WifiBandLockPro.App", "app.ico");
        Assert.True(File.Exists(icoPath), $"Expected application icon at {icoPath}");
    }

    [Fact]
    public void GitHubReleaseWorkflow_ShouldBeConfigured()
    {
        string root = GetSolutionRoot();
        string workflowPath = Path.Combine(root, ".github", "workflows", "release.yml");
        Assert.True(File.Exists(workflowPath), $"Expected GitHub Actions release workflow at {workflowPath}");
    }
}
