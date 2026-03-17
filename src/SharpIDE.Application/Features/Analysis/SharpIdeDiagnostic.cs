using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace SharpIDE.Application.Features.Analysis;

public record SharpIdeDiagnostic(LinePositionSpan Span, Diagnostic Diagnostic, string FilePath);
