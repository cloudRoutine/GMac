﻿using System;
using GMac.GMacMath.Numeric.Maps.Bilinear;
using GMac.GMacMath.Numeric.Maps.Unilinear;
using GMac.GMacMath.Numeric.Metrics;
using GMac.GMacMath.Numeric.Multivectors;
using GMac.GMacMath.Numeric.Products;
using MathNet.Numerics.LinearAlgebra.Double;

namespace GMac.GMacMath.Numeric.Frames
{
    public sealed class GaNumFrameNonOrthogonal : GaNumFrame
    {
        /// <summary>
        /// The matric holding all information of the derived frame system
        /// </summary>
        public GaNumMetricNonOrthogonal NonOrthogonalMetric { get; }

        public IGaNumMapUnilinear ThisToBaseFrameCba
            => NonOrthogonalMetric.DerivedToBaseCba;

        public IGaNumMapUnilinear BaseFrameToThisCba
            => NonOrthogonalMetric.BaseToDerivedCba;

        public override IGaNumMetric Metric
            => NonOrthogonalMetric;

        public override IGaNumMetricOrthogonal BaseOrthogonalMetric
            => NonOrthogonalMetric.BaseMetric;

        public override GaNumFrame BaseOrthogonalFrame
            => NonOrthogonalMetric.BaseFrame;

        public override bool IsEuclidean => false;

        public override bool IsOrthogonal => false;

        public override bool IsOrthonormal => false;

        public override int VSpaceDimension => InnerProductMatrix.RowCount;

        public override IGaNumMapBilinear ComputedOp { get; }

        public override IGaNumMapBilinear ComputedGp { get; }

        public override IGaNumMapBilinear ComputedSp { get; }

        public override IGaNumMapBilinear ComputedLcp { get; }

        public override IGaNumMapBilinear ComputedRcp { get; }

        public override IGaNumMapBilinear ComputedFdp { get; }

        public override IGaNumMapBilinear ComputedHip { get; }

        public override IGaNumMapBilinear ComputedAcp { get; }

        public override IGaNumMapBilinear ComputedCp { get; }


        internal GaNumFrameNonOrthogonal(GaNumFrame baseOrthoFrame, Matrix ipm, GaNumOutermorphism derivedToBaseOm, GaNumOutermorphism baseToDerivedOm)
        {
            if (baseOrthoFrame.IsOrthogonal == false)
                throw new GMacNumericException("Base frame must be orthogonal");

            if (ipm.IsDiagonal())
                throw new GMacNumericException("Inner product matrix must be non-diagonal");

            InnerProductMatrix = ipm;

            NonOrthogonalMetric =
                new GaNumMetricNonOrthogonal(
                    baseOrthoFrame,
                    this,
                    derivedToBaseOm,
                    baseToDerivedOm
                );

            Op = ComputedOp = new GaNumOp(VSpaceDimension);
            Gp = ComputedGp = GaNumBilinearProductCba.CreateGp(NonOrthogonalMetric);
            Sp = ComputedSp = GaNumBilinearProductCba.CreateSp(NonOrthogonalMetric);
            Lcp = ComputedLcp = GaNumBilinearProductCba.CreateLcp(NonOrthogonalMetric);
            Rcp = ComputedRcp = GaNumBilinearProductCba.CreateRcp(NonOrthogonalMetric);
            Fdp = ComputedFdp = GaNumBilinearProductCba.CreateFdp(NonOrthogonalMetric);
            Hip = ComputedHip = GaNumBilinearProductCba.CreateHip(NonOrthogonalMetric);
            Acp = ComputedAcp = GaNumBilinearProductCba.CreateAcp(NonOrthogonalMetric);
            Cp = ComputedCp = GaNumBilinearProductCba.CreateCp(NonOrthogonalMetric);

            UnitPseudoScalarCoef =
                (MaxBasisBladeId.BasisBladeIdHasNegativeReverse() ? -1.0d : 1.0d) /
                BasisBladeSignature(MaxBasisBladeId)[0];
        }


        protected override void ComputeIpm()
        {
            throw new NotImplementedException();
        }

        public override double BasisVectorSignature(int basisVectorIndex)
        {
            return InnerProductMatrix[basisVectorIndex, basisVectorIndex];
        }

        public override GaNumMultivector BasisBladeSignature(int id)
        {
            var basisBlade = GaNumMultivector.CreateBasisBlade(GaSpaceDimension, id);

            var sig = Gp[basisBlade, basisBlade];

            return id.BasisBladeIdHasNegativeReverse() ? -sig : sig;
        }

    }
}
