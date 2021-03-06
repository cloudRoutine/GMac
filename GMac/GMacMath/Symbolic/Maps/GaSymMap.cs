﻿using SymbolicInterface.Mathematica;

namespace GMac.GMacMath.Symbolic.Maps
{
    public abstract class GaSymMap : IGaSymMap
    {
        public MathematicaInterface CasInterface { get; }

        public MathematicaConnection CasConnection => CasInterface.Connection;

        public MathematicaEvaluator CasEvaluator => CasInterface.Evaluator;

        public MathematicaConstants CasConstants => CasInterface.Constants;


        public abstract int TargetVSpaceDimension { get; }

        public int TargetGaSpaceDimension
            => TargetVSpaceDimension.ToGaSpaceDimension();


        protected GaSymMap()
        {
            CasInterface = SymbolicUtils.Cas;
        }
    }
}