using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Task3
{
    public class WordCounter
    {
        protected const int DEFAULT_MEMORY_LIMIT = 1_048_576; // 1 Mb

        /// <summary>
        /// Memory limit for reading data in bytes, set to 1_048_576 bytes
        /// </summary>
        public int MemoryLimit { get; set; }

        /// <summary>
        /// StreamReader for line by line data input, used by the TextProcessor to read data
        /// </summary>
        public StreamReader? Input { get; set; }

        /// <summary>
        /// StreamWriter for line by line data output, used by the TextProcessor to write data
        /// </summary>
        public StreamWriter Output { get; set; }

        /// <summary>
        /// Encoding for reading and writing data
        /// </summary>
        public Encoding TextEncoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Creates an instance of WordCounter with the specified memory limit for reading data, input and output streams
        /// </summary>
        /// <param name="memoryLimit">Memory limit for reading data in bytes, set to 1_048_576 by default</param>
        public WordCounter(StreamReader input, StreamWriter output, int memoryLimit = DEFAULT_MEMORY_LIMIT)
        {
            Input = input;
            Output = output;
            MemoryLimit = memoryLimit;
        }

        /// <summary>
        /// Creates an instance of WordCounter with the specified memory limit for reading data and an input stream. Sets console as an output
        /// </summary>
        /// <param name="input">StreamWriter supplying data to the WordCounter</param>
        /// <param name="memoryLimit">Memory limit for reading data in bytes, set to 1_048_576 by default</param>
        public WordCounter(StreamReader input, int memoryLimit = DEFAULT_MEMORY_LIMIT)
        {
            Input = input;
            Output = new StreamWriter(Console.OpenStandardOutput());
            MemoryLimit = memoryLimit;
        }

        /// <summary>
        /// Creates an instance of WordCounter with the specified memory limit for reading data, an input and an output from specified files
        /// </summary>
        /// <param name="inputFile">Name of the file used for input</param>
        /// <param name="outputFile">Name of the file used for output</param>
        /// <param name="memoryLimit">Memory limit for reading data in bytes, set to 1_048_576 by default</param>
        public WordCounter(string inputFile, string outputFile, int memoryLimit = DEFAULT_MEMORY_LIMIT)
        {
            SetFileInput(inputFile);
            SetFileOutput(outputFile);
            MemoryLimit = memoryLimit;
        }

        /// <summary>
        /// Creates an instance of WordCounter with the specified memory limit for reading data and an input from specified file. Sets console as an output
        /// </summary>
        /// <param name="inputFile">Name of the file used for input</param>
        /// <param name="memoryLimit">Memory limit for reading data in bytes, set to 1_048_576 by default</param>
        public WordCounter(string inputFile, int memoryLimit = DEFAULT_MEMORY_LIMIT)
        {
            SetFileInput(inputFile);
            Output = new StreamWriter(Console.OpenStandardOutput());
            MemoryLimit = memoryLimit;
        }

        /// <summary>
        /// Creates an instance of WordCounter with the specified memory limit for reading data.
        /// Input is set to null, should be specified somewhere later in code before calling the processing methods.
        /// Sets console as an output
        /// </summary>
        /// <param name="memoryLimit">Memory limit for reading data in bytes, set to 1_048_576 by default</param>
        public WordCounter(int memoryLimit = DEFAULT_MEMORY_LIMIT)
        {
            Output = new StreamWriter(Console.OpenStandardOutput());
            MemoryLimit = memoryLimit;
        }

        // simple write to output line by line
        protected void WriteData(string result)
        {
            string[] lines = result.Split(' ');
            foreach (string line in lines) Output.WriteLine(line);
        }

        // helper method for parallel counting
        protected long CountWordsParallel(char[] block) => CountWords(block, block[0], block.Length, 1);

        // main counting logic, gets a char from previous block and a current block of chars
        // can be overriden to implement own logic of counting
        protected virtual long CountWords(char[] currentBlock, char prevChar, int blockLength, int startingIndex = 0)
        {
            long amount = 0;
            char currentChar;
            // look for the occurences of "SX", where X is any non whitespace charachter and S is a whitespace
            for (int i = startingIndex; i < blockLength; i++)
            {
                currentChar = currentBlock[i];
                if (char.IsWhiteSpace(prevChar) && !char.IsWhiteSpace(currentChar)) amount++;
                prevChar = currentChar;
            }
            return amount;
        }

        // helper method for non parallel counting
        protected virtual long CountWordsNonParallel(char[] currentBlock, char prevChar, int blockLength) => CountWords(currentBlock, prevChar, blockLength);

        // main parallel counting logic, gets a char from previous block and a current block of chars
        // can be overriden to implement own logic of counting
        protected virtual long CountWordsParallel(char[] currentBlock, char prevChar, int blockLength)
        {
            int parts = Environment.ProcessorCount;
            if (parts > blockLength / 4) parts = blockLength / 4; // get the amount of parts to partition the array into
            // if amount of parts is greater than initial block length, reduce it to 1/4 of block length
            var partitionedBlock = PartitionBlock(currentBlock, prevChar, blockLength, parts); // partition the block
            long result = partitionedBlock.AsParallel().Sum(x => CountWordsParallel(x)); // count words in parallel
            return result;
        }

        // supplier of block parts, used in a parallel counting methods
        protected static IEnumerable<char[]> PartitionBlock(char[] currentBlock, char prevChar, int blockLength, int parts)
        {
            int partSize = blockLength / parts; // calculate size of a part
            for (int i = 0; i < parts; i++)
            {
                char[] part = new char[partSize + 1]; // create a part with 1 extra space for a previous char from other block
                part[0] = prevChar; // set first char to that previous char
                Array.Copy(currentBlock, partSize * i, part, 1, partSize); // copy the needed elements of initial block into the part
                prevChar = part[^1]; // set the previous char to the last char of the current part
                yield return part;
            }
        }

        /// <summary>
        /// Starts the WordCounter. Reads and processes data, then writes (if doWrite is true) and returns it
        /// </summary>
        /// <param name="doWrite">If true, writes the result to specified Output, true by default</param>
        /// <param name="doParallel">If true, processes data in parallel by partitioning the blocks of data. Recommended to use with a memory limit of at least 100 Kb, true by default</param>
        /// <returns>The result of data processing</returns>
        public long ProcessText(bool doWrite = true, bool doParallel = true)
        {
            if (Input == null) throw new ArgumentNullException("No input was found");
            Func<char[], char, int, long> countingFunc = doParallel ? CountWordsParallel : CountWordsNonParallel; // determine the processing function to use (no if statememnt on every read)
            char prevChar = ' ';
            char[] currentBlock = new char[MemoryLimit]; // init buffer for the data
            int charsRead = Input.ReadBlock(currentBlock, 0, MemoryLimit); // first read
            long wordsAmount = 0;
            // while there are chars being read, continue reading
            while (charsRead > 0)
            {
                wordsAmount += countingFunc(currentBlock, prevChar, charsRead); // process data
                prevChar = currentBlock[^1]; // set the prev char to the last char of the current block
                charsRead = Input.ReadBlock(currentBlock, 0, MemoryLimit); // read data
            }
            string result = wordsAmount.ToString();
            if (doWrite) WriteData(result); // write data to output if needed
            // release resources
            Input.Dispose();
            Output.Dispose();
            return wordsAmount;
        }

        private static FileInfo FindFile(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists) throw new FileNotFoundException($"File {filePath} was not found");
            if (fileInfo.Attributes.HasFlag(FileAttributes.Directory)) throw new ArgumentException($"Expected a file, got a directory from path {filePath}");
            return fileInfo;
        }

        /// <summary>
        /// Sets a file as a WordCounter input
        /// </summary>
        /// <param name="filePath">Name of the file used for input</param>
        public void SetFileInput(string filePath)
        {
            FileInfo fileInfo = FindFile(filePath);
            FileStream fileStream = fileInfo.OpenRead();
            StreamReader reader = new StreamReader(fileStream, TextEncoding);
            Input = reader;
        }

        /// <summary>
        /// Sets a file as a WordCounter output
        /// </summary>
        /// <param name="filePath">Name of the file used for output</param>
        public void SetFileOutput(string filePath)
        {
            FileInfo fileInfo = FindFile(filePath);
            FileStream fileStream = fileInfo.OpenWrite();
            StreamWriter writer = new StreamWriter(fileStream, TextEncoding);
            Output = writer;
        }

        /// <summary>
        /// Starts the WordCounter using console as input. Reads and processes data, then writes (if doWrite is true) and returns it
        /// </summary>
        /// <param name="doWrite">If true, writes the result to specified Output, true by default</param>
        /// <param name="doParallel">If true, processes data in parallel by partitioning the blocks of data. Recommended to use with a memory limit of at least 100 Kb, false by default</param>
        /// <returns>The result of data processing</returns>
        public long ProcessTextConsole(bool doWrite = true, bool doParallel = false)
        {
            Console.WriteLine($"Подсчёт слов в тексте, нажмите Ctrl-Z для окончания ввода");
            string? line;
            MemoryStream stream = new MemoryStream();
            StreamWriter sw = new StreamWriter(stream);
            while (true)
            {
                line = Console.ReadLine();
                if (line == null || line.Contains('\u001A')) break;
                sw.WriteLine(line);
            }
            sw.Flush();
            stream.Position = 0;
            Input = new StreamReader(stream);
            return ProcessText(doWrite, doParallel);
        }

        /// <summary>
        /// Starts the WordCounter using string value as input. Reads and processes data, then writes (if doWrite is true) and returns it
        /// </summary>
        /// <param name="data">If true, writes the result to specified Output, true by default</param>
        /// <param name="doWrite">If true, writes the result to specified Output, false by default</param>
        /// <param name="doParallel">If true, processes data in parallel by partitioning the blocks of data. Recommended to use with a memory limit of at least 100 Kb, false by default</param>
        /// <returns>The result of data processing</returns>
        public long ProcessString(string data, bool doWrite = false, bool doParallel = false)
        {
            SetStringInput(data);
            return ProcessText(doWrite, doParallel);
        }

        /// <summary>
        /// Sets a string value as a WordCounter input
        /// </summary>
        /// <param name="data">A string value</param>
        public void SetStringInput(string data)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter sw = new StreamWriter(stream);
            sw.Write(data);
            sw.Flush();
            stream.Position = 0;
            Input = new StreamReader(stream);
        }

        /// <summary>
        /// Sets console as WordCounter output
        /// </summary>
        public void SetConsoleOutput() => Output = new StreamWriter(Console.OpenStandardOutput());
    }
}
