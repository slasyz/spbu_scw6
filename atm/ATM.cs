using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

namespace atm
{
	class InputException : Exception {
		public InputException(string msg) : base(msg) {
		}
	}

	class ATM
	{
		public static void ReadFromConsole(out int sum, out ISet<int> coinSet) {
			string line;
			string[] lineSplit;
			coinSet = new HashSet<int> ();

			Console.Write ("Введите сумму: ");
			line = Console.ReadLine ();
			if (!int.TryParse (line, out sum) || (sum <= 0))
				throw new InputException ("сумма должна быть целым числом больше нуля");

			Console.Write ("Введите номиналы монет через пробел: ");
			line = Console.ReadLine ().Trim();
			//lineSplit = line.Split (null);
			lineSplit = Regex.Split (line, @"\s+");
			foreach (string el in lineSplit) {
				int input;

				if (!int.TryParse (el, out input) || (input <= 0))
					throw new InputException ("номиналы монет должны быть целыми числами больше нуля");

				coinSet.Add (input);
			}
		}

		private static List<int[]> Step(int sum, List<int> coinList, int start, int[] countToStart) {
			List<int[]> result = new List<int[]>();
			int[] newCountToStart = (int[]) countToStart.Clone ();

			if (start < coinList.Count - 1) {
				// Not the last step:
				// iterate by count of "start"-th coin.
				for (int cnt = 0; cnt <= sum / coinList [start]; cnt++) {
					newCountToStart [start] = cnt;

					List<int[]> stepResult = Step (sum - cnt * coinList [start], coinList, start + 1, newCountToStart);
					result.AddRange (stepResult);
				}

			} else {
				// The last step:
				// check if we can make correct exchange, then append it to result.
				if (sum % coinList [start] == 0) {
					newCountToStart [start] = sum / coinList [start];
					result.Add (newCountToStart);
				}
			}

			return result;
		}

		public static List<int[]> GetExchangesList(int sum, ISet<int> coinSet, out List<int> coinList) {
			// Convert set to list to enumerate each coin
			coinList = coinSet.ToList ();
			coinList.Sort ();
			coinList.Reverse ();

			return Step (sum, coinList, 0, new int[coinList.Count]);
		}

		public static int Main (string[] args)
		{
			int sum;
			ISet<int> coinSet;
			List<int> coinList;

			try {
				ReadFromConsole (out sum, out coinSet);
			} catch (InputException ex) {
				Console.WriteLine ("Ошибка: {0}", ex.Message);
				return 1;
			}

			List<int[]> exchanges = GetExchangesList (sum, coinSet, out coinList);

			Console.WriteLine ("Список разменов:");
			foreach (int[] exchange in exchanges)
				Console.WriteLine (string.Join (", ", exchange));
			Console.WriteLine ("(монеты: {0})", string.Join(", ", coinList));
			Console.WriteLine ("Всего {0} разменов", exchanges.Count);

			return 0;
		}
	}
}
