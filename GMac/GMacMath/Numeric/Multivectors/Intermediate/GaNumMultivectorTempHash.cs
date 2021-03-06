﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GMac.GMacMath.Numeric.Multivectors.Intermediate
{
    public sealed class GaNumMultivectorTempHash : IGaNumMultivectorTemp
    {
        public static GaNumMultivectorTempHash Create(int gaSpaceDim)
        {
            return new GaNumMultivectorTempHash(gaSpaceDim);
        }


        private readonly Dictionary<int, double> _termsDictionary;


        public double this[int id]
        {
            get
            {
                double coef;
                _termsDictionary.TryGetValue(id, out coef);

                return coef;
            }
        }

        public double this[int grade, int index]
        {
            get
            {
                var id = GMacMathUtils.BasisBladeId(grade, index);

                double coef;
                _termsDictionary.TryGetValue(id, out coef);

                return coef;
            }
        }

        public IEnumerable<int> BasisBladeIds 
            => _termsDictionary.Keys;

        public IEnumerable<int> NonZeroBasisBladeIds
            => _termsDictionary.Where(p => !p.Value.IsNearZero()).Select(p => p.Key);

        public IEnumerable<double> BasisBladeScalars 
            => _termsDictionary.Values;

        public IEnumerable<double> NonZeroBasisBladeScalars
            => _termsDictionary.Values.Where(v => !v.IsNearZero());

        public IEnumerable<KeyValuePair<int, double>> Terms
            => _termsDictionary;

        public int VSpaceDimension 
            => GaSpaceDimension.ToVSpaceDimension();

        public int GaSpaceDimension { get; }

        public IEnumerable<KeyValuePair<int, double>> NonZeroTerms
            => _termsDictionary.Where(p => !p.Value.IsNearZero());

        public bool IsTemp => true;

        public int TermsCount => _termsDictionary.Count;


        public bool IsTerm()
        {
            return _termsDictionary.Count(v => !v.Value.IsNearZero()) <= 1;
        }

        public bool IsScalar()
        {
            return !_termsDictionary.Any(p => p.Key != 0 && !p.Value.IsNearZero());
        }

        public bool IsZero()
        {
            return _termsDictionary.All(p => p.Value.IsNearZero());
        }

        public bool IsNearZero(double epsilon)
        {
            return _termsDictionary.All(p => p.Value.IsNearZero(epsilon));
        }

        public bool ContainsBasisBlade(int id)
        {
            return _termsDictionary.ContainsKey(id);
        }


        private GaNumMultivectorTempHash(int gaSpaceDim)
        {
            GaSpaceDimension = gaSpaceDim;
            _termsDictionary = new Dictionary<int, double>();
        }


        public IGaNumMultivectorTemp AddFactor(int id, double coef)
        {
            double oldCoef;
            if (_termsDictionary.TryGetValue(id, out oldCoef))
                _termsDictionary[id] = oldCoef + coef;
            else
                _termsDictionary.Add(id, coef);

            return this;
        }

        public IGaNumMultivectorTemp AddFactor(int id, bool isNegative, double coef)
        {
            double oldCoef;
            if (_termsDictionary.TryGetValue(id, out oldCoef))
                _termsDictionary[id] = isNegative ? oldCoef - coef : oldCoef + coef;
            else
                _termsDictionary.Add(id, isNegative ? -coef : coef);

            return this;
        }

        public IGaNumMultivectorTemp SetTermCoef(int id, double coef)
        {
            if (_termsDictionary.ContainsKey(id))
                _termsDictionary[id] = coef;
            else
                _termsDictionary.Add(id, coef);

            return this;
        }

        public IGaNumMultivectorTemp SetTermCoef(int id, bool isNegative, double coef)
        {
            if (_termsDictionary.ContainsKey(id))
                _termsDictionary[id] = isNegative ? -coef : coef;
            else
                _termsDictionary.Add(id, isNegative ? -coef : coef);

            return this;
        }

        public void Simplify()
        {
            var idsList = new List<int>(
                _termsDictionary
                    .Where(p => p.Value.IsNearZero())
                    .Select(p => p.Key)
                );

            foreach (var id in idsList)
                _termsDictionary.Remove(id);
        }

        public double[] TermsToArray()
        {
            var scalarsArray = new double[GaSpaceDimension];

            foreach (var term in _termsDictionary)
                if (!term.Value.IsNearZero())
                    scalarsArray[term.Key] = term.Value;

            return scalarsArray;
        }

        public GaNumMultivector ToMultivector()
        {
            var mv = GaNumMultivector.CreateZero(GaSpaceDimension);

            foreach (var term in _termsDictionary.Where(p => !p.Value.IsNearZero()))
                mv.SetTermCoef(term.Key, term.Value);

            return mv;
        }

        public GaNumMultivector GetVectorPart()
        {
            var mv = GaNumMultivector.CreateZero(GaSpaceDimension);

            foreach (var id in GMacMathUtils.BasisVectorIDs(VSpaceDimension))
            {
                var coef = this[id];
                if (!coef.IsNearZero())
                    mv.SetTermCoef(id, coef);
            }

            return mv;
        }

        public IEnumerator<KeyValuePair<int, double>> GetEnumerator()
        {
            return NonZeroTerms.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return NonZeroTerms.GetEnumerator();
        }
    }
}