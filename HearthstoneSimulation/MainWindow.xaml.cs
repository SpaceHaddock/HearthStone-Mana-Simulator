using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using GLib;
using System.ComponentModel;
using Excel;
using Microsoft.Win32;

namespace HearthstoneSimulation
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		BackgroundWorker worker = new BackgroundWorker();
		ViewModel vm = new ViewModel();

		public MainWindow()
		{
			InitializeComponent();
			worker = this.FindResource("backgroundWorker") as BackgroundWorker;
			vm.worker = worker;
			DataContext = vm;
		}

		//READ EXCEL
		private void LoadDeckButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog of = new OpenFileDialog();
			of.ShowDialog();
			foreach(var worksheet in Workbook.Worksheets(of.FileName))
			{
				for (int i = 1; i < worksheet.Rows.Count(); i++)
				{
					Row row = worksheet.Rows[i];
					string text = row.Cells[0].Text;
					int create_num = int.Parse(text.Substring(0, 1));
					for (int create_count = 0; create_count < create_num; create_count++)
					{
						Card card = new Card();
						if (row.Cells[0] != null)  //name and count
							card.name = text.Substring(2, text.Length - 2);
						if (row.Cells[1] != null)  //draws cards
							card.draw_count = int.Parse(row.Cells[1].Text);
						if (row.Cells[2] != null)  //mulligan
							card.mulligan = row.Cells[2].Text == "Yes";
						if (row.Cells[3] != null)  //holds until turn x
							card.hold_until_turn = int.Parse(row.Cells[3].Text);
						if (vm.card_cost_lookup.ContainsKey(card.name))
							card.mana_cost = vm.card_cost_lookup[card.name];
						else
						{
							MessageBox.Show(String.Format("Card name \"{0}\" is not recognized", card.name));
							break;
						}
						vm.deck.Add(card);
					}
				}
			}
		}

		//SIMULATION and BACKGROUND worker
		private void SimulateButton_Click(object sender, RoutedEventArgs e)
		{
			if (!worker.IsBusy)
			{
				GList.Shuffle<Card>(vm.deck);
				worker.RunWorkerAsync();
			}
		}

		private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
		{
			vm.Simulate();
		}

		private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
		{
			Progress_TextBox.Text = String.Format("Simmed: {0}", e.ProgressPercentage);
			Progress_Bar.Value = (e.ProgressPercentage * 100) / vm.sim_count;
			if (e.ProgressPercentage % vm.sim_update_tick == 0)
				vm.UpdateSimulationTick();
		}

		private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			Progress_TextBox.Text = "Done";
			vm.SimulationEnded();
		}
	}
}