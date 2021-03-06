﻿using GMac.GMacCompiler.Semantic.ASTConstants;
using IronyGrammars.Semantic.Expression.Value;
using IronyGrammars.Semantic.Symbol;
using SymbolicInterface.Mathematica.Expression;

namespace GMac.GMacCompiler.Semantic.AST
{
    /// <summary>
    /// A sigle basis vector for a GMac frame. Any basis blade for the frame is a symbolic outer product of different basis vectors.
    /// Any multivector in the frame is a linear combination of the basis blades of the frame
    /// </summary>
    public sealed class GMacFrameBasisVector : SymbolNamedValue
    {
        /// <summary>
        /// The zero-based index of the basis vector in the frame. If the frame has n basis vectors the index is an integer between 0 and n - 1
        /// </summary>
        internal int BasisVectorIndex { get; }

        /// <summary>
        /// The scalar signature of the basis vector (the inner product of this beais vector by itself)
        /// </summary>
        internal MathematicaScalar Signature { get; private set; }

        internal GMacValueMultivector MultivectorValue { get; }

        public override ILanguageValue AssociatedValue => MultivectorValue;

        /// <summary>
        /// The ID associated with the basis vector as a basis blade in the frame. 
        /// If the frame has n basis vectors and k is a basis vector index between 0 and (n - 1)
        /// the ID is the iteger 2 ^ k
        /// </summary>
        internal int BasisVectorId => 1 << BasisVectorIndex;

        /// <summary>
        /// The parent frame of this basis vector
        /// </summary>
        internal GMacFrame ParentFrame => (GMacFrame)ParentLanguageSymbol;

        internal GMacAst GMacRootAst => (GMacAst)RootAst;


        internal GMacFrameBasisVector(string basisVectorName, GMacFrame parentFrame, int basisVectorIndex, MathematicaScalar signature)
            : base(basisVectorName, parentFrame.ChildScope, RoleNames.FrameBasisVector, parentFrame.MultivectorType)
        {
            BasisVectorIndex = basisVectorIndex;

            Signature = signature;

            MultivectorValue = GMacValueMultivector.CreateBasisBlade(ParentFrame.MultivectorType, BasisVectorId);
        }
    }
}