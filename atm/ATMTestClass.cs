using NUnit.Framework;
using System;
using System.Collections.Generic;
using atm;

namespace atm
{
	[TestFixture ()]
	public class ATMTestClass
	{
		[Test ()]
		public void TestSmallExchange ()
		{
			List<int> coinList;
			HashSet<int> coinSet = new HashSet<int> (){ 1, 2, 3, 4, 5 };

			Assert.AreEqual(7, atm.ATM.GetExchangesList(5, coinSet, out coinList).Count);
		}

		[Test ()]
		public void TestBigExchange ()
		{
			List<int> coinList;
			HashSet<int> coinSet = new HashSet<int> (){ 1, 2, 3, 4, 5 };

			Assert.AreEqual(643287, atm.ATM.GetExchangesList(200, coinSet, out coinList).Count);
		}
	}
}

