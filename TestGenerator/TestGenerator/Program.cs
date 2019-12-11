using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;

namespace TestGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var resultpath = @"..\\..\\..\\TestGenerated.cs";
            var path = @"..\\..\\..\\Test.cs";
            using (var stream = File.OpenRead(path))
            {
                var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(stream), path: path);
                var root = syntaxTree.GetRoot();
                var nodes = root.DescendantNodes();
                var Usings = nodes.OfType<UsingDirectiveSyntax>().FirstOrDefault();
                var Namespace = nodes.OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
                var Class = nodes.OfType<ClassDeclarationSyntax>().FirstOrDefault();
                var Method = nodes.OfType<MethodDeclarationSyntax>().FirstOrDefault();
                var Generated = CompilationUnit()
                                                .WithUsings(
                                                    SingletonList<UsingDirectiveSyntax>(
                                                        UsingDirective(
                                                            QualifiedName(
                                                                IdentifierName("NUnit"),
                                                                IdentifierName("Framework")))))
                                                .WithMembers(
                                                    SingletonList<MemberDeclarationSyntax>(
                                                        NamespaceDeclaration(
                                                            QualifiedName(
                                                                IdentifierName(Namespace.Name.ToString()),
                                                                IdentifierName("Tests")))
                                                        .WithMembers(
                                                            SingletonList<MemberDeclarationSyntax>(
                                                                ClassDeclaration(Class.Identifier.Text + "Tests")
                                                                .WithAttributeLists(
                                                                    SingletonList<AttributeListSyntax>(
                                                                        AttributeList(
                                                                            SingletonSeparatedList<AttributeSyntax>(
                                                                                Attribute(
                                                                                    IdentifierName("TestFixture"))))))
                                                                .WithMembers(
                                                                    SingletonList<MemberDeclarationSyntax>(
                                                                        ConstructorDeclaration(
                                                                            Identifier(Method.Identifier.Text + "MethodTest"))
                                                                        .WithAttributeLists(
                                                                            SingletonList<AttributeListSyntax>(
                                                                                AttributeList(
                                                                                    SingletonSeparatedList<AttributeSyntax>(
                                                                                        Attribute(
                                                                                            IdentifierName("Test"))))))
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
                                                                                                            Literal("autogenerated"))))))))))))))))
                                                .NormalizeWhitespace();

                FileStream fileStream = new FileStream(resultpath, FileMode.OpenOrCreate);
                byte[] array = System.Text.Encoding.Default.GetBytes(Generated.ToFullString());
                fileStream.Write(array, 0, array.Length);
                fileStream.Close();
            }
        }
    }
}
