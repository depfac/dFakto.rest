using System.Diagnostics.CodeAnalysis;

namespace dFakto.Rest.AspNetCore.Mvc.Requests
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

        internal static bool TryParse(string filterString, out Filter filter)
        {
            filter = null;
            
            if (string.IsNullOrEmpty(filterString))
            {
                return false;
            }
            
            var tokens = filterString.Split(' ',3);
            if (tokens.Length == 3 && TryParseOperator(tokens[1], out var filterOperator))
            {
                filter = new Filter
                {
                    Field = tokens[0],
                    Operator = filterOperator.Value,
                    Value = tokens[2]
                };
            }

            return filter != null;
        }

        private static bool TryParseOperator(string op,[NotNullWhen(true)] out FilterOperator? filterOperator)
        {
            filterOperator = null;
            
            switch (op.ToLower())
            {
                case "eq":
                case "equal":
                case "=":
                    filterOperator = FilterOperator.Equal;
                    break;
                case "neq":
                case "notequal":
                case "!=":
                case "<>":
                    filterOperator = FilterOperator.NotEqual;
                    break;
                case "gt":
                case "greatherthan":
                case ">":                    
                    filterOperator = FilterOperator.GreaterThan;
                    break;
                case "gte":
                case "greatherthanorequal":
                case ">=":
                    filterOperator = FilterOperator.GreaterThanOrEqual;
                    break;
                case "lt":
                case "lessthan":
                case "<":
                    filterOperator = FilterOperator.LessThan;
                    break;
                case "lte":
                case "lessthanorequal":
                case "<=":
                    filterOperator = FilterOperator.LessThanOrEqual;
                    break;
            }
            return filterOperator != null;
        }
    }
}