﻿using System.Collections.Generic;
using System.Linq;
using GMac.GMacCompiler.Semantic.AST;
using IronyGrammars.Semantic.Expression;
using IronyGrammars.Semantic.Expression.Basic;
using IronyGrammars.Semantic.Operator;
using SymbolicInterface.Mathematica.Expression;

namespace GMac.GMacAST.Expressions
{
    public sealed class AstParametricSymbolicExpression : AstExpression 
    {
        internal BasicPolyadic AssociatedPolyadicExpression { get; }

        internal override ILanguageExpression AssociatedExpression => AssociatedPolyadicExpression;

        internal ILanguageOperator AssociatedOperator => AssociatedPolyadicExpression.Operator;

        internal GMacParametricSymbolicExpression AssociatedParametricSymbolic => AssociatedPolyadicExpression.Operator as GMacParametricSymbolicExpression;


        public override bool IsValidParametricSymbolic => AssociatedPolyadicExpression != null &&
                                                          AssociatedParametricSymbolic != null;

        /// <summary>
        /// The symbolic scalar of this expression
        /// </summary>
        public MathematicaScalar SymbolicScalar => IsValidParametricSymbolic ? AssociatedParametricSymbolic.AssociatedMathematicaScalar : null;

        /// <summary>
        /// The names of the symbolic variables used in this expression
        /// </summary>
        public IEnumerable<string> UsedSymbolicVariables
        {
            get
            {
                var assignments =
                    AssociatedPolyadicExpression.Operands as OperandsByName;

                if (ReferenceEquals(assignments, null)) return null;

                return
                    assignments.OperandsDictionary.Select(item => item.Key);
            }
        }

        /// <summary>
        /// The symbolic variables assignments of this expression
        /// </summary>
        public IEnumerable<KeyValuePair<string, AstExpression>> Assignments
        {
            get
            {
                var assignments = 
                    AssociatedPolyadicExpression.Operands as OperandsByName;

                if (ReferenceEquals(assignments, null)) return null;

                return 
                    assignments.OperandsDictionary.Select(
                        item => new KeyValuePair<string, AstExpression>(
                            item.Key,
                            item.Value.ToAstExpression()
                            )
                        );
            }
        }


        internal AstParametricSymbolicExpression(BasicPolyadic expr)
        {
            AssociatedPolyadicExpression = expr;
        }
    }
}
