﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GMac.GMacMath.Symbolic.Multivectors;
using GMac.GMacMath.Symbolic.Multivectors.Intermediate;
using SymbolicInterface.Mathematica.ExprFactory;
using TextComposerLib.Text.Structured;
using UtilLib.DataStructures.SparseTable;
using Wolfram.NETLink;

namespace GMac.GMacMath.Symbolic.Maps.Unilinear
{
    public sealed class GaSymMapUnilinearCoefSums : GaSymMapUnilinear
    {
        private sealed class GaSymMapUnilinearCoefSumsTerm : IEnumerable<Tuple<int, Expr>>
        {
            private readonly List<Tuple<int, Expr>> _factorsList
                = new List<Tuple<int, Expr>>();


            public int TargetBasisBladeId { get; private set; }

            public IEnumerable<string> TermsText
            {
                get
                {
                    var termBuilder = new StringBuilder();

                    foreach (var pair in _factorsList)
                        yield return termBuilder
                            .Clear()
                            .Append(pair.Item2)
                            .Append(" * (")
                            .Append(pair.Item1.BasisBladeIndexedName())
                            .Append(")")
                            .ToString();
                }
            }

            public Expr this[int domainBasisBladeId]
                => _factorsList
                       .FirstOrDefault(
                           factor =>
                               factor.Item1 == domainBasisBladeId
                       )?.Item2 ?? Expr.INT_ZERO;

            public Expr this[IGaSymMultivector mv1]
                => _factorsList.Count == 0
                    ? Expr.INT_ZERO
                    : Mfs.SumExpr(
                        _factorsList
                        .Select(term => Mfs.Times[term.Item2, mv1[term.Item1]])
                        .ToArray()
                    );


            internal GaSymMapUnilinearCoefSumsTerm(int targetBasisBladeId)
            {
                TargetBasisBladeId = targetBasisBladeId;
            }


            internal GaSymMapUnilinearCoefSumsTerm Reset()
            {
                _factorsList.Clear();

                return this;
            }

            internal GaSymMapUnilinearCoefSumsTerm AddFactor(int domainBasisBladeId, bool isNegative = false)
            {
                _factorsList.Add(
                    Tuple.Create(
                        domainBasisBladeId,
                        isNegative ? Expr.INT_MINUSONE : Expr.INT_ONE
                    ));

                return this;
            }

            internal GaSymMapUnilinearCoefSumsTerm AddFactor(int domainBasisBladeId, Expr factor)
            {
                _factorsList.Add(
                    Tuple.Create(
                        domainBasisBladeId,
                        factor
                    ));

                return this;
            }

            internal GaSymMapUnilinearCoefSumsTerm RemoveFactor(int domainBasisBladeId)
            {
                var index = _factorsList.FindIndex(
                    factor =>
                        factor.Item1 == domainBasisBladeId
                );

                if (index >= 0)
                    _factorsList.RemoveAt(index);

                return this;
            }

            public IEnumerator<Tuple<int, Expr>> GetEnumerator()
            {
                return _factorsList.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _factorsList.GetEnumerator();
            }


            public override string ToString()
            {
                var composer = new ListComposer(" + ")
                {
                    FinalPrefix = TargetBasisBladeId.BasisBladeIndexedName() + ": { ",
                    FinalSuffix = " }"
                };

                composer.AddRange(TermsText);

                return composer.ToString();
            }
        }


        public static GaSymMapUnilinearCoefSums Create(int targetVSpaceDim)
        {
            return new GaSymMapUnilinearCoefSums(
                targetVSpaceDim
            );
        }

        public static GaSymMapUnilinearCoefSums Create(int domainVSpaceDim, int targetVSpaceDim)
        {
            return new GaSymMapUnilinearCoefSums(
                domainVSpaceDim,
                targetVSpaceDim
            );
        }
        

        private readonly SparseTable1D<int, GaSymMapUnilinearCoefSumsTerm> _coefSumsTable
            = new SparseTable1D<int, GaSymMapUnilinearCoefSumsTerm>();


        public override int TargetVSpaceDimension { get; }

        public override int DomainVSpaceDimension { get; }

        public override IGaSymMultivector this[int id1] 
            => MapToTemp(id1).ToMultivector();


        private GaSymMapUnilinearCoefSums(int targetVSpaceDim)
        {
            DomainVSpaceDimension = targetVSpaceDim;
            TargetVSpaceDimension = targetVSpaceDim;
        }

        private GaSymMapUnilinearCoefSums(int domainVSpaceDim, int targetVSpaceDim)
        {
            DomainVSpaceDimension = domainVSpaceDim;
            TargetVSpaceDimension = targetVSpaceDim;
        }


        public GaSymMapUnilinearCoefSums SetBasisBladeMap(int basisBladeId, IGaSymMultivector targetMv)
        {
            if (ReferenceEquals(targetMv, null))
                return this;

            foreach (var term in targetMv.NonZeroExprTerms)
                SetFactor(term.Key, basisBladeId, term.Value);

            return this;
        }

        public GaSymMapUnilinearCoefSums SetFactor(int targetBasisBladeId, int domainBasisBladeId, bool isNegative = false)
        {
            GaSymMapUnilinearCoefSumsTerm sum;

            if (!_coefSumsTable.TryGetValue(targetBasisBladeId, out sum))
            {
                sum = new GaSymMapUnilinearCoefSumsTerm(targetBasisBladeId);
                _coefSumsTable[targetBasisBladeId] = sum;
            }

            sum.AddFactor(domainBasisBladeId, isNegative);

            return this;
        }

        public GaSymMapUnilinearCoefSums SetFactor(int targetBasisBladeId, int domainBasisBladeId, Expr factorValue)
        {
            GaSymMapUnilinearCoefSumsTerm sum;

            if (!_coefSumsTable.TryGetValue(targetBasisBladeId, out sum))
            {
                sum = new GaSymMapUnilinearCoefSumsTerm(targetBasisBladeId);
                _coefSumsTable[targetBasisBladeId] = sum;
            }

            sum.AddFactor(domainBasisBladeId, factorValue);

            return this;
        }

        public GaSymMapUnilinearCoefSums RemoveFactor(int targetBasisBladeId, int domainBasisBladeId)
        {
            GaSymMapUnilinearCoefSumsTerm sum;
            if (!_coefSumsTable.TryGetValue(targetBasisBladeId, out sum))
                return this;

            sum.RemoveFactor(domainBasisBladeId);

            return this;
        }

        public GaSymMapUnilinearCoefSums RemoveFactors(int targetBasisBladeId)
        {
            _coefSumsTable.Remove(targetBasisBladeId);

            return this;
        }


        public override IGaSymMultivectorTemp MapToTemp(int id1)
        {
            var mv1 = GaSymMultivector.CreateBasisBlade(TargetGaSpaceDimension, id1);

            return MapToTemp(mv1);
        }

        public override IGaSymMultivectorTemp MapToTemp(GaSymMultivector mv1)
        {
            if (mv1.GaSpaceDimension != DomainGaSpaceDimension)
                throw new GMacSymbolicException("Multivector size mismatch");

            var resultMv = GaSymMultivector.CreateZeroTemp(TargetGaSpaceDimension);

            foreach (var termValue in _coefSumsTable.Values)
                resultMv.AddFactor(
                    termValue.TargetBasisBladeId,
                    termValue[mv1]
                );

            return resultMv;
        }

        public override IEnumerable<Tuple<int, IGaSymMultivector>> BasisBladeMaps()
        {
            for (var id1 = 0; id1 < DomainGaSpaceDimension; id1++)
            {
                var mv = this[id1];

                if (!mv.IsNullOrZero())
                    yield return new Tuple<int, IGaSymMultivector>(id1, mv);
            }
        }
    }
}
