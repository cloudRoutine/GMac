﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using GMac.GMacAST.Expressions;
using IronyGrammars.Semantic.Expression.ValueAccess;
using Wolfram.NETLink;

namespace GMac.GMacAPI.CodeBlock
{
    /// <summary>
    /// This class represents a low-level input variable in the Code Block
    /// </summary>
    public sealed class GMacCbInputVariable : GMacCbVariable, IGMacCbParameterVariable, IGMacCbRhsVariable
    {
        private readonly List<GMacCbComputedVariable> _userVariables = new List<GMacCbComputedVariable>();


        /// <summary>
        /// The primitive macro parameter component associated with this variable
        /// </summary>
        public AstDatastoreValueAccess ValueAccess { get; }

        /// <summary>
        /// The name of the primitive macro parameter component associated with this variable
        /// </summary>
        public string ValueAccessName => ValueAccess.ValueAccessName;

        /// <summary>
        /// A Test Value associated with this input variable for debugging purposes
        /// </summary>
        public Expr TestValueExpr { get; internal set; }

        /// <summary>
        /// The computed varibales depending on this variable
        /// </summary>
        public IEnumerable<GMacCbComputedVariable> UserVariables => _userVariables;

        /// <summary>
        /// True if there is a computed variable depending on this variable
        /// </summary>
        public bool IsUsed => _userVariables.Count > 0;

        /// <summary>
        /// The last computed variable using this input variable in its computation
        /// </summary>
        public GMacCbComputedVariable LastUsingVariable =>
            _userVariables?
            .OrderByDescending(v => v.ComputationOrder)
            .FirstOrDefault();

        /// <summary>
        /// The computation order of the last computed variable using this input variable in its computation
        /// </summary>
        public int LastUsingComputationOrder
        {
            get
            {
                var computedVar = LastUsingVariable;

                if (computedVar == null) return -1;

                return computedVar.ComputationOrder;
            }
        }


        internal GMacCbInputVariable(string lowLevelName, int lowLevelId, LanguageValueAccess valueAccess)
            : base(lowLevelName)
        {
            LowLevelId = lowLevelId;
            ValueAccess = valueAccess.ToAstDatastoreValueAccess();
            MaxComputationLevel = 0;
        }


        /// <summary>
        /// Add a user variable of this input variable
        /// </summary>
        /// <param name="computedVar"></param>
        internal void AddUserVariable(GMacCbComputedVariable computedVar)
        {
            _userVariables.Add(computedVar);
        }

        internal override void ClearDependencyData()
        {
            _userVariables.Clear();
        }

        public override string ToString()
        {
            var s = new StringBuilder();

            s.Append("Input: ").AppendLine(LowLevelName);

            return s.ToString();
        }


        
    }
}
