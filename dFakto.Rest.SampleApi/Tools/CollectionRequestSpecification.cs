#nullable enable
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Ardalis.Specification;
using dFakto.Rest.AspNetCore.Mvc;
using dFakto.Rest.AspNetCore.Mvc.Requests;

namespace dFakto.Rest.SampleApi.Tools
{
    public class CollectionRequestSpecification<T> : Specification<T,T>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly MemberInfo ValueFilterMemberInfo = typeof(Filter).GetMember("Value")[0];
        
        public CollectionRequestSpecification(ResourceCollectionRequest resourceCollectionRequest)
        {
            ApplyFilter(resourceCollectionRequest.Filter);
            ApplyOrderBy(resourceCollectionRequest.Sort);

            if (resourceCollectionRequest.Index.HasValue)
            {
                Query.Skip(resourceCollectionRequest.Index.Value);
            }

            if (resourceCollectionRequest.Limit.HasValue)
            {
                Query.Take(resourceCollectionRequest.Limit.Value);
            }
        }

        private void ApplyFilter(Filter? filter)
        {
            if (filter != null)
            {
                Query.Where(GetWhereExpressionFromFilter(filter));
            }
        }
        private void ApplyOrderBy(IEnumerable<string> sorts)
        {
            IOrderedSpecificationBuilder<T>? orderedSpecificationBuilder = null;

            foreach (var s in sorts)
            {
                bool desc = s.StartsWith('-');
                string field = desc ? s.Substring(1) : s;

                if (orderedSpecificationBuilder == null)
                {
                    orderedSpecificationBuilder = desc
                        ? Query.OrderByDescending(GetOrderBy(field))
                        : Query.OrderBy(GetOrderBy(field));
                }
                else
                {
                    orderedSpecificationBuilder = desc
                        ? orderedSpecificationBuilder.ThenByDescending(GetOrderBy(field))
                        : orderedSpecificationBuilder.ThenBy(GetOrderBy(field));
                }
            }
        }
        private static Expression ParseString(Expression getStringValue, Type outputType)
        {
            if (outputType == typeof(string))
            {
                return getStringValue;
            }

            if (outputType == typeof(int))
            {
                return Expression.Call(typeof(int), "Parse", null, getStringValue);
            }
            
            if (outputType == typeof(long))
            {
                return Expression.Call(typeof(long), "Parse", null, getStringValue);
            }
            
            if (outputType == typeof(double))
            {
                return Expression.Call(typeof(double), "Parse", null, getStringValue);
            }
            
            if (outputType == typeof(DateTime))
            {
                return Expression.Call(typeof(DateTime), "Parse", null, getStringValue);
            }
            
            if (outputType == typeof(DateTimeOffset))
            {
                return Expression.Call(typeof(DateTimeOffset), "Parse", null, getStringValue);
            }

            throw new NotSupportedException();
        }
        private static Expression<Func<T, object?>> GetOrderBy(string order)
        {
            var parameter = Expression.Parameter(typeof(T), "x");
            var prop = Expression.Property(parameter, order);
            var casttoObject = Expression.Convert(prop, typeof(Object));
            return Expression.Lambda<Func<T, object?>>(casttoObject, parameter);
        }
        private static Expression<Func<T,bool>> GetWhereExpressionFromFilter(Filter filter)
        {
            var val = Expression.Constant(filter);
            var getStrVal = Expression.MakeMemberAccess(val, ValueFilterMemberInfo);
            
            var parameter = Expression.Parameter(typeof(T), "x");
            var property = Expression.Property(parameter, filter.Field);
            var getvalue = ParseString(getStrVal, property.Type);
            
            Expression expr = Expression.Empty();
            switch (filter.Operator)
            {
                case FilterOperator.Equal:
                    expr = Expression.Equal(property,getvalue);
                    break;
                case FilterOperator.NotEqual:
                    expr = Expression.Not(
                        Expression.Equal(
                            property,getvalue));
                    break;
                case FilterOperator.LessThan:
                    expr = Expression.LessThan(
                        property,getvalue);
                    break;
                case FilterOperator.LessThanOrEqual:
                    expr = Expression.LessThanOrEqual(
                        property,getvalue);
                    break;
                case FilterOperator.GreaterThan:
                    expr = Expression.GreaterThan(
                        property,getvalue);
                    break;
                case FilterOperator.GreaterThanOrEqual:
                    expr = Expression.GreaterThanOrEqual(
                        property,getvalue);
                    break;
            }
            
            return Expression.Lambda<Func<T, bool>>(expr, parameter);
        }
    }
}