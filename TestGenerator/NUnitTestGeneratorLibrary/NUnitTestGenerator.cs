using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NUnitTestGeneratorLibrary
{
    public class NUnitTestGenerator
    {

        private SyntaxNode GenerateCompilationUnit(ClassDeclarationSyntax classDeclaration)
        {
            if (!(classDeclaration.Parent is NamespaceDeclarationSyntax))
            {
                return null;
            }

            string namespaceOfSourceClass = (classDeclaration.Parent as NamespaceDeclarationSyntax).Name.ToString();
            return CompilationUnit()
                    .WithUsings(
                        List<UsingDirectiveSyntax>(
                            new UsingDirectiveSyntax[]{
                            UsingDirective(
                                IdentifierName("System")),
                            UsingDirective(
                                IdentifierName("System.Collection.Generic")),
                            UsingDirective(
                                IdentifierName("System.Linq")),
                            UsingDirective(
                                IdentifierName("System.Text")),
                            UsingDirective(
                                IdentifierName("NUnit.Framework")),
                            UsingDirective(
                                IdentifierName(namespaceOfSourceClass))}))
                    .WithMembers(
                        SingletonList<MemberDeclarationSyntax>(
                            NamespaceDeclaration(
                                QualifiedName(
                                    IdentifierName(namespaceOfSourceClass),
                                    IdentifierName("Tests")))));
        }

        private SyntaxNode GenerateClassNode(SyntaxNode root, ClassDeclarationSyntax classDeclaration)
        {
            string sourceClassName = classDeclaration.Identifier.Text;
            NamespaceDeclarationSyntax oldNamespaceDeclaration = root.DescendantNodes().OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            NamespaceDeclarationSyntax newNamespaceDeclaration = oldNamespaceDeclaration.AddMembers(
                ClassDeclaration(sourceClassName)
                    .WithAttributeLists(
                        SingletonList<AttributeListSyntax>(
                            AttributeList(
                                SingletonSeparatedList<AttributeSyntax>(
                                    Attribute(
                                        IdentifierName("TestFixture"))))))
                    .WithModifiers(
                        TokenList(
                            Token(SyntaxKind.PublicKeyword)))
                    );

            return root.ReplaceNode(oldNamespaceDeclaration, newNamespaceDeclaration);
        }

        private SyntaxNode GenerateTestMethods(SyntaxNode root, IEnumerable<MethodDeclarationSyntax> methods)
        {
            ClassDeclarationSyntax oldClassDeclaration = root.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            ClassDeclarationSyntax newClassDeclaration = oldClassDeclaration;
            foreach (MethodDeclarationSyntax method in methods)
            {
                newClassDeclaration = newClassDeclaration.AddMembers(GenerateTestMethod(method));
            }

            return root.ReplaceNode(oldClassDeclaration, newClassDeclaration);
        }

        private MemberDeclarationSyntax GenerateTestMethod(MethodDeclarationSyntax method)
        {
            string methodIdentifier = method.Identifier.Text + "MethodTest";
            return MethodDeclaration(
                        PredefinedType(
                            Token(SyntaxKind.VoidKeyword)),
                        Identifier(methodIdentifier))
                    .WithAttributeLists(
                        SingletonList<AttributeListSyntax>(
                            AttributeList(
                                SingletonSeparatedList<AttributeSyntax>(
                                    Attribute(
                                        IdentifierName("Test"))))))
                    .WithModifiers(
                        TokenList(
                            Token(SyntaxKind.PublicKeyword)))
                    .WithBody(
                        Block(
                            SingletonList<StatementSyntax>(
                                ExpressionStatement(
                                    InvocationExpression(
                                        MemberAccessExpression(
                                            SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("Assert"),
                                            IdentifierName("Fail")))
                                    .WithArgumentList(
                                        ArgumentList(
                                            SingletonSeparatedList<ArgumentSyntax>(
                                                Argument(
                                                    LiteralExpression(
                                                        SyntaxKind.StringLiteralExpression,
                                                        Literal("autogenerated"))))))))));
        }

        public SyntaxNode GenerateCompilationUnitFromSourceCode(string sourceCode)
        {

            try { 
              CompilationUnitSyntax sourceRoot = CSharpSyntaxTree.ParseText(sourceCode).GetCompilationUnitRoot();

              if (sourceRoot.Members.Count == 0)
              {
                return null;
              }

              ClassDeclarationSyntax classDeclaration = sourceRoot.DescendantNodes().OfType<ClassDeclarationSyntax>().FirstOrDefault();

              SyntaxNode result = GenerateCompilationUnit(classDeclaration);
              result = GenerateClassNode(result, classDeclaration);
              result = GenerateTestMethods(result, sourceRoot.DescendantNodes().OfType<MethodDeclarationSyntax>().Where(method => method.Modifiers.Any(modifier => modifier.ToString() == "public")));
               return result.NormalizeWhitespace();
              }
            catch(Exception e)
            {
                return null;
            }
}

        public SyntaxNode Generate(string source)
        {
            return GenerateCompilationUnitFromSourceCode(source);
        }
    }
}
