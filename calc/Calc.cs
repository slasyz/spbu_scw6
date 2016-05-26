using System;
using System.Text.RegularExpressions;

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
				mod = Math.Abs (mod);
			}

			do {
				mod = Math.DivRem(mod, Base, out div);
				result = DigitsSet[div] + result;
			} while (mod != 0);
				
			if (num < 0) {
				result = "-" + result;
			}
			return result;
		}

		public static string GetResult(string line) {
			string lineOld = line;
			Regex expr = new Regex(@"(\-?\d+)\s*([+*/-])\s*(\-?\d+)");
			Regex brackets = new Regex(@"\(\s*(-?\d+)\d*\s*\)");
			string numStr = String.Format (@"^-?[{0}]+$", DigitsSet);
			Regex num = new Regex (numStr);

			do {
				lineOld = line;

				// Replace "123 + 321" occurence to evaluated result
				line = expr.Replace(line, delegate(Match match) {
					int n1, n2;
					char op;

					n1 = ToDec(match.Groups[1].ToString());
					op = match.Groups[2].ToString()[0];
					n2 = ToDec(match.Groups[3].ToString());

					Console.WriteLine("[debug] Calculate '{0}' '{1}' '{2}'", n1, op, n2);

					switch (op) {
					case '+':
						return FromDec(n1 + n2);
					case '-':
						return FromDec(n1 - n2);
					case '*':
						return FromDec(n1 * n2);
					case '/':
						return FromDec(n1 / n2);
					default:
						throw new Exception();
					}
				});

				Console.WriteLine("[debug] Result is '{0}'", line);

				// Replace "(123)" to "123"
				line = brackets.Replace(line, "$1");

				// Result
				if (num.Match(line).Success)
					return line;
				else {
					Console.WriteLine("[debug] '{0} is not a number, so continue", line);
				}
			} while (lineOld != line);

			throw new InvalidInputException ();
		}

		public static int Main (string[] args)
		{
			string line;
			string result;

			while (true) {
				Console.Write("> ", Base);
				line = Console.ReadLine ();

				try {
					result = GetResult (line);
					Console.WriteLine ("Ответ: {0}\n", result);
				} catch (InvalidInputException ex) {
					Console.WriteLine ("Invalid input");
				} catch (DivideByZeroException ex) {
					Console.WriteLine ("Division by zero");
				}
			}
		}
	}
}
