﻿using System.Collections.Generic;
using GMac.GMacMath.Symbolic;
using GMac.GMacMath.Symbolic.Frames;
using GMac.GMacMath.Symbolic.Multivectors;
using IronyGrammars.Semantic;
using IronyGrammars.Semantic.Expression.Value;
using IronyGrammars.Semantic.Type;
using SymbolicInterface.Mathematica.Expression;

namespace GMac.GMacCompiler.Semantic.AST
{
    public sealed class GMacValueMultivector : ILanguageValueComposite
    {
        internal GMacFrameMultivector ValueMultivectorType { get; }

        internal GaSymMultivector SymbolicMultivector { get; }


        internal TypePrimitive CoefficientType => ((GMacAst)ValueMultivectorType.RootAst).ScalarType;

        internal GMacFrame MultivectorFrame => ValueMultivectorType.ParentFrame;

        internal GaSymFrame SymbolicFrame => ValueMultivectorType.ParentFrame.SymbolicFrame;

        public IronyAst RootAst => ValueMultivectorType.RootAst;

        internal GMacAst GMacRootAst => (GMacAst)ValueMultivectorType.RootAst;


        /// <summary>
        /// A language value is always a simple expression
        /// </summary>
        public bool IsSimpleExpression => true;


        private GMacValueMultivector(GMacFrameMultivector mvType, GaSymMultivector mvCoefs)
        {
            ValueMultivectorType = mvType;

            SymbolicMultivector = mvCoefs;
        }


        public ILanguageValue DuplicateValue(bool deepCopy)
        {
            return new GMacValueMultivector(ValueMultivectorType, GaSymMultivector.CreateCopy(SymbolicMultivector));
        }

        public ILanguageType ExpressionType => ValueMultivectorType;


        internal GMacValueMultivector this[IEnumerable<int> idsList]
        {
            get
            {
                var mv = CreateZero(ValueMultivectorType);

                foreach (var id in idsList)
                    mv.SymbolicMultivector.SetTermCoef(id, SymbolicMultivector[id]);

                return mv;
            }
            set
            {
                foreach (var id in idsList)
                    SymbolicMultivector.SetTermCoef(id, value.SymbolicMultivector[id]);
            }
        }

        internal ValuePrimitive<MathematicaScalar> this[int id]
        {
            get
            {
                return ValuePrimitive<MathematicaScalar>.Create(
                    CoefficientType, 
                    SymbolicMultivector[id].ToMathematicaScalar()
                    );
            }
            set
            {
                SymbolicMultivector.SetTermCoef(id, value.Value);
            }
        }


        public override string ToString()
        {
            return RootAst.Describe(this);
        }


        internal static GMacValueMultivector Create(GMacFrameMultivector mvType, GaSymMultivector mvCoefs)
        {
            return new GMacValueMultivector(mvType, mvCoefs);
        }

        internal static GMacValueMultivector CreateZero(GMacFrameMultivector mvType)
        {
            var mvCoefs = GaSymMultivector.CreateZero(mvType.ParentFrame.GaSpaceDimension);

            return new GMacValueMultivector(mvType, mvCoefs);
        }

        internal static GMacValueMultivector CreateTerm(GMacFrameMultivector mvType, int id, MathematicaScalar coef)
        {
            var mvCoefs = GaSymMultivector.CreateTerm(mvType.ParentFrame.GaSpaceDimension, id, coef);

            return new GMacValueMultivector(mvType, mvCoefs);
        }

        internal static GMacValueMultivector CreateScalar(GMacFrameMultivector mvType, MathematicaScalar coef)
        {
            var mvCoefs = GaSymMultivector.CreateScalar(mvType.ParentFrame.GaSpaceDimension, coef);

            return new GMacValueMultivector(mvType, mvCoefs);
        }

        internal static GMacValueMultivector CreatePseudoScalar(GMacFrameMultivector mvType, MathematicaScalar coef)
        {
            var mvCoefs = GaSymMultivector.CreatePseudoscalar(mvType.ParentFrame.GaSpaceDimension, coef);

            return new GMacValueMultivector(mvType, mvCoefs);
        }

        internal static GMacValueMultivector CreateBasisBlade(GMacFrameMultivector mvType, int id)
        {
            var mvCoefs = GaSymMultivector.CreateBasisBlade(mvType.ParentFrame.GaSpaceDimension, id);

            return new GMacValueMultivector(mvType, mvCoefs);
        }

    }
}
