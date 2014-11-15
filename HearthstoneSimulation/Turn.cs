using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace HearthstoneSimulation
{
	public class Turn : INotifyPropertyChanged
	{
		public List<Card> cards_in_hand = new List<Card>();
		public List<Card> played_cards = new List<Card>();

		public Dictionary<Card, double> average_hand_cards = new Dictionary<Card, double>();
		public Dictionary<Card, double> average_played_cards = new Dictionary<Card, double>();

		double _mana_used = 0;
		public double mana_used
		{
			get { return _mana_used; }
			set
			{
				_mana_used = value;
				NotifyPropertyChanged("mana_used");
				NotifyPropertyChanged("rect_height");
				NotifyPropertyChanged("most_common_hand_cards");
				NotifyPropertyChanged("most_common_played_cards");
				NotifyPropertyChanged("turn_by_turn_string");
			}
		}

		public string turn_by_turn_string
		{
			get { return String.Format("Turn {0}", turn_number); }
		}

		double _hand_card_count = 0;
		public double hand_card_count
		{
			get { return _hand_card_count; }
			set
			{
				_hand_card_count = value;
				NotifyPropertyChanged("hand_card_count");
			}
		}

		double _played_card_count = 0;
		public double played_card_count
		{
			get { return _played_card_count; }
			set
			{
				_played_card_count = value;
				NotifyPropertyChanged("played_card_count");
			}
		}

		double _drawn_card_count = 0;
		public double drawn_card_count
		{
			get { return _drawn_card_count; }
			set
			{
				_drawn_card_count = value;
				NotifyPropertyChanged("drawn_cards");
			}
		}

		//Getters
		public List<string> most_common_hand_cards
		{ get { return DictToString(average_hand_cards); } }

		public List<string> most_common_played_cards
		{ get { return DictToString(average_played_cards); } }

		public int rect_height
		{ get { return (int)(mana_used * 10); } }

		int _turn_number = 0;
		public int turn_number
		{
			get { return _turn_number; }
			set
			{
				_turn_number = value;
				NotifyPropertyChanged("turn_number");
			}
		}

		//CHANGED EVENT
		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName = "")
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		//FUNCTIONS
		List<string> DictToString(Dictionary<Card, double> input_dict)
		{
			List<string> result = new List<string>();
			List<KeyValuePair<Card, double>> use_list = input_dict.ToList();
			use_list.Sort((b, a) => a.Value.CompareTo(b.Value));
			for (int i = 0; i < use_list.Count && i < 5; i++)
				result.Add(String.Format("{0}: {1:0.00}", use_list[i].Key.name, use_list[i].Value));
			return result;
		}
	}
}
