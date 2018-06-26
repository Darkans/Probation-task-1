using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace MyTask.Unit_Tests
{
	[TestFixture]
	public class HandlerTest
	{
		[Test]
		public void SourceDataIsNullTest() {
			string[] sourceData = null;
			
			var handler = new Handler(sourceData);
			var result = handler.GetResult();
			var emptyResult = new Dictionary<string, int>() {};
			
			Assert.AreEqual(result, emptyResult);
		}
		
		[Test]
		public void WrongStringsFormatTest() {
			string[] sourceData = {
				"    ,123",
				" City1 , ,",
				"   City_1,    900   ",
				"CorrectCityName, 100",
				",",
				"",
				"     ,     "
			};
			
			var handler = new Handler(sourceData);
			var result = handler.GetResult();
			var expectedResult = new Dictionary<string, int>() {};
			
			expectedResult.Add("CITY_1", 900);
			expectedResult.Add("CORRECTCITYNAME", 100);
			
			Assert.AreEqual(result, expectedResult);
		}
		
		[Test]
		public void WrongValuesRangeTest() {
			string[] sourceData = {
				"City_100, 100",
				"City_0, 0",
				"City_Negative, -100",
				"City_Billion, 1000000000",
				"City_Million, 1000000"
			};
			
			var handler = new Handler(sourceData);
			var result = handler.GetResult();
			var expectedResult = new Dictionary<string, int>() {};
			
			expectedResult.Add("CITY_100", 100);
			expectedResult.Add("CITY_0", 0);
			expectedResult.Add("CITY_MILLION", 1000000);
			
			Assert.AreEqual(result, expectedResult);
		}
		
		[Test]
		public void CalculationTest() {
			string[] sourceData = {
				"City_1, 100",
				"City_2, 0",
				"City_1, 12345",
				"City_3, 20000000",
				"City_1, 1000000"
			};
			
			var handler = new Handler(sourceData);
			var result = handler.GetResult();
			var expectedResult = new Dictionary<string, int>() {};
			
			expectedResult.Add("CITY_1", 1012445);
			expectedResult.Add("CITY_2", 0);
			expectedResult.Add("CITY_3", 20000000);
			
			Assert.AreEqual(result, expectedResult);
		}
	}
}
