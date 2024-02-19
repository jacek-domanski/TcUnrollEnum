using System.Text.RegularExpressions;

namespace TcUnrollEnum
{
	internal class Program
	{
		private static string ENUM_EXTENSION = ".TcDUT";
		private static string[] UNWANTED_STATES = ["esInit", "esDone", "esError", "esAborted"];

		[STAThread]
		static void Main(string[] args)
		{
			string fileName;
			if (args.Length > 0) fileName = args[0];
			else fileName = Clipboard.GetText();

			if (fileName is null || fileName == "")
			{
				Console.WriteLine("No argument given and clipboard is empty");
				System.Environment.Exit(1);
			}

			string fileContents = getFileContents(fileName);
			string[] states = getStates(fileName, fileContents);
			states = removeUnwantedStates(states);
			states = addText(fileName, states);

			string output = String.Join("\n", states);

			Console.WriteLine();
			Console.WriteLine(output);
			Clipboard.SetText(output);
			Console.WriteLine();
			Console.WriteLine("Done! States copied to clipboard");
		}

		static string getFileContents(string enumName)
		{
			string fileName = enumName.Trim();
			if (fileName.Contains('.'))
			{
				fileName = fileName.Split('.')[0];
			}
			fileName += ENUM_EXTENSION;

			string[] filesPaths = Directory.GetFiles(Environment.CurrentDirectory, fileName, SearchOption.AllDirectories);
			if (filesPaths.Length == 0)
			{
				Console.WriteLine("File \"" + enumName + "\" not found");
				System.Environment.Exit(1);
			}

			foreach (string filePath in filesPaths)
			{
				try
				{
					return File.ReadAllText(filePath);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error reading file: {ex.Message}");
				}
			}
			throw new FileNotFoundException();
		}

		static string[] getStates(string fileName, string fileContents)
		{
			string notStatesPattern = @".*TYPE\s+S_EP_\s*:\s*\(|\)\s*;.*?END_TYPE.*|\s+";
			fileContents = Regex.Replace(fileContents, notStatesPattern, "", RegexOptions.Singleline);

			string assignmentPattern = @":=.*?,";
			fileContents = Regex.Replace(fileContents, assignmentPattern, ",", RegexOptions.Singleline);

			return fileContents.Split(',');
		}

		static string[] removeUnwantedStates(string[] states)
		{
			List<string> newStates = new List<string>();
			foreach (string state in states)
			{
				if (!UNWANTED_STATES.Contains(state)) newStates.Add(state);
			}

			return newStates.ToArray();
		}

		static string[] addText(string fileName, string[] states)
		{
			for (int i = 0; i < states.Length; i++)
			{
				states[i] = fileName + '.' + states[i] + ':';
			}
			return states;
		}
	}
}