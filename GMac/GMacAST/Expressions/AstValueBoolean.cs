﻿using IronyGrammars.Semantic.Expression.Value;

namespace GMac.GMacAST.Expressions
{
    public sealed class AstValueBoolean : AstValue
    {
        #region Static members
        #endregion


        internal ValuePrimitive<bool> AssociatedBooleanValue { get; }

        internal override ILanguageValue AssociatedValue => AssociatedBooleanValue;

        public override bool IsValidBooleanValue => AssociatedBooleanValue != null;

        public override bool IsValidPrimitiveValue => AssociatedBooleanValue != null;

        public bool BooleanValue => AssociatedBooleanValue.Value;


        internal AstValueBoolean(ValuePrimitive<bool> value)
        {
            AssociatedBooleanValue = value;
        }
    }
}
