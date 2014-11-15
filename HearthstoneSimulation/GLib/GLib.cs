using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GLib
{
	public abstract class Intable
	{
		abstract public int Intval();
	}

	public static class GList
	{
		//GREEDY SEARCH
		//Returns indices of items which have int values that add up to find_value
		public static List<T> GreedySearch<T>(int find_value, List<T> possible_values) where T : Intable
		{
			possible_values.Sort((a, b) => a.Intval().CompareTo(b.Intval()));
			possible_values.Reverse();
			List<int> converted_values = new List<int>();
			foreach (T v in possible_values)
			{
				int a = v.Intval();
				converted_values.Add(a);
			}
			List<int> greedy = GreedySearch(find_value, converted_values);

			if (greedy == null) return null;

			var result = new List<T>();
			foreach (int val in greedy)
			{
				int index = converted_values.IndexOf(val);
				converted_values[index] = -17;
				result.Add(possible_values[index]);
			}
			return result;
		}

		public static List<int> GreedySearch(int find_value, List<int> possible_values)
		{
			foreach (int value in possible_values)
			{
				if (value == find_value)
					return new List<int>() { value };
				else
				{
					var copy = new List<int>();
					foreach (int v in possible_values) copy.Add(v);
					copy.Remove(value);
					var result = GreedySearch(find_value - value, copy);
					if (result != null)
					{
						result.Add(value);
						return result;
					}
				}
			}
			return null;
		}

		//SHUFFLE
		//Randomly reorders the passed in list
		static Random rng = new Random();
		public static void Shuffle<T>(this IList<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}
	}
}