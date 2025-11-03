using Godot;
using NuGet.Versioning;
using SharpIDE.Application.Features.SolutionDiscovery.VsPersistence;

namespace SharpIDE.Godot.Features.Nuget;

public partial class PackageDetailsProjectEntry : MarginContainer
{
    private Label _projectNameLabel = null!;
    private Label _installedVersionLabel = null!;
    
    public SharpIdeProjectModel ProjectModel { get; set; } = null!;
    public NuGetVersion? InstalledVersion { get; set; }
    public bool IsTransitive { get; set; }
    public override void _Ready()
    {
        _projectNameLabel = GetNode<Label>("%ProjectNameLabel");;
        _installedVersionLabel = GetNode<Label>("%InstalledVersionLabel");
        SetValues();
    }

    private void SetValues()
    {
        if (ProjectModel == null) return;
        _projectNameLabel.Text = ProjectModel.Name;
        _installedVersionLabel.Text = IsTransitive ? $"({InstalledVersion?.ToNormalizedString()})" : InstalledVersion?.ToNormalizedString();
    }
}