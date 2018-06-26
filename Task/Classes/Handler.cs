using System;
using System.Collections.Generic;

namespace MyTask
{
	/// <summary>
	/// Класс Handler выполняет обработку входящего массива строк вида {Имя города},{Число}.
	/// Подсчитывает суммарное количество жителей каждого города, учитывая повторяющиеся города.
	/// </summary>
	
	public class Handler
	{
		// Данные ограничения введены для фильтрации некорректных значений, таких, как 
		// название города, длиной в 30+ символов, или отрицательного количества жителей и т.д.
		// Ограничения по длине названия города
		const int MIN_NAME_LENGTH = 3;
		const int MAX_NAME_LENGTH = 30;
		
		// Ограничения по количеству населения в городах
		const int MIN_INHABITANTS_COUNT = 0;
		const int MAX_INHABITANTS_COUNT = 100000000;
				
		// _result содержит результат обработки текущих входных данных
		private Dictionary<string,int> _result;
		private string[] _dataSource;
		
		/// <summary>
		/// Инициализирует экземпляр класса Handler
		/// </summary>
		/// <param name="DataSource">Массив строк для обработки. Строки должны быть формата {Имя города},{Число}.</param>
		public Handler(string[] DataSource) {
			_dataSource = DataSource;
			_result = new Dictionary<string, int> {};
		}
		
		/// <summary>
		/// Запускает процесс обработки массива строк, которые были переданы в конструктор.
		/// </summary>
		/// <returns>Возвращает объект типа Dictionary&lt;string, int&gt; - результат подсчета числа жителей в городах.</returns>
		public Dictionary<string,int> GetResult() {
			if (_dataSource == null) return _result;
			
			foreach (string line in _dataSource) {
				string cityName = String.Empty;
				int inhabitantsNumber = 0;
				
				if (!TryGetNameAndNumber(line, ref cityName, ref inhabitantsNumber)) continue;
				
				if (_result.ContainsKey(cityName)) {
					_result[cityName] += inhabitantsNumber;
				} else {
					_result.Add(cityName, inhabitantsNumber);
				}
			}
			return _result;
		}
		
		/// <summary>
		/// Проверяет корректность формата переданной строки. Строка должна иметь вид {Имя города},{Число}.
		/// Если формат корректен, то значения обеих частей строки передаются в аргументы.
		/// </summary>
		/// <param name="line">Строка для проверки</param>
		/// <param name="cityName">При возвращении этим методом содержит имя города, если строка корректна, 
		/// или пустую строку, если проверка не удалась.</param>
		/// <param name="inhabitantsNumber">Содержит число жителей в городе, если строка корректна, или ноль, если проверка не удалась.</param>
		/// <returns>Значение True, если строка имеет корректный формат, в противном случае - False</returns>
		private bool TryGetNameAndNumber(string line, ref string cityName, ref int inhabitantsNumber) {
			cityName = String.Empty;
			inhabitantsNumber = 0;
			
			if (String.IsNullOrWhiteSpace(line)) return false;
			
			var lineParts = line.Split(',');
			
			// Возвратить False, если строка содержит не одну запятую, 
			// или в строке отсутствует название города или количество жителей
			if (lineParts.Length != 2) return false;
			if (String.IsNullOrWhiteSpace(lineParts[0]) || String.IsNullOrWhiteSpace(lineParts[1])) return false;
			
			if (!int.TryParse(lineParts[1].Trim(), out inhabitantsNumber)) return false;
			
			// Имя города записывается в верхнем регистре, чтобы исключить дублирование городов с одинаковым именем
			cityName = lineParts[0].Trim().ToUpper();
			
			// Проверка значений на корректность
			if ((cityName.Length < MIN_NAME_LENGTH) || (cityName.Length > MAX_NAME_LENGTH)) return false;
			if ((inhabitantsNumber < MIN_INHABITANTS_COUNT) || (inhabitantsNumber > MAX_INHABITANTS_COUNT)) return false;
			
			return true;
		}
	}
}
