using System;

namespace calc
{
	public class InvalidNumberException : Exception {
	}
	public class IncompatibleBases : Exception {
	}

	public class Number 
	{
		const string DigitsSet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

		private string Digits { get; set; }
		private int Base { get; set; }
		private short Sign { get; set; }

		public Number(string digits, short sign, int b) {
			CheckDigits (digits, b);

			Digits = digits;
			Sign = sign;
			Base = b;
		}

		public Number(string digits, int b) {
			string digitsPure = digits;
			Sign = +1;

			if (digits [0] == '-')
				Sign = -1;
			if (digits [0] == '-' || digits [0] == '+')
				digitsPure = digits.Substring (1);

			CheckDigits(digits, b);
			Digits = digitsPure;
			Base = b;
		}

		public static void CheckDigits(string digits, int b) {
			foreach (char digit in digits) {
				if (ToDec (digit) >= b)
					throw new InvalidNumberException ();
			}
		}

		public override string ToString() {
			return Digits;
		}

		public static Number operator +(Number a, Number b) {
			ushort tr = 0; // add to next digit

			if (a.Sign == 1 && b.Sign == 1) {
				// модуль первого плюс модуль второго
				// знак плюс
			} else if (a.Sign == 1 && b.Sign == -1) {
				// модуль первого минус модуль второго
			} else if (a.Sign == -1 && b.Sign == 1) {
				// 
			}

			string d1 = a.Digits;
			string d2 = b.Digits;

			if (d1.Length > d2.Length)
		}

		public static Number operator -(Number a, Number b) {
			
		}

		public static Number operator *(Number a, Number b) {
			
		}

		public static Number operator /(Number a, Number b) {
			short newSign = a.Sign * b.Sign;
		}

		public static Number operator %(Number a, Number b) {
			return a - a / b * b;
		}

		private static int ToDec(char digit) {
			return DigitsSet.IndexOf(digit);
		}

		private static char FromDec(int value) {
			return DigitsSet[value];
		}
	}
}

