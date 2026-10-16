using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
using NYCasting.Infrastructure.DataAccess;

namespace NYCastings.API.Infrastructure.Helpers;

// Turns any location text used in emails into state names only:
//   "Atlanta (Georgia)" / "Georgia (Atlanta)" / "Atlanta" / "ATL" => "Georgia"
public static class LocationDisplayHelper
{
	private sealed class LocationLookup
	{
		public HashSet<string> States { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

		public Dictionary<string, string> StateByCity { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		public Dictionary<string, string> StateByCode { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		// "city|state" pairs, so "D.C. (Washington)" resolves to D.C. even though Washington is also a state
		public HashSet<string> CityStatePairs { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
	}

	private static readonly Regex BracketPattern = new Regex("^(?<outside>[^()]*?)\\s*\\(\\s*(?<inside>[^()]+?)\\s*\\)$", RegexOptions.Compiled);

	private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1.0);

	private static readonly object CacheLock = new object();

	private static LocationLookup _cachedLookup;

	private static DateTime _cachedAtUtc;

	public static string ToStatesOnly(string locationDisplay, DbManager dbManager)
	{
		if (string.IsNullOrWhiteSpace(locationDisplay))
		{
			return locationDisplay ?? "";
		}
		LocationLookup lookup = GetLookup(dbManager);
		List<string> states = new List<string>();
		HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
		foreach (string item in locationDisplay.Split(','))
		{
			string location = item.Trim();
			if (location.Length == 0)
			{
				continue;
			}
			string state = ResolveState(location, lookup);
			if (seen.Add(state))
			{
				states.Add(state);
			}
		}
		return string.Join(", ", states);
	}

	private static string ResolveState(string location, LocationLookup lookup)
	{
		Match match = BracketPattern.Match(location);
		if (match.Success)
		{
			string outside = match.Groups["outside"].Value.Trim();
			string inside = match.Groups["inside"].Value.Trim();
			if (lookup.CityStatePairs.Contains(outside + "|" + inside))
			{
				return inside;
			}
			if (lookup.CityStatePairs.Contains(inside + "|" + outside))
			{
				return outside;
			}
			if (lookup.States.Contains(inside))
			{
				return inside;
			}
			if (lookup.States.Contains(outside))
			{
				return outside;
			}
			return LookupPart(inside, lookup) ?? LookupPart(outside, lookup) ?? ((inside.Length > 0) ? inside : outside);
		}
		if (lookup.States.Contains(location))
		{
			return location;
		}
		return LookupPart(location, lookup) ?? location;
	}

	private static string LookupPart(string part, LocationLookup lookup)
	{
		if (part.Length == 0)
		{
			return null;
		}
		if (lookup.StateByCity.TryGetValue(part, out var stateFromCity))
		{
			return stateFromCity;
		}
		if (lookup.StateByCode.TryGetValue(part, out var stateFromCode))
		{
			return stateFromCode;
		}
		return null;
	}

	private static LocationLookup GetLookup(DbManager dbManager)
	{
		lock (CacheLock)
		{
			if (_cachedLookup != null && DateTime.UtcNow - _cachedAtUtc < CacheDuration)
			{
				return _cachedLookup;
			}
			LocationLookup lookup = new LocationLookup();
			try
			{
				DataTable dt = dbManager?.ReadData("USP_GET_LOCATIONS", CommandType.StoredProcedure);
				if (dt != null)
				{
					foreach (DataRow row in dt.Rows)
					{
						string state = row["T_LocationState"]?.ToString()?.Trim();
						if (string.IsNullOrWhiteSpace(state))
						{
							continue;
						}
						lookup.States.Add(state);
						string city = row["T_LocationCity"]?.ToString()?.Trim();
						if (!string.IsNullOrWhiteSpace(city))
						{
							lookup.CityStatePairs.Add(city + "|" + state);
							if (!lookup.StateByCity.ContainsKey(city))
							{
								lookup.StateByCity[city] = state;
							}
						}
						string code = row["T_LocationCode"]?.ToString()?.Trim();
						if (!string.IsNullOrWhiteSpace(code) && !lookup.StateByCode.ContainsKey(code))
						{
							lookup.StateByCode[code] = state;
						}
					}
				}
			}
			catch
			{
				// If locations can't be loaded, fall back to the text as-is (don't block the email)
				return lookup;
			}
			if (lookup.States.Count > 0)
			{
				_cachedLookup = lookup;
				_cachedAtUtc = DateTime.UtcNow;
			}
			return lookup;
		}
	}
}
