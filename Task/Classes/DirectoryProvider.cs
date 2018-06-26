using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace MyTask
{
	/// <summary>
	/// Данный класс является "поставщиком" исходных данных из файлов в определенной папке, согласно условиям задания.
	/// 
	/// Задачи класса:
	/// 1. Построение списка файлов в указанной папке для дальнейшей обработки.
	/// 2. Проверка доступности папки и каждого файла.
	/// 3. Передача данных из очередного файла за пределы класса.
	/// </summary>
	public class DirectoryProvider: ISourceProvider
	{
		private DirectoryInfo _currentDirectory;
		private Queue<FileInfo> _fileQueue;
		private string _fileExtension;
		private int _processedFilesCount;
		
		/// <summary>
		/// Инициализирует экземпляр класса.
		/// </summary>
		/// <param name="directoryPath">Путь к папке, файлы в которой нужно обработать</param>
		/// <param name="ignoredFiles">Список игнорируемых файлов в папке</param>
		public DirectoryProvider(string directoryPath, params string[] ignoredFiles) {
			try {
				_currentDirectory = new DirectoryInfo(directoryPath);
				
				// Создается очередь файлов на обработку, исключая файлы из IgnoredFiles
				_fileQueue = new Queue<FileInfo>(_currentDirectory.EnumerateFiles().Where(f => !ignoredFiles.Contains(Path.GetFileNameWithoutExtension(f.Name))));
				
				// Определение расширения файлов в папке. Используется при создании файла с результатом обработки.
				if (_fileQueue.Count > 0) _fileExtension = _fileQueue.Peek().Extension;
			} catch (Exception e) {
				throw e;
			}
			_processedFilesCount = 0;
		}
		
		public string FileExtension {
			get { return _fileExtension; }
		}
		
		/// <summary>
		/// Количество обработанных файлов. Для статистики
		/// </summary>
		public int ProcessedFilesCount {
			get { return _processedFilesCount; }
		}
		
		public string[] GetNextSourceData() {
			if (_fileQueue.Count == 0) return new string[] {};
			try {
				// Берется очередной файл из очереди, открывается и читается до конца. Полученный текст разбивается 
				// на строки и возвращается в виде массива
				var data = _fileQueue.Dequeue().OpenText().ReadToEnd().Split(Environment.NewLine.ToCharArray());
				_processedFilesCount++;
				return data;
			} catch (UnauthorizedAccessException e) {
				// Если нет прав для открытия файла, то он пропускается
				Console.WriteLine(e.Message);
			} catch (Exception e) {
				Console.WriteLine("Необработанная ошибка: " + e.Message);
			}
			
			// При возникновении ошибки возвращается пустой массив
			return new string[] {};
		}
		
		public bool DataIsOver()
		{
			return _fileQueue.Count == 0;
		}
	}
}

/* Вариант оптимизации на случай, когда файл содержит большое количество информации:
 * Для начала нужно определить признаки "большого" файла. За эти признаки можно взять общее количество строк
 * в файле (например, 200к+), или размер файла (> 2Мб).
 * Далее добавить в класс DirectoryProvider что-то наподобие пула данных. Пусть это будет список строк.
 * Теперь содержимое файлов не будет сразу передаваться через GetNextSourceData(), а будет добавляться в пул,
 * и уже потом из пула можно извлекать определенное количество записей и передавать в GetNextSourceData().
 * Таким образом, класс DirectoryProvider может контролировать количество информации, передаваемой обработчику.
 * Т.к. в классе HandleManager есть возможность ограничения максимального количества потоков, можно выбирать между
 * большим количеством потоков, обрабатывающих малое количество информации, и наоборот, меньше потоков - больше информации на поток.
 */
