﻿using System;
using GMac.GMacCompiler.Semantic.AST;
using GMac.GMacCompiler.Semantic.ASTConstants;
using GMac.GMacCompiler.Syntax;
using GMac.GMacMath.Symbolic;
using Irony.Parsing;
using IronyGrammars.Semantic.Expression;
using IronyGrammars.Semantic.Expression.Value;
using IronyGrammars.SourceCode;
using SymbolicInterface.Mathematica.Expression;

namespace GMac.GMacCompiler.Semantic.ASTGenerator
{
    internal sealed class GMacExpressionAtomicGenerator : GMacAstSymbolGenerator
    {
        public static ILanguageExpression Translate(GMacExpressionBasicGenerator langExprGen, ParseTreeNode node)
        {
            var context = langExprGen.Context;

            context.PushState(node);

            var translator = new GMacExpressionAtomicGenerator();// new GMacExpressionAtomicGenerator(langExprGen);

            translator.SetContext(langExprGen);
            translator.Translate();

            context.PopState();

            var result = translator._generatedExpression;

            //MasterPool.Release(translator);

            return result;
        }


        private ILanguageExpression _generatedExpression;

        public GMacExpressionBasicGenerator BasicExpressionGenerator { get; private set; }


        //public override void ResetOnAcquire()
        //{
        //    base.ResetOnAcquire();

        //    _generatedExpression = null;
        //    BasicExpressionGenerator = null;
        //}


        //private GMacExpressionAtomicGenerator(GMacSymbolTranslatorContext context)
        //    : base(context)
        //{
        //    BasicExpressionGenerator = new GMacExpressionBasicGenerator(context);
        //}

        private void SetContext(GMacExpressionBasicGenerator basicExprGen)
        {
            SetContext(basicExprGen.Context);
            BasicExpressionGenerator = basicExprGen;
        }


        ///Translate a constant number (Int32 or Double)
        private ILanguageExpression translate_Constant_Number(ParseTreeNode node)
        {
            var numberText = node.FindTokenAndGetText();

            Int32 intNumber;
            if (Int32.TryParse(numberText, out intNumber))
                return ValuePrimitive<MathematicaScalar>.Create(
                    GMacRootAst.ScalarType,
                    MathematicaScalar.Create(SymbolicUtils.Cas, intNumber)
                    );

            Double doubleNumber;
            if (Double.TryParse(numberText, out doubleNumber))
                return ValuePrimitive<MathematicaScalar>.Create(
                    GMacRootAst.ScalarType, 
                    MathematicaScalar.Create(SymbolicUtils.Cas, doubleNumber)
                    );

            return CompilationLog.RaiseGeneratorError<ILanguageExpression>("Constant number not recognized", node);
        }

        private ILanguageExpression translate_Expression_Scoped(ParseTreeNode node)
        {
            var frame =
                (GMacFrame)GMacValueAccessGenerator.Translate_Direct(Context, node.ChildNodes[0], RoleNames.Frame);

            Context.OpenScope(frame);

            var expr = GMacExpressionGenerator.Translate(Context, node.ChildNodes[1]);

            Context.CloseScope(frame);

            return expr;
        }

        //private ILanguageExpression translate_Expression_Cast(ParseTreeNode node)
        //{
        //    var langType = GMacValueAccessGenerator.Translate_Direct_LanguageType(this.Context, node.ChildNodes[0]);

        //    var expr = GMacExpressionGenerator.Translate(this.BasicExpressionGenerator, node.ChildNodes[0]);

        //    return this.BasicExpressionGenerator.Generate_TypeCast(langType, expr);
        //}

        protected override void Translate()
        {
            var subNode = RootParseNode.ChildNodes[0];

            switch (subNode.Term.Name)
            {
                case GMacParseNodeNames.ConstantNumber:
                    _generatedExpression = translate_Constant_Number(subNode);
                    break;

                case GMacParseNodeNames.ExpressionScoped:
                    _generatedExpression = translate_Expression_Scoped(subNode);
                    break;

                case GMacParseNodeNames.QualifiedItem:
                    _generatedExpression = GMacValueAccessGenerator.Translate(Context, subNode.ChildNodes[0], false);
                    break;

                case GMacParseNodeNames.ExpressionBracketed:
                    _generatedExpression = GMacExpressionGenerator.Translate(BasicExpressionGenerator, subNode.ChildNodes[0]);
                    break;

                case GMacParseNodeNames.StringLiteral:
                    _generatedExpression = GMacExpressionAtomicSymbolicGenerator.Translate(Context, subNode);
                    break;

                case GMacParseNodeNames.ExpressionComposite:
                    _generatedExpression = GMacExpressionCompositeGenerator.Translate(Context, subNode);
                    break;

                case GMacParseNodeNames.ExpressionFunction:
                    _generatedExpression = GMacExpressionAtomicFunctionGenerator.Translate(BasicExpressionGenerator, subNode);
                    break;

                default:
                    CompilationLog.RaiseGeneratorError<ILanguageExpression>("Expression not recognized", RootParseNode);
                    break;
            }
        }
    }
}
