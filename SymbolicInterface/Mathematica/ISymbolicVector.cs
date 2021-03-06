﻿using System.Collections.Generic;
using SymbolicInterface.Mathematica.Expression;

namespace SymbolicInterface.Mathematica
{
    public interface ISymbolicVector : ISymbolicObject, IEnumerable<MathematicaScalar>
    {
        int Size { get; }

        MathematicaScalar this[int index] { get; }


        bool IsFullVector();

        bool IsSparseVector();

        
        ISymbolicVector Times(ISymbolicMatrix m);


        MathematicaVector ToMathematicaVector();

        MathematicaVector ToMathematicaFullVector();

        MathematicaVector ToMathematicaSparseVector();
    }
}
