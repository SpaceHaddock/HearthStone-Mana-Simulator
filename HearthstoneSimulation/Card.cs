using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLib;

namespace HearthstoneSimulation
{
	public class Card : Intable
	{
		public string name { get; set; } = "no name";

		public int mana_cost { get; set; } = 1;
		public int draw_count { get; set; } = 0;
		public bool mulligan { get; set; } = true;
		public int hold_until_turn { get; set; } = 0;

		public override int Intval()
		{
			return mana_cost;
		}

		public override bool Equals(System.Object obj)
		{
			if (obj == null)
				return false;
			Card c = obj as Card;
			if ((System.Object)c == null)
				return false;
			return c.name == name;
		}

		public override int GetHashCode()
		{
			return name.GetHashCode();
		}
	}
}