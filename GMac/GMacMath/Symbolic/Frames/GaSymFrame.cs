﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using GMac.GMacMath.Symbolic.Maps;
using GMac.GMacMath.Symbolic.Maps.Bilinear;
using GMac.GMacMath.Symbolic.Metrics;
using GMac.GMacMath.Symbolic.Multivectors;
using SymbolicInterface.Mathematica;
using SymbolicInterface.Mathematica.Expression;
using TextComposerLib.Text;
using TextComposerLib.Text.Structured;
using Wolfram.NETLink;

namespace GMac.GMacMath.Symbolic.Frames
{
    public abstract class GaSymFrame : ISymbolicObject, IGMacFrame
    {
        #region Static Members
        private static readonly Dictionary<string, GaSymFrame> OrthonormalFramesCache 
            = new Dictionary<string, GaSymFrame>();

        private static void VerifyVSpaceDim(int vSpaceDim, int minVSpaceDim = 1)
        {
            if (vSpaceDim < minVSpaceDim || vSpaceDim > GMacMathUtils.MaxVSpaceDimension)
                throw new GMacSymbolicException("Invalid vector space dimension");
        }

        private static string GetOrthonormalSignatureString(int vSpaceDim)
        {
            return "".PadLeft(vSpaceDim, '+');
        }

        /// <summary>
        /// Create a non orthogonal frame based on a symmetric inner product matrix
        /// </summary>
        /// <param name="ipm"></param>
        /// <returns></returns>
        private static GaSymFrameNonOrthogonal CreateNonOrthogonalFromIpm(MathematicaMatrix ipm)
        {
            MathematicaVector eigenValues;
            MathematicaMatrix eigenVectors;

            if (ipm.EigenSystem(MathematicaMatrix.EigenVectorsSpecs.OrthogonalInMatrixColumns, out eigenValues, out eigenVectors) == false)
                throw new GMacSymbolicException("Cannot obtain orthogonal eigen system of the given matrix");

            var baseFrame = CreateOrthogonal(eigenValues);

            //var baseToDerivedOm = GaSymOutermorphismHash.Create(eigenVectors);
            //var derivedToBaseOm = GaSymOutermorphismHash.Create(eigenVectors.Transpose());

            var baseToDerivedOm = eigenVectors.ToOutermorphismTree();
            var derivedToBaseOm = eigenVectors.Transpose().ToOutermorphismTree();

            return new GaSymFrameNonOrthogonal(baseFrame, ipm, derivedToBaseOm, baseToDerivedOm);
        }


        public static GaSymFrameEuclidean CreateEuclidean(int vSpaceDim)
        {
            VerifyVSpaceDim(vSpaceDim);

            GaSymFrame gaFrame;
            var frameSig = GetOrthonormalSignatureString(vSpaceDim);

            if (OrthonormalFramesCache.TryGetValue(frameSig, out gaFrame))
                return (GaSymFrameEuclidean)gaFrame;

            gaFrame = new GaSymFrameEuclidean(vSpaceDim);

            OrthonormalFramesCache.Add(frameSig, gaFrame);

            return (GaSymFrameEuclidean)gaFrame;
        }

        public static GaSymFrameNonOrthogonal CreateConformal(int vSpaceDim)
        {
            VerifyVSpaceDim(vSpaceDim, 3);

            var eSpaceDim = vSpaceDim - 2;
            var listComposer = new ListComposer(",")
            {
                FinalPrefix = "{",
                FinalSuffix = "}"
            };

            for (var i = 0; i < eSpaceDim; i++)
                listComposer.Add(
                    (1 << i)
                    .PatternToSequence(vSpaceDim, "0", "1")
                    .Concatenate(",", "{", "}")
                    );

            listComposer.Add(
                "{" + 
                0.PatternToSequence(eSpaceDim, "0", "1").Concatenate(",") + 
                ",0,-1}"
            );

            listComposer.Add(
                "{" +
                0.PatternToSequence(eSpaceDim, "0", "1").Concatenate(",") +
                ",-1,0}"
            );

            var ipm = MathematicaMatrix.Create(SymbolicUtils.Cas, listComposer.ToString());

            return CreateNonOrthogonalFromIpm(ipm);
        }

        public static GaSymFrameOrthogonal CreateProjective(int vSpaceDim)
        {
            VerifyVSpaceDim(vSpaceDim, 2);

            var sigArray = Enumerable.Repeat(1, vSpaceDim).ToArray();
            sigArray[vSpaceDim - 1] = 0;

            var frameSigList = 
                sigArray.Select(
                    i => MathematicaScalar.Create(SymbolicUtils.Cas, i)
                    );

            return new GaSymFrameOrthogonal(frameSigList);
        }

        public static GaSymFrame CreateOrthonormal(IEnumerable<char> frameSigSignList)
        {
            var psgCount = 0;
            var nsgCount = 0;
            var s = new StringBuilder();
            var sigList = new List<bool>(GMacMathUtils.MaxVSpaceDimension);

            //Convert the string signature into a list of integers with values of 1 and -1.
            foreach (var c in frameSigSignList)
                switch (c)
                {
                    case '+':
                        sigList.Add(false);
                        s.Append("+");
                        psgCount++;
                        break;

                    case '-':
                        sigList.Add(true);
                        s.Append("-");
                        nsgCount++;
                        break;

                    default:
                        throw new GMacSymbolicException("Invalid orthonormal frame signature");
                }

            var vSpaceDim = psgCount + nsgCount;

            VerifyVSpaceDim(vSpaceDim);

            var frameSig = s.ToString();
            GaSymFrame gaFrame;

            if (OrthonormalFramesCache.TryGetValue(frameSig, out gaFrame))
                return gaFrame;

            //The frame signature has no vectors with negative signature; create a Euclidean frame
            if (nsgCount == 0)
                gaFrame = new GaSymFrameEuclidean(vSpaceDim);

            //Else create an orthonormal frame
            else
                gaFrame = new GaSymFrameOrthonormal(sigList.ToArray());

            OrthonormalFramesCache.Add(frameSig, gaFrame);

            return gaFrame;
        }

        public static GaSymFrame CreateOrthonormal(IEnumerable<int> frameSigList)
        {
            var psgCount = 0;
            var nsgCount = 0;
            var sigList = new List<bool>(GMacMathUtils.MaxVSpaceDimension);
            var frameSig = new StringBuilder(GMacMathUtils.MaxVSpaceDimension);

            //Convert the string signature into a list of integers with values of 1 and -1.
            foreach (var s in frameSigList)
                switch (s)
                {
                    case 1:
                        frameSig.Append('+');
                        sigList.Add(false);
                        psgCount++;
                        break;

                    case -1:
                        frameSig.Append('-');
                        sigList.Add(true);
                        nsgCount++;
                        break;

                    default:
                        throw new GMacSymbolicException("Invalid orthonormal frame signature");
                }

            var vSpaceDim = psgCount + nsgCount;

            VerifyVSpaceDim(vSpaceDim);

            GaSymFrame gaFrame;

            if (OrthonormalFramesCache.TryGetValue(frameSig.ToString(), out gaFrame))
                return gaFrame;

            //The frame signature has no vectors with negative signature; create a Euclidean frame
            if (nsgCount == 0)
                gaFrame = new GaSymFrameEuclidean(vSpaceDim);

            //Else create an orthonormal frame
            else
                gaFrame = new GaSymFrameOrthonormal(sigList);

            OrthonormalFramesCache.Add(frameSig.ToString(), gaFrame);

            return gaFrame;
        }

        public static GaSymFrame CreateOrthogonal(IEnumerable<MathematicaScalar> frameSigList)
        {
            var frameSigArray = frameSigList.ToArray();

            var vSpaceDim = 0;

            //Positive and negative unity signature vectors count
            var pusvCount = 0;
            var nusvCount = 0;

            //Orthonormal Basis Vectors Signatures
            var onSigList = new List<char>();

            foreach (var bvSig in frameSigArray)
            {
                //if (ReferenceEquals(bvSig, null) || bvSig.IsNonZeroRealConstant() == false)
                if (ReferenceEquals(bvSig, null) || bvSig.IsRealConstant() == false)
                    throw new GMacSymbolicException("Invalid basis vector signature");

                if (bvSig.IsEqualScalar(SymbolicUtils.Constants.One))
                {
                    pusvCount++;
                    onSigList.Add('+');
                }
                else if (bvSig.IsEqualScalar(SymbolicUtils.Constants.One))
                {
                    nusvCount++;
                    onSigList.Add('-');
                }

                vSpaceDim++;
            }

            //Test if an orthonormal basis is given
            return
                vSpaceDim == (pusvCount + nusvCount)
                ? CreateOrthonormal(onSigList)
                : new GaSymFrameOrthogonal(frameSigArray);
        }

        public static GaSymFrame CreateFromIpm(ISymbolicMatrix ipm)
        {
            if (ipm.IsSymmetric() == false)
                throw new GMacSymbolicException("Inner product matrix must be symmetric");

            if (ipm.IsIdentity())
                return CreateEuclidean(ipm.RowCount);

            return
                ipm.IsDiagonal()
                ? CreateOrthogonal(ipm.GetDiagonal())
                : CreateNonOrthogonalFromIpm(ipm.ToMathematicaMatrix());
        }

        /// <summary>
        /// Create a derived frame system using a change of basis matrix for the basis vectors
        /// </summary>
        /// <param name="baseFrame">The base frame. It may be any kind of frame</param>
        /// <param name="cbm">The 'Change Of Basis Vectors' matrix. It must be invertable</param>
        /// <returns></returns>
        public static GaSymMetricNonOrthogonal CreateDerivedCbmFrameSystem(GaSymFrame baseFrame, MathematicaMatrix cbm)
        {
            var baseIpm = baseFrame.Ipm.ToMathematicaMatrix();
            var cbmTrans = (MathematicaMatrix)cbm.Transpose();
            var cbmInverseTrans = (MathematicaMatrix)cbmTrans.Inverse();

            var ipm = cbm * baseIpm * cbmTrans;

            var baseToDerivedCba = cbmInverseTrans.ToOutermorphismTree();
            var derivedToBaseCba = cbmTrans.ToOutermorphismTree();

            if (ipm.IsDiagonal())
            {
                var derivedFrame = CreateOrthogonal(ipm.GetDiagonal());
                return new GaSymMetricNonOrthogonal(baseFrame, derivedFrame, derivedToBaseCba, baseToDerivedCba);
            }

            if (baseFrame.IsOrthogonal)
            {
                var derivedFrame = new GaSymFrameNonOrthogonal(baseFrame, ipm, derivedToBaseCba, baseToDerivedCba);
                return derivedFrame.NonOrthogonalMetric;
            }

            var gaFrame =
                //new GaFrameNonOrthogonal(baseFrame, ipm, derivedToBaseOm, baseToDerivedOm);
                CreateNonOrthogonalFromIpm(ipm.ToMathematicaMatrix());

            return gaFrame.NonOrthogonalMetric;
            //return new DerivedFrameSystem(baseFrame, gaFrame, derivedToBaseOm, baseToDerivedOm);
        }

        /// <summary>
        /// Create a derived frame system where the derived frame is the reciprocal of the base frame
        /// </summary>
        /// <param name="baseFrame"></param>
        /// <returns></returns>
        public static GaSymMetricNonOrthogonal CreateReciprocalCbmFrameSystem(GaSymFrame baseFrame)
        {
            if (baseFrame.IsOrthogonal)
            {
                var cbmat = baseFrame.Ipm.Inverse();

                var b2DOm = baseFrame.Ipm.ToOutermorphismTree();
                var d2BOm = cbmat.ToOutermorphismTree();

                var derivedFrame = CreateOrthogonal(cbmat.GetDiagonal());
                return new GaSymMetricNonOrthogonal(baseFrame, derivedFrame, d2BOm, b2DOm);
            }

            var cbmArray = new MathematicaScalar[baseFrame.VSpaceDimension, baseFrame.VSpaceDimension];

            var mv2 = baseFrame.CreateInverseUnitPseudoScalar();

            for (var i = 0; i < baseFrame.VSpaceDimension; i++)
            {
                var id = (1 << i) ^ baseFrame.MaxBasisBladeId;
                var mv1 = GaSymMultivector.CreateTerm(baseFrame.GaSpaceDimension, id, SymbolicUtils.Constants.One);

                var mv = baseFrame.Lcp[mv1, mv2];

                foreach (var term in mv.NonZeroTerms)
                {
                    var j = term.Key.BasisBladeIndex();

                    if ((i & 1) == 1)
                        cbmArray[i, j] = term.Value;
                    else
                        cbmArray[i, j] = -term.Value;
                }
            }

            var cbm = MathematicaMatrix.CreateFullMatrix(SymbolicUtils.Cas, cbmArray);

            return CreateDerivedCbmFrameSystem(baseFrame, cbm);
        }
        #endregion


        /// <summary>
        /// Inner-Product Matrix
        /// </summary>
        protected ISymbolicMatrix InnerProductMatrix;

        public MathematicaScalar UnitPseudoScalarCoef { get; protected set; }

        public MathematicaInterface CasInterface { get; }

        public MathematicaConnection CasConnection => CasInterface.Connection;

        public MathematicaEvaluator CasEvaluator => CasInterface.Evaluator;

        public MathematicaConstants CasConstants => CasInterface.Constants;

        public ISymbolicMatrix Ipm
        {
            get
            {
                if (ReferenceEquals(InnerProductMatrix, null))
                    ComputeIpm();

                return InnerProductMatrix;
            }
        }

        public abstract int VSpaceDimension { get; }

        public int MaxBasisBladeId => GMacMathUtils.MaxBasisBladeId(VSpaceDimension);

        public int GaSpaceDimension => VSpaceDimension.ToGaSpaceDimension();

        public int GradesCount => VSpaceDimension + 1;

        public bool IsNonOrthogonal => !IsOrthogonal;


        /// <summary>
        /// A fixed computed bilinear map implementation of the Outer product of this frame
        /// </summary>
        public abstract IGaSymMapBilinear ComputedOp { get; }

        /// <summary>
        /// A fixed computed bilinear map implementation of the Geometric product of this frame
        /// </summary>
        public abstract IGaSymMapBilinear ComputedGp { get; }

        /// <summary>
        /// A fixed computed bilinear map implementation of the Scalar product of this frame
        /// </summary>
        public abstract IGaSymMapBilinear ComputedSp { get; }

        /// <summary>
        /// A fixed computed bilinear map implementation of the Left Contraction product of this frame
        /// </summary>
        public abstract IGaSymMapBilinear ComputedLcp { get; }

        /// <summary>
        /// A fixed computed bilinear map implementation of the Right Contraction product of this frame
        /// </summary>
        public abstract IGaSymMapBilinear ComputedRcp { get; }

        /// <summary>
        /// A fixed computed bilinear map implementation of the Fat-Dor Contraction product of this frame
        /// </summary>
        public abstract IGaSymMapBilinear ComputedFdp { get; }

        /// <summary>
        /// A fixed computed bilinear map implementation of the Hestenes Inner product of this frame
        /// </summary>
        public abstract IGaSymMapBilinear ComputedHip { get; }

        /// <summary>
        /// A fixed computed bilinear map implementation of the Anti-Commutator product of this frame
        /// </summary>
        public abstract IGaSymMapBilinear ComputedAcp { get; }

        /// <summary>
        /// A fixed computed bilinear map implementation of the Commutator product of this frame
        /// </summary>
        public abstract IGaSymMapBilinear ComputedCp { get; }


        /// <summary>
        /// A bilinear map implementation of the Outer product of this frame
        /// </summary>
        public IGaSymMapBilinear Op { get; internal set; }

        /// <summary>
        /// A bilinear map implementation of the Geometrc product of this frame
        /// </summary>
        public IGaSymMapBilinear Gp { get; internal set; }

        /// <summary>
        /// A bilinear map implementation of the Scalar product of this frame
        /// </summary>
        public IGaSymMapBilinear Sp { get; internal set; }

        /// <summary>
        /// A bilinear map implementation of the Left Contraction product product of this frame
        /// </summary>
        public IGaSymMapBilinear Lcp { get; internal set; }

        /// <summary>
        /// A bilinear map implementation of the Right Contraction product product of this frame
        /// </summary>
        public IGaSymMapBilinear Rcp { get; internal set; }

        /// <summary>
        /// A bilinear map implementation of the Fat-Dot product of this frame
        /// </summary>
        public IGaSymMapBilinear Fdp { get; internal set; }

        /// <summary>
        /// A bilinear map implementation of the Hestenes Inner product of this frame
        /// </summary>
        public IGaSymMapBilinear Hip { get; internal set; }

        /// <summary>
        /// A bilinear map implementation of the Anti-Commutator product of this frame
        /// </summary>
        public IGaSymMapBilinear Acp { get; internal set; }

        /// <summary>
        /// A bilinear map implementation of the Commutator product of this frame
        /// </summary>
        public IGaSymMapBilinear Cp { get; internal set; }


        public abstract IGaSymMetric Metric { get; }

        public abstract IGaSymMetricOrthogonal BaseOrthogonalMetric { get; }

        public abstract GaSymFrame BaseOrthogonalFrame { get; }

        public abstract bool IsEuclidean { get; }

        public abstract bool IsOrthogonal { get; }

        public abstract bool IsOrthonormal { get; }


        protected GaSymFrame()
        {
            CasInterface = SymbolicUtils.Cas;
        }


        private void SetProductsImplementationToComputed()
        {
            Op = ComputedOp;
            Gp = ComputedGp;
            Sp = ComputedSp;
            Lcp = ComputedLcp;
            Rcp = ComputedRcp;
            Fdp = ComputedFdp;
            Hip = ComputedHip;
            Acp = ComputedAcp;
            Cp = ComputedCp;
        }

        private void SetProductsImplementationToLookupArray()
        {
            Op = ComputedOp.ToArrayMap();
            Gp = ComputedGp.ToArrayMap();
            Sp = ComputedSp.ToArrayMap();
            Lcp = ComputedLcp.ToArrayMap();
            Rcp = ComputedRcp.ToArrayMap();
            Fdp = ComputedFdp.ToArrayMap();
            Hip = ComputedHip.ToArrayMap();
            Acp = ComputedAcp.ToArrayMap();
            Cp = ComputedCp.ToArrayMap();
        }

        private void SetProductsImplementationToLookupHash()
        {
            Op = ComputedOp.ToHashMap();
            Gp = ComputedGp.ToHashMap();
            Sp = ComputedSp.ToHashMap();
            Lcp = ComputedLcp.ToHashMap();
            Rcp = ComputedRcp.ToHashMap();
            Fdp = ComputedFdp.ToHashMap();
            Hip = ComputedHip.ToHashMap();
            Acp = ComputedAcp.ToHashMap();
            Cp = ComputedCp.ToHashMap();
        }

        private void SetProductsImplementationToLookupTree()
        {
            Op = ComputedOp.ToTreeMap();
            Gp = ComputedGp.ToTreeMap();
            Sp = ComputedSp.ToTreeMap();
            Lcp = ComputedLcp.ToTreeMap();
            Rcp = ComputedRcp.ToTreeMap();
            Fdp = ComputedFdp.ToTreeMap();
            Hip = ComputedHip.ToTreeMap();
            Acp = ComputedAcp.ToTreeMap();
            Cp = ComputedCp.ToTreeMap();
        }

        private void SetProductsImplementationToLookupCoefSums()
        {
            Op = ComputedOp.ToCoefSumsMap();
            Gp = ComputedGp.ToCoefSumsMap();
            Sp = ComputedSp.ToCoefSumsMap();
            Lcp = ComputedLcp.ToCoefSumsMap();
            Rcp = ComputedRcp.ToCoefSumsMap();
            Fdp = ComputedFdp.ToCoefSumsMap();
            Hip = ComputedHip.ToCoefSumsMap();
            Acp = ComputedAcp.ToCoefSumsMap();
            Cp = ComputedCp.ToCoefSumsMap();
        }

        /// <summary>
        /// Select and initialize an implementation method for the bilinear products of this frame
        /// </summary>
        /// <param name="method"></param>
        public void SetProductsImplementation(GaBilinearProductImplementation method)
        {
            switch (method)
            {
                case GaBilinearProductImplementation.LookupArray:
                    SetProductsImplementationToLookupArray();
                    break;

                case GaBilinearProductImplementation.LookupHash:
                    SetProductsImplementationToLookupHash();
                    break;

                case GaBilinearProductImplementation.LookupTree:
                    SetProductsImplementationToLookupTree();
                    break;

                case GaBilinearProductImplementation.LookupCoefSums:
                    SetProductsImplementationToLookupCoefSums();
                    break;

                case GaBilinearProductImplementation.Computed:
                default:
                    SetProductsImplementationToComputed();
                    break;
            }
        }

        protected abstract void ComputeIpm();

        public abstract MathematicaScalar BasisVectorSignature(int basisVectorIndex);

        public abstract GaSymMultivector BasisBladeSignature(int id);


        public GaSymMultivector CreateUnitPseudoScalar()
        {
            return GaSymMultivector.CreateBasisBlade(GaSpaceDimension, MaxBasisBladeId);
        }

        public GaSymMultivector CreateInverseUnitPseudoScalar()
        {
            //TODO: Review this computation
            return GaSymMultivector.CreateTerm(GaSpaceDimension, MaxBasisBladeId, UnitPseudoScalarCoef);
        }

        public MathematicaScalar Magnitude(GaSymMultivector mv)
        {
            return
                mv
                .GetKVectorParts()
                .Select(pair => Sp[pair.Value, pair.Value.Reverse()])
                .Aggregate(
                    CasConstants.Zero, 
                    (current, mv1) => current + mv1[0].ToMathematicaScalar().Abs()
                )
                .Sqrt();
        }

        public MathematicaScalar Magnitude2(GaSymMultivector mv)
        {
            return 
                mv
                .GetKVectorParts()
                .Select(pair => Sp[pair.Value, pair.Value.Reverse()])
                .Aggregate(
                    CasConstants.Zero, 
                    (current, mv1) => current + mv1[0].ToMathematicaScalar().Abs()
                );
        }

        public MathematicaScalar Norm2(GaSymMultivector mv)
        {
            return Sp[mv, mv.Reverse()][0].ToMathematicaScalar();
        }

        /// <summary>
        /// Odd Versor Product of a list of basis blades given by their IDs
        /// </summary>
        /// <param name="oddVersor"></param>
        /// <param name="basisBladeIDs"></param>
        /// <returns></returns>
        public IEnumerable<GaSymMultivector> OddVersorProduct(GaSymMultivector oddVersor, IEnumerable<int> basisBladeIDs)
        {
            var oddVersorReverse = oddVersor.Reverse();
            var oddVersorNorm2Inverse = 
                SymbolicUtils.Constants.One / Sp[oddVersor, oddVersorReverse][0]
                    .ToMathematicaScalar();
            var oddVersorInverse = oddVersorReverse * oddVersorNorm2Inverse;

            return basisBladeIDs.Select(id =>
            {
                var mv = GaSymMultivector.CreateTerm(
                    GaSpaceDimension,
                    id,
                    id.BasisBladeIdHasNegativeGradeInv() ? Expr.INT_MINUSONE : Expr.INT_ONE
                );

                return
                    Gp[Gp[oddVersor, mv], oddVersorInverse]
                        .GetKVectorPart(id.BasisBladeGrade());
            });
        }

        public IEnumerable<GaSymMultivector> RotorProduct(GaSymMultivector rotorVersor, IEnumerable<int> basisBladeIDs)
        {
            var rotorVersorInverse = rotorVersor.Reverse();

            return basisBladeIDs.Select(id =>
            {
                var mv = GaSymMultivector.CreateBasisBlade(GaSpaceDimension, id);

                return
                    Gp[Gp[rotorVersor, mv], rotorVersorInverse]
                        .GetKVectorPart(id.BasisBladeGrade());
            });
        }

        public IEnumerable<GaSymMultivector> EvenVersorProduct(GaSymMultivector evenVersor, IEnumerable<int> basisBladeIDs)
        {
            var evenVersorReverse = evenVersor.Reverse();
            var evenVersorNorm2Inverse = 
                SymbolicUtils.Constants.One / Sp[evenVersor, evenVersorReverse][0]
                    .ToMathematicaScalar();
            var evenVersorInverse = evenVersorReverse * evenVersorNorm2Inverse;

            return basisBladeIDs.Select(id =>
            {
                var mv = GaSymMultivector.CreateBasisBlade(GaSpaceDimension, id);

                return
                    Gp[Gp[evenVersor, mv], evenVersorInverse]
                        .GetKVectorPart(id.BasisBladeGrade());
            });
        }

        /// <summary>
        /// Odd Versor Product
        /// </summary>
        /// <param name="oddVersor"></param>
        /// <param name="mv"></param>
        /// <returns></returns>
        public GaSymMultivector OddVersorProduct(GaSymMultivector oddVersor, GaSymMultivector mv)
        {
            var oddVersorReverse = oddVersor.Reverse();
            var oddVersorNorm2Inverse = 
                SymbolicUtils.Constants.One / Sp[oddVersor, oddVersorReverse][0]
                .ToMathematicaScalar();
            var oddVersorInverse = oddVersorReverse * oddVersorNorm2Inverse;

            return Gp[Gp[oddVersor, mv.GradeInv()], oddVersorInverse];
        }

        /// <summary>
        /// Even Versor Product
        /// </summary>
        /// <param name="evenVersor"></param>
        /// <param name="mv"></param>
        /// <returns></returns>
        public GaSymMultivector EvenVersorProduct(GaSymMultivector evenVersor, GaSymMultivector mv)
        {
            var evenVersorReverse = evenVersor.Reverse();
            var evenVersorNorm2Inverse = 
                SymbolicUtils.Constants.One / Sp[evenVersor, evenVersorReverse][0]
                    .ToMathematicaScalar();
            var evenVersorInverse = evenVersorReverse * evenVersorNorm2Inverse;

            return Gp[Gp[evenVersor, mv], evenVersorInverse];
        }

        /// <summary>
        /// Rotor Product
        /// </summary>
        /// <param name="rotorVersor"></param>
        /// <param name="mv"></param>
        /// <returns></returns>
        public GaSymMultivector RotorProduct(GaSymMultivector rotorVersor, GaSymMultivector mv)
        {
            return Gp[Gp[rotorVersor, mv], rotorVersor.Reverse()];
        }
    }
}
