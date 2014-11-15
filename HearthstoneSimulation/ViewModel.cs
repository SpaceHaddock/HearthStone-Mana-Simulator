using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GLib;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading;
using Excel;
using OxyPlot;

namespace HearthstoneSimulation
{
	public class ViewModel : INotifyPropertyChanged
	{
		public BackgroundWorker worker;

		//Public getters
		ObservableCollection<Card> _deck = new ObservableCollection<Card>();
		public ObservableCollection<Card> deck
		{
			get { return _deck; }
			set
			{
				_deck = value;
				NotifyPropertyChanged("deck");
			}
		}

		ObservableCollection<Turn> _average_game = new ObservableCollection<Turn>();
		public ObservableCollection<Turn> average_game
		{
			get { return _average_game; }
			set
			{
				_average_game = value;
				NotifyPropertyChanged("average_game");
				NotifyPropertyChanged("average_mana");
				NotifyPropertyChanged("average_hand_cards");
				NotifyPropertyChanged("average_played_cards");
				NotifyPropertyChanged("average_drawn_cards");
			}
		}

		Turn _turn_by_turn_selection;
		public Turn turn_by_turn_selection
		{
			get { return _turn_by_turn_selection; }
			set
			{
				_turn_by_turn_selection = value;
				NotifyPropertyChanged("turn_by_turn_selection");

			}
		}

		public List<DataPoint> average_mana
		{
			get
			{
				List<DataPoint> result = new List<DataPoint>();

				for (int i = 0; i < average_game.Count; i++)
					result.Add(new DataPoint(i + 1, average_game[i].mana_used));

				return result;
			}
		}

		public List<DataPoint> average_hand_cards
		{
			get
			{
				List<DataPoint> result = new List<DataPoint>();

				for (int i = 0; i < average_game.Count; i++)
					result.Add(new DataPoint(i + 1, average_game[i].hand_card_count));

				return result;
			}
		}

		public List<DataPoint> average_played_cards
		{
			get
			{
				List<DataPoint> result = new List<DataPoint>();

				for (int i = 0; i < average_game.Count; i++)
					result.Add(new DataPoint(i + 1, average_game[i].played_card_count));

				return result;
			}
		}

		public List<DataPoint> average_drawn_cards
		{
			get
			{
				List<DataPoint> result = new List<DataPoint>();

				for (int i = 0; i < average_game.Count; i++)
					result.Add(new DataPoint(i + 1, average_game[i].drawn_card_count));

				return result;
			}
		}

		int _sim_count = 1000;
		public int sim_count
		{
			get { return _sim_count; }
			set
			{
				_sim_count = value;
				NotifyPropertyChanged("sim_count");
			}
		}

		public int sim_update_tick = 1000;

		public Dictionary<string, int> card_cost_lookup = new Dictionary<string, int>();

		//Threaded variables
		List<Turn> average_game_simulation = new List<Turn>();

		//CONSTRUCTOR
		public ViewModel()
		{
			foreach (var worksheet in Workbook.Worksheets("Hearthstone - Card List.xlsx"))
			{
				bool first = true;
				foreach (Row row in worksheet.Rows)
					if (first) first = false;
					else
					{
						int mana_cost = int.Parse(row.Cells[1].Text);
                        card_cost_lookup.Add(row.Cells[0].Text, mana_cost);
					}
			}
		}

		//FUNCTIONS
		//runs the simulation setting the average_game property
		public void Simulate()
		{
			//Setup workspace
			const int turn_limit = 20;
			List<List<Turn>> all_games = new List<List<Turn>>();

			for (int simulation_num = 0; simulation_num < sim_count; simulation_num++)
			{
				//Set up the deck for turn 0
				List<Card> sim_deck = new List<Card>(deck);
				GList.Shuffle<Card>(sim_deck);
				List<Card> hand = new List<Card>();
				List<Turn> turns = new List<Turn>();

				//Draw initial hand
				for (int i = 0; i < 3; i++)
					DrawCard(sim_deck, hand);

				int returned = 0;
				for (int i = 0; i < hand.Count; i++)
				{
					if (hand[i].mulligan)
					{
						returned++;
						sim_deck.Add(hand[i]);
						hand.RemoveAt(i--);
					}
				}

				for (int i = 0; i < returned; i++)
					DrawCard(sim_deck, hand);

				//Play 20 turns
				for (int i = 1; i <= turn_limit; i++)
				{
					Turn turn = new Turn();

					//Draw card
					if (DrawCard(sim_deck, hand)) turn.drawn_card_count++;

					//Find a suitable play for the turn
					for (int j = Math.Min(i, 10); j > 0; j--)	//try all possible mana values
					{
						turn.played_cards = GList.GreedySearch<Card>(j, hand);
						if (turn.played_cards != null)
						{
							foreach (Card c in turn.played_cards)
							{
								for(int draw_amt = 0; draw_amt<c.draw_count; draw_amt++)
									if (DrawCard(sim_deck, hand)) turn.drawn_card_count++;
								hand.Remove(c);
							}
							turn.mana_used = j;
							break;
						}
						else
							turn.played_cards = new List<Card>();
					}

					turn.cards_in_hand = new List<Card>(hand);
					turns.Add(turn);
				}
				all_games.Add(turns);

				//Update at the correct times
				if (((simulation_num + 1) % sim_update_tick) == 0 || simulation_num == sim_count - 1)
				{
					average_game_simulation.Clear();
					for (int i = 0; i < turn_limit; i++)
						average_game_simulation.Add(new Turn() { turn_number = i + 1 });

					//iterate through every turn i in every game
					foreach (List<Turn> simmed_game in all_games)
						for (int i = 0; i < simmed_game.Count; i++)
						{
							var edit = average_game_simulation[i];
							edit.mana_used += simmed_game[i].mana_used;
							edit.hand_card_count += simmed_game[i].cards_in_hand.Count;
							edit.played_card_count += simmed_game[i].played_cards.Count;
							edit.drawn_card_count += simmed_game[i].drawn_card_count;

							foreach (Card card in simmed_game[i].cards_in_hand)
							{
								if (edit.average_hand_cards.ContainsKey(card))
									edit.average_hand_cards[card]++;
								else
									edit.average_hand_cards.Add(card, 1);
							}
							foreach (Card card in simmed_game[i].played_cards)
							{
								if (edit.average_played_cards.ContainsKey(card))
									edit.average_played_cards[card]++;
								else
									edit.average_played_cards.Add(card, 1);
							}
						}

					//average it all out
					foreach (Turn average_turn in average_game_simulation)
					{
						average_turn.mana_used /= simulation_num + 1;
						average_turn.hand_card_count /= simulation_num + 1;
						average_turn.played_card_count /= simulation_num + 1;
						average_turn.drawn_card_count /= simulation_num + 1;

						foreach (Card card in average_turn.average_hand_cards.Keys.ToList())
							average_turn.average_hand_cards[card] /= simulation_num + 1;
						foreach (Card card in average_turn.average_played_cards.Keys.ToList())
							average_turn.average_played_cards[card] /= simulation_num + 1;
					}
				}

				worker.ReportProgress(simulation_num);
			}
		}

		public bool DrawCard(List<Card> sim_deck, List<Card> hand)
		{
			if (sim_deck.Count > 0)
			{
				hand.Add(sim_deck[0]);
				sim_deck.RemoveAt(0);
				return true;
			}
			return false;
		}

		public void UpdateSimulationTick()
		{
			average_game = new ObservableCollection<Turn>(average_game_simulation);
		}

		public void SimulationEnded()
		{
			average_game = new ObservableCollection<Turn>(average_game_simulation);
		}

		//CHANGED EVENT
		public event PropertyChangedEventHandler PropertyChanged;
		private void NotifyPropertyChanged(String propertyName = "")
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}