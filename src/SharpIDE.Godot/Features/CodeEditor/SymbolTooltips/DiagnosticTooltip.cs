using Godot;
using SharpIDE.Application.Features.Analysis;

namespace SharpIDE.Godot.Features.CodeEditor;

public static partial class SymbolInfoComponents
{
    public static RichTextLabel GetDiagnostic(SharpIdeDiagnostic diagnostic)
    {
        var label = new RichTextLabel();
        label.PushColor(CachedColors.White);
        label.PushFont(MonospaceFont);
        label.AddText(diagnostic.Diagnostic.GetMessage());
        label.Pop(); // font
        label.Pop();
        return label;
    }
}