using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace MyTask
{
	/// <summary>
	/// Задачи класса:
	/// 1. Получение исходных данных извне
	/// 2. Распределение исходных данных по доступным обработчикам и запуск обработки.
	/// 3. Получение и объединение результатов работы разных обработчиков.
	/// 4. Предоставление доступа к итоговым результатам извне класса.
	/// </summary>
	public class HandleManager
	{
		// Получение данных реализованно в виде делегатов по причине того, что 
		// поставщики данных могут быть разные. При изменении поставщика
		// код данного класса никак не изменится.
		private Func<string[]> _getNextSourceDataDelegate;
		private Func<bool> _dataIsOverDelegate;
		
		private int _maxThreads;
		
		// Список заданий, выполняющихся одновременно.
		private List<Task<Dictionary<string, int>>> _tasks;
		
		private Dictionary<string, int> _result;
		
		/// <summary>
		/// Инициализирует экземпляр класса. 
		/// </summary>
		/// <param name="getNextSourceData">Делегат, возвращающий очередной набор исходных данных</param>
		/// <param name="dataIsOver">Делегат, определяющий, закончились ли исходные данные в источнике</param>
		/// <param name="maxThreads">Максимальное количество одновременно работающих обработчиков</param>
		public HandleManager(Func<string[]> getNextSourceData, Func<bool> dataIsOver, int maxThreads) {
			_getNextSourceDataDelegate = getNextSourceData;
			_dataIsOverDelegate = dataIsOver;
			
			_tasks = new List<Task<Dictionary<string, int>>>() {};
			_result = new Dictionary<string, int>() {};
			
			_maxThreads = maxThreads;
		}
		
		public Dictionary<string,int> Result {
			get { return _result; }
		}
		
		/// <summary>
		/// Запускает процесс извлечения исходных данных из источника, распределения их по заданиям (потокам)
		/// и объединения результатов их работы.
		/// </summary>
		public void StartProcess() {
			do {
				// Пока есть свободные потоки и исходные данные не закончились, 
				// будут извлекаться очередные данные и создаваться поток для их обработки 
				while (( _tasks.Count < _maxThreads ) && ( !_dataIsOverDelegate() )) {
					var data = _getNextSourceDataDelegate();
					var handler = new Handler(data);
					var newTask = new Task<Dictionary<string, int>>(handler.GetResult);
					newTask.Start();
					
					_tasks.Add(newTask);
				}
				
				// Ждём, пока не завершится хотя бы один поток
				Task.WaitAny(_tasks.ToArray());
				
				// Создается копия текущего списка потоков. Было бы неплохо убрать этот костыль
				var tempQueue = new List<Task<Dictionary<string,int>>>(_tasks);
				
				// Просматривается каждый поток в копии.Если поток завершил свою работу,
				// то из него извлекается результат и обрабатывается, а сам поток удаляется
				// из оригинального списка потоков.
				// Если вместо копии использовать оригинальный список, то после удаления 
				// потока из него возникнет исключение, т.к. просматриваемая foreach 
				// коллекция была изменена.
				foreach (var task in tempQueue) {
					if (task.IsCompleted) {
						MergeResult(task.Result);
						_tasks.Remove(task);
					}
				}
				// Выполняем данные действия, пока есть исходные данные или хотя бы один поток работает
			} while ((!_dataIsOverDelegate()) || (_tasks.Count > 0));
		}
		
		/// <summary>
		/// Объединяет отдельный результат с итоговым
		/// </summary>
		/// <param name="newResult">Добавляемый результат</param>
		private void MergeResult(Dictionary<string,int> newResult) {
			foreach (var item in newResult) {
				string newKey = item.Key;
				int value = item.Value;
				
				if (_result.ContainsKey(newKey)) {
					_result[newKey] += value;
				} else {
					_result.Add(newKey, value);
				}
			}
		}
	}
}
