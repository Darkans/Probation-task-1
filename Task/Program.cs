using System;
using System.IO;
using System.Collections.Generic;

namespace MyTask
{
	class Program
	{
		const string RESULT_FILE_NAME = "RESULT";
		
		//1-й параметр - путь к папке, 2-й параметр - количество одновременно обрабатываемых файлов
		public static void Main(string[] args)
		{
			if (args.Length == 0) {
				Console.WriteLine("Использование: MyTask DIRECTORY [MAXTHREADS]");
				Console.WriteLine("DIRECTORY - папка с исходными файлами");
				Console.WriteLine("MAXTHREADS - количество одновременно обрабатываемых файлов. По умолчанию: 1");
				return;
			}			
			
			// Извлечение и проверка пути из аргумента
			string directoryPath = String.Empty;
			try {
				directoryPath = Path.GetFullPath(args[0]);
			} catch (Exception e) {
				Console.WriteLine(e.Message);
				return;
			}
			
			int maxThreads = 0;
			bool argIsCorrect = false;
				
			// Извлечение значения максимального количества потока из аргумента, если он был передан
			if (args.Length > 1) {
				argIsCorrect = int.TryParse(args[1], out maxThreads);
			}
				
			if (!argIsCorrect) maxThreads = 1;
			
			try {
				// В данном случае игнорируется файл, содержащий результат предыдущей работы программы
				var directoryProvider = new DirectoryProvider(directoryPath, RESULT_FILE_NAME);
				var handleManager = new HandleManager(directoryProvider.GetNextSourceData, directoryProvider.DataIsOver, maxThreads);
				
				// Отметка времени и начало обработки
				DateTime time = DateTime.Now;
				handleManager.StartProcess();
				TimeSpan res = DateTime.Now - time;
				
				WriteResultInFile(directoryPath,
				                  RESULT_FILE_NAME + directoryProvider.FileExtension,
				                  handleManager.Result);
				
				// Вывод некоторой статистики
				Console.WriteLine("Обработано файлов: " + directoryProvider.ProcessedFilesCount);
				Console.WriteLine("Времени прошло: {0} мс", res.TotalMilliseconds);
				Console.WriteLine("Количество потоков: {0}", maxThreads);
			} catch (Exception e) {
				Console.WriteLine(e.Message);
			}			
		}
		
		/// <summary>
		/// Записывает результат работы программы в файл
		/// </summary>
		/// <param name="directory">Папка, где нужно создать файл</param>
		/// <param name="fileName">Имя файла</param>
		/// <param name="result">Результат</param>
		public static void WriteResultInFile(string directory, string fileName, Dictionary<string, int> result) {
			string resultFullPath = Path.Combine(directory, fileName);
			
			using (var sw = new StreamWriter(resultFullPath, false)) {
				foreach (var item in result)
					sw.WriteLine(String.Format("{0},{1}",item.Key,item.Value));
			}
		}
	}
}