﻿using System;
using System.Collections.Generic;
using GMac.GMacCompiler.Semantic.AST;
using GMac.GMacCompiler.Syntax;
using GMac.GMacMath;
using Irony.Parsing;
using IronyGrammars.SourceCode;
using UtilLib.DataStructures;

namespace GMac.GMacCompiler.Semantic.ASTGenerator
{
    /// <summary>
    /// Translate the boolean pattern of a GMac frame subspace
    /// </summary>
    internal sealed class GMacFrameSubspacePatternGenerator : GMacAstSymbolGenerator
    {
        #region Static Members

        /// <summary>
        /// Translate the boolean pattern of a GMac frame subspace
        /// </summary>
        /// <param name="context"></param>
        /// <param name="node"></param>
        /// <param name="frame"></param>
        /// <returns></returns>
        public static MutableBooleanPattern Translate(GMacSymbolTranslatorContext context, ParseTreeNode node, GMacFrame frame)
        {
            context.PushState(node);

            var translator = new GMacFrameSubspacePatternGenerator();//new GMacFrameSubspacePatternGenerator(context, frame);

            translator.SetContext(context, frame);
            translator.Translate();

            context.PopState();

            var result = translator._generatedBooleanPattern;

            //MasterPool.Release(translator);

            return result;
        }

        #endregion


        private GMacFrame _frame;

        ///The generated list of basis blade ids
        private MutableBooleanPattern _generatedBooleanPattern;


        //public override void ResetOnAcquire()
        //{
        //    base.ResetOnAcquire();

        //    _frame = null;
        //    _generatedBooleanPattern = null;
        //}


        private void SetContext(GMacSymbolTranslatorContext context, GMacFrame frame)
        {
            SetContext(context);
            _frame = frame;
            _generatedBooleanPattern = new MutableBooleanPattern(frame.GaSpaceDimension, false);
        }


        private void AddBasisBladeId(int basisBladeId)
        {
            _generatedBooleanPattern[basisBladeId] = true;
        }

        private void AddBasisBladeIDs(GMacFrameSubspace subspace)
        {
            _generatedBooleanPattern.OrWith(subspace.SubspaceSignaturePattern);
        }

        private void translate_PredefinedBasisBladeIDs(string identName, ParseTreeNode node)
        {
            var firstChar = identName[0];

            switch (firstChar)
            {
                case 'E':
                {
                    int id;
                    if (Int32.TryParse(identName.Substring(1), out id) && _frame.IsValidBasisBladeId(id))
                        AddBasisBladeId(id);

                    else
                        CompilationLog.RaiseGeneratorError<int>("Basis blades set not recognized", node);
                }
                    break;

                case 'B':
                {
                    var id = identName.Substring(1).StringToPattern();

                    if (_frame.IsValidBasisBladeId(id))
                        AddBasisBladeId(id);

                    else
                        CompilationLog.RaiseGeneratorError<int>("Basis blades set not recognized", node);
                }
                    break;

                case 'G':
                {
                    var pos = identName.IndexOf('I');

                    if (pos < 2 || pos == identName.Length - 1)
                        CompilationLog.RaiseGeneratorError<int>("Basis blades set not recognized", node);

                    var gradeText = identName.Substring(1, pos - 1);
                    var indexText = identName.Substring(pos + 1);

                    int grade, index;

                    if (
                        Int32.TryParse(gradeText, out grade) && 
                        Int32.TryParse(indexText, out index) && 
                        _frame.IsValidBasisBladeGradeIndex(grade, index)
                        )
                        AddBasisBladeId(GMacMathUtils.BasisBladeId(grade, index));

                    else
                        CompilationLog.RaiseGeneratorError<int>("Basis blades set not recognized", node);
                }
                    break;

                default:
                    CompilationLog.RaiseGeneratorError<int>("Basis blades set not recognized", node);
                    break;
            }
        }

        private void translate_Outerproduct_List_SingleIdentifier(ParseTreeNode node)
        {
            var identName = GenUtils.Translate_Identifier(node);

            GMacFrameSubspace subspace;
            if (_frame.LookupSubspace(identName, out subspace))
                AddBasisBladeIDs(subspace);

            else
            {
                GMacFrameBasisVector basisVector;
                if (_frame.LookupBasisVector(identName, out basisVector))
                    AddBasisBladeId(basisVector.BasisVectorId);

                else
                    translate_PredefinedBasisBladeIDs(identName, node);
            }
        }

        private void translate_Outerproduct_List(ParseTreeNode node)
        {
            if (node.ChildNodes.Count == 1)
            {
                translate_Outerproduct_List_SingleIdentifier(node.ChildNodes[0]);
            }
            else if (node.ChildNodes.Count > 1)
            {
                var basisVectorsList = new List<GMacFrameBasisVector>(node.ChildNodes.Count);
                var basisBladeId = 0;

                foreach (var nodeIdentifier in node.ChildNodes)
                {
                    GMacFrameBasisVector basisVector;
                    var basisVectorName = GenUtils.Translate_Identifier(nodeIdentifier);

                    if (_frame.LookupBasisVector(basisVectorName, out basisVector) == false)
                        CompilationLog.RaiseGeneratorError<int>("Basis vector not recognized", nodeIdentifier);

                    if (basisVectorsList.Exists(x => x.BasisVectorId == basisVector.BasisVectorId))
                        CompilationLog.RaiseGeneratorError<int>("Basis blades set not recognized", node);

                    basisVectorsList.Add(basisVector);
                    basisBladeId = basisBladeId | basisVector.BasisVectorId;
                }

                AddBasisBladeId(basisBladeId);
            }
            else
            {
                CompilationLog.RaiseGeneratorError<int>("Basis blades set not recognized", node);
            }
        }

        private void translate_BasisBladesSet_List_Item_GASpan(ParseTreeNode node)
        {
            var nodeIdentifierList = node.ChildNodes[0];

            //Create the set of unique spanning vectors for the GA subspace
            var basisVectorsList = new List<int>(nodeIdentifierList.ChildNodes.Count);

            foreach (var nodeIdentifier in nodeIdentifierList.ChildNodes)
            {
                GMacFrameBasisVector basisVector;
                var basisVectorName = GenUtils.Translate_Identifier(nodeIdentifier);

                if (_frame.LookupBasisVector(basisVectorName, out basisVector) == false)
                    CompilationLog.RaiseGeneratorError<int>("Basis vector not recognized", nodeIdentifier);

                //Only add unique basis vectors to the spanning set
                if (basisVectorsList.Exists(x => x == basisVector.BasisVectorId) == false)
                    basisVectorsList.Add(basisVector.BasisVectorId);
            }

            //Compute the dimension of the GA spanned by the basis vectors
            var subspaceDimension = 1 << basisVectorsList.Count;

            //Scalars are always part of the GA subspace based on any given set of basis vectors
            AddBasisBladeId(0);

            //Add the remaining basis blades to the GA subspace
            for (var idIndex = 1; idIndex <= subspaceDimension - 1; idIndex++)
            {
                var id = GMacMathUtils.ComposeGaSubspaceBasisBladeId(basisVectorsList, idIndex);

                AddBasisBladeId(id);
            }
        }

        private void translate_BasisBladesSet(ParseTreeNode node)
        {
            var nodeBasisBladesSetList = node.ChildNodes[0];

            foreach (var nodeBasisBladesSetListItem in nodeBasisBladesSetList.ChildNodes)
            {
                var subnode = nodeBasisBladesSetListItem.ChildNodes[0];

                switch (subnode.Term.ToString())
                {
                    case GMacParseNodeNames.OuterproductList:
                        translate_Outerproduct_List(subnode);
                        break;

                    case GMacParseNodeNames.BasisBladesSetListItemGaSpan:
                        translate_BasisBladesSet_List_Item_GASpan(subnode);
                        break;

                    default:
                        CompilationLog.RaiseGeneratorError<int>("Basis blades set not recognized", nodeBasisBladesSetListItem);
                        break;
                }
            }
        }

        protected override void Translate()
        {
            switch (RootParseNode.Term.ToString())
            {
                case GMacParseNodeNames.BasisBladeCoefficient:
                    translate_Outerproduct_List(RootParseNode.ChildNodes[0]);

                    if (_generatedBooleanPattern.TrueCount != 1)
                        CompilationLog.RaiseGeneratorError<int>("Basis blade coefficient not recognized", RootParseNode);
                    break;

                case GMacParseNodeNames.BasisBladesSet:
                    translate_BasisBladesSet(RootParseNode);
                    break;

                default:
                    CompilationLog.RaiseGeneratorError<int>("Basis blades set not recognized", RootParseNode);
                    break;
            }
        }
    }
}
