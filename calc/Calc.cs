using System;

namespace calc
{
	class InvalidInputException : Exception {
	}
	class InvalidOperatorException : Exception {
	}
	class InvalidNumberException : Exception {
	}

	class Calc
	{
		const int Base = 24;
		const string DigitsSet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		public static int ToDec(string num) {
			int result = 0;
			int fact = 1;
			int sign = 1;

			if (num [0] == '-') {
				sign = -1;
				num = num.Substring (1);
			}
			if (num[0] == '+')
				num = num.Substring (1);
		
			for (int i = num.Length - 1; i >= 0; i--) {
				int pos = DigitsSet.IndexOf (num [i]);
				if (pos == -1) {
					throw new InvalidNumberException ();
				}
				
				result += fact * pos;
				fact *= Base;
			}

			return result * sign;
		}

		public static string FromDec(int num) {
			string result = "";
			int div, mod;

			mod = num;
			if (num < 0) {
				result = "-";
				mod = Math.Abs (mod);
			}

			do {
				mod = Math.DivRem(mod, Base, out div);
				result = DigitsSet[div] + result;
			} while (mod != 0);

			return result;
		}

		public static string GetResult(string num1, string num2, char op) {
			int decNum1 = ToDec(num1);
			int decNum2 = ToDec(num2);

			switch (op) {
			case '+':
				return FromDec(decNum1 + decNum2);
			case '-':
				return FromDec(decNum1 - decNum2);
			case '*':
				return FromDec(decNum1 * decNum2);
			case '/':
				return FromDec(decNum1 / decNum2);
			case '%':
				return FromDec(decNum1 % decNum2);
			default:
				throw new InvalidOperatorException ();
			}
		}

		public static void ParseInput(string line, out string num1, out string num2, out char op) {
			string[] lineSplit;
			lineSplit = line.Split (null);

			if (lineSplit.Length != 3)
				throw new InvalidInputException ();

			if (lineSplit [1].Length != 1)
				throw new InvalidInputException ();

			num1 = lineSplit [0];
			num2 = lineSplit [2];
			op = lineSplit [1] [0];
		}

		public static int Main (string[] args)
		{
			string line;
			string num1, num2;
			char op;
			string result;

			while (true) {
				Console.Write("Введите выражение вида \"A B C\", где:\n" +
					"    A и C - числа в {0}-ичной системе счисления\n" +
					"    B - оператор: +, -, *, / или %\n" +
					"> ", Base);
				line = Console.ReadLine ();

				try {
					ParseInput (line, out num1, out num2, out op);
				} catch (InvalidInputException ex) {
					Console.WriteLine ("Неверный формат ввода");
					continue;
				} catch (InvalidNumberException ex) {
					Console.WriteLine ("Неверный формат числа");
					continue;
				}

				try {
					result = GetResult (num1, num2, op);
				} catch (InvalidOperatorException ex) {
					Console.WriteLine ("Неверный оператор, должен быть: +, -, *, / или %");
					continue;
				}

				Console.WriteLine ("Ответ: {0}\n", result);
			}
		}
	}
}
