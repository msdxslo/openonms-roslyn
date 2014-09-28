using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DiagnosticAndCodeFix
{
    // TODO: Consider implementing other interfaces that implement IDiagnosticAnalyzer instead of or in addition to ISymbolAnalyzer

    [DiagnosticAnalyzer]
    [ExportDiagnosticAnalyzer(DiagnosticId, LanguageNames.CSharp)]
    public class DiagnosticAnalyzer : ISyntaxNodeAnalyzer<SyntaxKind>
    {
        public const string DiagnosticId = "DiagnosticAndCodeFix";
        internal const string Description = "Manjka Async postfix";
        internal const string MessageFormat = "Metoda '{0}' ni pravilno pomenovana, bumbar";
        internal const string Category = "Naming";

        internal static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Description, MessageFormat, Category, DiagnosticSeverity.Warning, true);

        public ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        public ImmutableArray<SyntaxKind> SyntaxKindsOfInterest
        {
            get
            {
                return ImmutableArray.Create(SyntaxKind.MethodDeclaration);
            }
        }

        public void AnalyzeNode(SyntaxNode node, SemanticModel semanticModel, Action<Diagnostic> addDiagnostic, AnalyzerOptions options, CancellationToken cancellationToken)
        {
            MethodDeclarationSyntax metoda = (MethodDeclarationSyntax)node;
            if (!metoda.Identifier.Text.EndsWith("Async"))
            {
                var info = semanticModel.GetSymbolInfo(metoda.ReturnType, cancellationToken);
                if (info.Symbol != null && info.Symbol.Name == "Task" && info.Symbol.ContainingAssembly.Name == "mscorlib")
                {
                    addDiagnostic(Diagnostic.Create(Rule, metoda.Identifier.GetLocation(), metoda.Identifier.Text));
                }
            }
        }
    }
}
