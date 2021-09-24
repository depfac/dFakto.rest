using System;

namespace dFakto.Rest.AspNetCore.Mvc
{
    public enum FilterOperator
    {
        Equal,
        NotEqual,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual
    }
    
    public class Filter
    {
        public string Field { get; private set; }
        public FilterOperator Operator { get; private set; }
        public string Value { get; private set;}

        internal static Filter Parse(string filter)
        {
            var tokens = filter.Split(' ',3);
            return new Filter
            {
                Field = tokens[0],
                Operator = ParseOperator(tokens[1]),
                Value = tokens[2]
            };
        }

        private static FilterOperator ParseOperator(string op)
        {
            switch (op.ToLower())
            {
                case "eq":
                case "equal":
                case "=":
                    return FilterOperator.Equal;
                case "neq":
                case "notequal":
                case "!=":
                case "<>":
                    return FilterOperator.NotEqual;
                case "gt":
                case "greatherthan":
                case ">":
                    return FilterOperator.GreaterThan;
                case "gte":
                case "greatherthanorequal":
                case ">=":
                    return FilterOperator.GreaterThanOrEqual;
                case "lt":
                case "lessthan":
                case "<":
                    return FilterOperator.LessThan;
                case "lte":
                case "lessthanorequal":
                case "<=":
                    return FilterOperator.LessThanOrEqual;
            }

            throw new NotImplementedException($"Operator '{op}' is not supported");
        }
    }
}