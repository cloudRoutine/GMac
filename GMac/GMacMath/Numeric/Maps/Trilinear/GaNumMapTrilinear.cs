﻿using System;
using System.Collections.Generic;
using GMac.GMacMath.Numeric.Multivectors;
using GMac.GMacMath.Numeric.Multivectors.Intermediate;

namespace GMac.GMacMath.Numeric.Maps.Trilinear
{
    public abstract class GaNumMapTrilinear : GaNumMap, IGaNumMapTrilinear
    {
        public abstract int DomainVSpaceDimension { get; }

        public int DomainGaSpaceDimension
            => DomainVSpaceDimension.ToGaSpaceDimension();

        public abstract IGaNumMultivector this[int id1, int id2, int id3] { get; }

        public virtual GaNumMultivector this[GaNumMultivector mv1, GaNumMultivector mv2, GaNumMultivector mv3]
            => MapToTemp(mv1, mv2, mv3).ToMultivector();

        public abstract IGaNumMultivectorTemp MapToTemp(GaNumMultivector mv1, GaNumMultivector mv2, GaNumMultivector mv3);

        public abstract IEnumerable<Tuple<int, int, int, IGaNumMultivector>> BasisBladesMaps();

        public virtual IEnumerable<Tuple<int, int, int, IGaNumMultivector>> BasisVectorsMaps()
        {
            for (var index1 = 0; index1 < DomainVSpaceDimension; index1++)
            for (var index2 = 0; index2 < DomainVSpaceDimension; index2++)
            for (var index3 = 0; index3 < DomainVSpaceDimension; index3++)
            {
                var id1 = GMacMathUtils.BasisBladeId(1, index1);
                var id2 = GMacMathUtils.BasisBladeId(1, index2);
                var id3 = GMacMathUtils.BasisBladeId(1, index3);
                var mv = this[id1, id2, id3];

                if (!mv.IsNullOrZero())
                    yield return new Tuple<int, int, int, IGaNumMultivector>(index1, index2, index3, mv);
            }
        }
    }
}
