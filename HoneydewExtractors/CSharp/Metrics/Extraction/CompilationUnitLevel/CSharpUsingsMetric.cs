using System.Collections.Generic;
using System.Linq;
using HoneydewExtractors.Core.Metrics.Extraction;
using HoneydewModels;
using HoneydewModels.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HoneydewExtractors.CSharp.Metrics.Extraction.CompilationUnitLevel
{
    public class CSharpUsingsMetric : HoneydewCSharpSyntaxWalker,
        IExtractionMetric<CSharpSyntacticModel, CSharpSemanticModel, CSharpSyntaxNode>
    {
        public CSharpSyntacticModel HoneydewSyntacticModel { get; set; }
        public CSharpSemanticModel HoneydewSemanticModel { get; set; }

        public IDictionary<string, ISet<UsingModel>> Usings { get; private set; } =
            new Dictionary<string, ISet<UsingModel>>();

        private readonly ISet<UsingModel> _commonUsings = new HashSet<UsingModel>();

        private readonly IDictionary<string, ISet<UsingModel>> _namespaceUsings =
            new Dictionary<string, ISet<UsingModel>>();

        public ExtractionMetricType GetMetricType()
        {
            return ExtractionMetricType.CompilationUnitLevel;
        }

        public override IMetricValue GetMetric()
        {
            return new MetricValue<IDictionary<string, ISet<UsingModel>>>(Usings);
        }

        public override string PrettyPrint()
        {
            return "Usings";
        }

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            var usingName = node.Name.ToString();
            var isStatic = node.StaticKeyword.Value != null;
            var alias = node.Alias == null ? "" : node.Alias.Name.ToString();
            var aliasType = EAliasType.None;

            if (!string.IsNullOrEmpty(alias))
            {
                aliasType = HoneydewSemanticModel.GetAliasTypeOfNamespace(node.Name);
            }

            var cSharpUsing = new UsingModel()
            {
                Name = usingName,
                IsStatic = isStatic,
                Alias = alias,
                AliasType = aliasType
            };

            if (node.Parent != null && node.Parent.Kind() == SyntaxKind.NamespaceDeclaration)
            {
                var namespaceSyntax = (NamespaceDeclarationSyntax) node.Parent;
                var namespaceName = HoneydewSemanticModel.GetFullName(namespaceSyntax.Name);

                if (_namespaceUsings.TryGetValue(namespaceName, out var usingsSet))
                {
                    usingsSet.Add(cSharpUsing);
                }
                else
                {
                    _namespaceUsings.Add(namespaceName, new HashSet<UsingModel>
                    {
                        cSharpUsing
                    });
                }
            }
            else
            {
                _commonUsings.Add(cSharpUsing);
            }
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            AddUsingToDeclaration(node);
        }

        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            AddUsingToDeclaration(node);
        }

        public override void VisitRecordDeclaration(RecordDeclarationSyntax node)
        {
            AddUsingToDeclaration(node);
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            AddUsingToDeclaration(node);
        }

        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            AddUsingToDeclaration(node);
        }

        public override void VisitDelegateDeclaration(DelegateDeclarationSyntax node)
        {
            AddUsingToDeclaration(node);
        }

        private void AddUsingToDeclaration(MemberDeclarationSyntax node)
        {
            var fullName = HoneydewSemanticModel.GetFullName(node);
            var usingsFromParent = AddUsingsFromParent(GetParentNamespace(node.Parent));

            AddToUsings(fullName, usingsFromParent);

            foreach (var declarationSyntax in node.DescendantNodes().OfType<BaseTypeDeclarationSyntax>())
            {
                AddUsingToDeclaration(declarationSyntax);
            }

            foreach (var declarationSyntax in node.DescendantNodes().OfType<DelegateDeclarationSyntax>())
            {
                AddUsingToDeclaration(declarationSyntax);
            }
        }

        private NamespaceDeclarationSyntax GetParentNamespace(SyntaxNode syntaxNode)
        {
            return syntaxNode switch
            {
                null => null,
                NamespaceDeclarationSyntax namespaceDeclaration => namespaceDeclaration,
                _ => syntaxNode.Parent switch
                {
                    null => null,
                    NamespaceDeclarationSyntax namespaceDeclarationSyntax => namespaceDeclarationSyntax,
                    _ => GetParentNamespace(syntaxNode.Parent)
                }
            };
        }

        private void AddToUsings(string name, ISet<UsingModel> usingsSet)
        {
            if (Usings.ContainsKey(name))
            {
                return;
            }

            Usings.Add(name, usingsSet);
        }

        private ISet<UsingModel> AddUsingsFromParent(NamespaceDeclarationSyntax node)
        {
            var usings = new HashSet<UsingModel>();

            if (node == null)
            {
                foreach (var @using in _commonUsings)
                {
                    usings.Add(@using);
                }

                return usings;
            }

            var name = HoneydewSemanticModel.GetFullName(node);
            if (_namespaceUsings.TryGetValue(name, out var namespaceSet))
            {
                foreach (var @using in namespaceSet)
                {
                    usings.Add(@using);
                }
            }

            foreach (var @using in AddUsingsFromParent(node.Parent as NamespaceDeclarationSyntax))
            {
                usings.Add(@using);
            }


            return usings;
        }
    }
}
