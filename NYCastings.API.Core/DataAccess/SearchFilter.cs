using System;
using System.Collections.Generic;

namespace NYCasting.Core.Models.DataAccess;

[Serializable]
public class SearchFilter
{
	[Serializable]
	public class Filter
	{
		public string PropertyName { get; set; }

		public Operator Operator { get; set; }

		public object Value { get; set; }

		public bool CaseSensitive { get; set; }
	}

	[Serializable]
	public enum Operator
	{
		Equals,
		GreaterThan,
		LessThan,
		GreaterThanOrEqual,
		LessThanOrEqual,
		Contains,
		StartsWith,
		EndsWith,
		NotEquals,
		IsNull
	}

	[Serializable]
	public enum ConditionOperator
	{
		AND,
		OR
	}

	public enum SortOrder
	{
		NoSort = 0,
		Ascending = 1,
		Descending = -1
	}

	public ConditionOperator conditionOperator { get; set; }

	public IList<Filter> Filters { get; set; }
}
