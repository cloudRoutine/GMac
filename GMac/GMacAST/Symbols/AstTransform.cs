﻿using GMac.GMacCompiler.Semantic.AST;
using GMac.GMacMath.Symbolic.Maps.Unilinear;
using IronyGrammars.Semantic.Symbol;

namespace GMac.GMacAST.Symbols
{
    public sealed class AstTransform : AstSymbol
    {
        internal GMacMultivectorTransform AssociatedTransform { get; }


        internal override LanguageSymbol AssociatedSymbol => AssociatedTransform;

        internal IGaSymMapUnilinear AssociatedSymbolicTransform => AssociatedTransform.AssociatedSymbolicTransform;


        public override bool IsValidTransform => AssociatedTransform != null;

        /// <summary>
        /// The source frame of this transform
        /// </summary>
        public AstFrame SourceFrame => new AstFrame(AssociatedTransform.SourceFrame);

        /// <summary>
        /// The target frame of this transform
        /// </summary>
        public AstFrame TargetFrame => new AstFrame(AssociatedTransform.TargetFrame);


        internal AstTransform(GMacMultivectorTransform transform)
        {
            AssociatedTransform = transform;
        }

    }
}
