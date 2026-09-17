using System.Collections.Generic;

namespace NYCasting.Core.Models.DataAccess;

public class QueryBuilder
{
	public static string BuildWhereExpression(SearchFilter searchFilter)
	{
		List<string> conditions = new List<string>();
		foreach (SearchFilter.Filter filter in searchFilter.Filters)
		{
			string searchStringValue = filter.Value.ToString().Replace("%", "\\%").Replace("_", "\\_");
			string value = GetFilterValue(filter.Value);
			switch (filter.Operator)
			{
			case SearchFilter.Operator.Equals:
				if (filter.CaseSensitive)
				{
					conditions.Add(filter.PropertyName + " (" + value + ")");
					break;
				}
				conditions.Add($"lower({filter.PropertyName}) = lower({value})");
				break;
			case SearchFilter.Operator.GreaterThan:
				conditions.Add(filter.PropertyName + " > " + value);
				break;
			case SearchFilter.Operator.LessThan:
				conditions.Add(filter.PropertyName + " < " + value);
				break;
			case SearchFilter.Operator.GreaterThanOrEqual:
				conditions.Add(filter.PropertyName + " >= " + value);
				break;
			case SearchFilter.Operator.LessThanOrEqual:
				conditions.Add(filter.PropertyName + " <= " + value);
				break;
			case SearchFilter.Operator.Contains:
				if (filter.CaseSensitive)
				{
					conditions.Add(filter.PropertyName + " LIKE '%" + searchStringValue + "%'");
					break;
				}
				conditions.Add($"lower({filter.PropertyName}) LIKE '%{searchStringValue.ToLower()}%'");
				break;
			case SearchFilter.Operator.StartsWith:
				conditions.Add($"lower({filter.PropertyName}) LIKE '{searchStringValue}'");
				break;
			case SearchFilter.Operator.EndsWith:
				conditions.Add($"lower({filter.PropertyName}) LIKE '{searchStringValue}'");
				break;
			case SearchFilter.Operator.NotEquals:
				if (filter.CaseSensitive)
				{
					conditions.Add($"lower({filter.PropertyName}) != lower({value})");
				}
				else
				{
					conditions.Add("lower(" + filter.PropertyName + ") != " + value);
				}
				break;
			}
		}
		return string.Join(" " + searchFilter.conditionOperator.ToString() + " ", conditions);
	}

	private static string GetFilterValue(object filterValue)
	{
		string value = string.Empty;
		switch (filterValue.GetType().ToString())
		{
		case "System.String":
		case "System.DateTime":
			return $"'{filterValue}'";
		case "System.Boolean":
			return filterValue.ToString().ToLower();
		default:
			return $"{filterValue}";
		}
	}
}
