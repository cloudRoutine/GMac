﻿using System;
using System.Collections.Generic;
using GMac.GMacMath.Symbolic.Metrics;
using GMac.GMacMath.Symbolic.Multivectors;

namespace GMac.GMacMath.Symbolic.Products
{
    public abstract class GaSymBilinearProductOrthogonal 
        : GaSymBilinearProduct, IGaSymBilinearOrthogonalProduct
    {
        public GaSymMetricOrthogonal OrthogonalMetric { get; }

        public override IGaSymMetric Metric
            => OrthogonalMetric;

        public override IGaSymMultivector this[int id1, int id2]
            => MapToTerm(id1, id2);


        protected GaSymBilinearProductOrthogonal(GaSymMetricOrthogonal basisBladesSignatures)
        {
            OrthogonalMetric = basisBladesSignatures;
        }

        public abstract GaSymMultivectorTerm MapToTerm(int id1, int id2);

        public override IEnumerable<Tuple<int, int, IGaSymMultivector>> BasisBladesMaps()
        {
            for (var id1 = 0; id1 < DomainGaSpaceDimension; id1++)
            for (var id2 = 0; id2 < DomainGaSpaceDimension2; id2++)
            {
                var mv = MapToTerm(id1, id2);

                if (!mv.IsNullOrZero())
                    yield return new Tuple<int, int, IGaSymMultivector>(id1, id2, mv);
            }
        }

        public override IEnumerable<Tuple<int, int, IGaSymMultivector>> BasisVectorsMaps()
        {
            for (var index1 = 0; index1 < DomainVSpaceDimension; index1++)
            for (var index2 = 0; index2 < DomainVSpaceDimension2; index2++)
            {
                var id1 = GMacMathUtils.BasisBladeId(1, index1);
                var id2 = GMacMathUtils.BasisBladeId(1, index2);

                var mv = MapToTerm(id1, id2);

                if (!mv.IsNullOrZero())
                    yield return new Tuple<int, int, IGaSymMultivector>(index1, index2, mv);
            }
        }
    }
}