using System;
using System.Collections.Generic;
using System.Linq;

public static class Extensions
{
	public static IList<T> Clone<T>(this IList<T> listToClone)
	{
		return listToClone.Select(item => item).ToList();
	}
}
