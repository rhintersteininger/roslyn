﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Composition;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Extensions;
using Microsoft.CodeAnalysis.CSharp.LanguageServices;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Editing;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.LanguageServices;

namespace Microsoft.CodeAnalysis.CSharp.CodeGeneration
{
    [ExportLanguageService(typeof(SyntaxGeneratorInternal), LanguageNames.CSharp), Shared]
    internal sealed class CSharpSyntaxGeneratorInternal : SyntaxGeneratorInternal
    {
        [ImportingConstructor]
        [SuppressMessage("RoslynDiagnosticsReliability", "RS0033:Importing constructor should be [Obsolete]", Justification = "Incorrectly used in production code: https://github.com/dotnet/roslyn/issues/42839")]
        public CSharpSyntaxGeneratorInternal()
        {
        }

        public static readonly SyntaxGeneratorInternal Instance = new CSharpSyntaxGeneratorInternal();

        internal override ISyntaxFacts SyntaxFacts => CSharpSyntaxFacts.Instance;

        internal override SyntaxNode LocalDeclarationStatement(SyntaxNode type, SyntaxToken name, SyntaxNode initializer, bool isConst)
        {
            return SyntaxFactory.LocalDeclarationStatement(
                isConst ? SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.ConstKeyword)) : default,
                 VariableDeclaration(type, name, initializer));
        }

        internal override SyntaxNode WithInitializer(SyntaxNode variableDeclarator, SyntaxNode initializer)
            => ((VariableDeclaratorSyntax)variableDeclarator).WithInitializer((EqualsValueClauseSyntax)initializer);

        internal override SyntaxNode EqualsValueClause(SyntaxToken operatorToken, SyntaxNode value)
            => SyntaxFactory.EqualsValueClause(operatorToken, (ExpressionSyntax)value);

        internal static VariableDeclarationSyntax VariableDeclaration(SyntaxNode type, SyntaxToken name, SyntaxNode expression)
        {
            return SyntaxFactory.VariableDeclaration(
                type == null ? SyntaxFactory.IdentifierName("var") : (TypeSyntax)type,
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.VariableDeclarator(
                            name, argumentList: null,
                            expression == null ? null : SyntaxFactory.EqualsValueClause((ExpressionSyntax)expression))));
        }

        internal override SyntaxToken Identifier(string identifier)
            => SyntaxFactory.Identifier(identifier);

        internal override SyntaxNode ConditionalAccessExpression(SyntaxNode expression, SyntaxNode whenNotNull)
            => SyntaxFactory.ConditionalAccessExpression((ExpressionSyntax)expression, (ExpressionSyntax)whenNotNull);

        internal override SyntaxNode MemberBindingExpression(SyntaxNode name)
            => SyntaxFactory.MemberBindingExpression((SimpleNameSyntax)name);

        internal override SyntaxNode RefExpression(SyntaxNode expression)
            => SyntaxFactory.RefExpression((ExpressionSyntax)expression);

        internal override SyntaxNode AddParentheses(SyntaxNode expressionOrPattern, bool includeElasticTrivia = true, bool addSimplifierAnnotation = true)
            => Parenthesize(expressionOrPattern, includeElasticTrivia, addSimplifierAnnotation);

        internal static SyntaxNode Parenthesize(SyntaxNode expressionOrPattern, bool includeElasticTrivia = true, bool addSimplifierAnnotation = true)
            => expressionOrPattern switch
            {
                ExpressionSyntax expression => expression.Parenthesize(includeElasticTrivia, addSimplifierAnnotation),
#if !CODE_STYLE
                PatternSyntax pattern => pattern.Parenthesize(includeElasticTrivia, addSimplifierAnnotation),
#endif
                var other => other,
            };

        internal override SyntaxNode YieldReturnStatement(SyntaxNode expressionOpt = null)
            => SyntaxFactory.YieldStatement(SyntaxKind.YieldReturnStatement, (ExpressionSyntax)expressionOpt);

        /// <summary>
        /// C# always requires a type to be present with a local declaration.  (Even if that type is
        /// <c>var</c>).
        /// </summary>
        internal override bool RequiresLocalDeclarationType() => true;

        internal override SyntaxNode InterpolatedStringExpression(SyntaxToken startToken, IEnumerable<SyntaxNode> content, SyntaxToken endToken)
            => SyntaxFactory.InterpolatedStringExpression(startToken, SyntaxFactory.List(content.Cast<InterpolatedStringContentSyntax>()), endToken);

        internal override SyntaxNode InterpolatedStringText(SyntaxToken textToken)
            => SyntaxFactory.InterpolatedStringText(textToken);

        internal override SyntaxToken InterpolatedStringTextToken(string content)
            => SyntaxFactory.Token(
                SyntaxFactory.TriviaList(),
                SyntaxKind.InterpolatedStringTextToken,
                content, "",
                SyntaxFactory.TriviaList());

        internal override SyntaxNode Interpolation(SyntaxNode syntaxNode)
            => SyntaxFactory.Interpolation((ExpressionSyntax)syntaxNode);

        internal override SyntaxNode InterpolationAlignmentClause(SyntaxNode alignment)
            => SyntaxFactory.InterpolationAlignmentClause(SyntaxFactory.Token(SyntaxKind.CommaToken), (ExpressionSyntax)alignment);

        internal override SyntaxNode InterpolationFormatClause(string format)
            => SyntaxFactory.InterpolationFormatClause(
                    SyntaxFactory.Token(SyntaxKind.ColonToken),
                    SyntaxFactory.Token(default, SyntaxKind.InterpolatedStringTextToken, format, format, default));
    }
}
