﻿using System.Collections.Generic;
using System.Linq;
using GMac.GMacAPI.CodeBlock;
using GMac.GMacMath.Symbolic;
using Wolfram.NETLink;

namespace GMac.GMacCompiler.Semantic.ASTInterpreter.LowLevel.Optimizer.Evaluator
{
    internal sealed class TlCodeBlockEvaluation
    {
        public string EvaluationTitle { get; private set; }

        public GMacCodeBlock CodeBlock { get; }


        private readonly Dictionary<string, Expr> _variablesValues =
            new Dictionary<string, Expr>();


        public Dictionary<string, Expr> OutputVariablesValues
        {
            get
            {
                return
                    CodeBlock
                        .OutputVariables
                        .Select(item => item.LowLevelName)
                        .ToDictionary(outputVarName => outputVarName, outputVarName => this[outputVarName]);
            }
        }

        public Expr this[string varName]
        {
            get
            {
                Expr value;

                if (_variablesValues.TryGetValue(varName, out value))
                    return value;

                return SymbolicUtils.Constants.ExprZero;
            }
            set
            {
                if (_variablesValues.ContainsKey(varName))
                    _variablesValues[varName] = value;
                else
                    _variablesValues.Add(varName, value);
            }
        }


        public TlCodeBlockEvaluation(GMacCodeBlock codeBlock, string evalTitle)
        {
            EvaluationTitle = evalTitle;
            CodeBlock = codeBlock;
        }
    }
}
