using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Task3
{
    public class WordCounter
    {
        public const int DEFAULT_MEMORY_LIMIT = 1_048_576; // 1 Mb

        /// <summary>
        /// Memory limit for reading data in bytes, set to 1_048_576 bytes
        /// </summary>
        public int MemoryLimit { get; set; }

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
        /// <param name="output">StreamWriter outputting the result</param>
        /// <param name="memoryLimit">Memory limit for reading data in bytes, set to 1_048_576 by default</param>
        public WordCounter(StreamWriter output, int memoryLimit = DEFAULT_MEMORY_LIMIT)
        {
            Output = output;
            MemoryLimit = memoryLimit;
        }

        /// <summary>
        /// Creates an instance of WordCounter with the specified memory limit for reading data and an input stream. Sets console as an output
        /// </summary>
        /// <param name="memoryLimit">Memory limit for reading data in bytes, set to 1_048_576 by default</param>
        public WordCounter(int memoryLimit = DEFAULT_MEMORY_LIMIT) : this(new StreamWriter(Console.OpenStandardOutput()), memoryLimit) { }

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
        public long ProcessText(StreamReader input, bool doWrite = true, bool doParallel = true)
        {
            Func<char[], char, int, long> countingFunc = doParallel ? CountWordsParallel : CountWordsNonParallel; // determine the processing function to use (no if statememnt on every read)
            char prevChar = ' ';
            char[] currentBlock = new char[MemoryLimit]; // init buffer for the data
            int charsRead = input.ReadBlock(currentBlock, 0, MemoryLimit); // first read
            long wordsAmount = 0;
            // while there are chars being read, continue reading
            while (charsRead > 0)
            {
                wordsAmount += countingFunc(currentBlock, prevChar, charsRead); // process data
                prevChar = currentBlock[^1]; // set the prev char to the last char of the current block
                charsRead = input.ReadBlock(currentBlock, 0, MemoryLimit); // read data
            }
            string result = wordsAmount.ToString();
            if (doWrite) WriteData(result); // write data to output if needed
            // release resources
            input.Dispose();
            return wordsAmount;
        }

        /// <summary>
        /// Starts the WordCounter. Reads and processes data, then writes (if doWrite is true) and returns it
        /// </summary>
        /// <param name="data">A string to process</param>
        /// <param name="doWrite">If true, writes the result to specified Output, true by default</param>
        /// <param name="doParallel">If true, processes data in parallel by partitioning the blocks of data. Recommended to use with a memory limit of at least 100 Kb, true by default</param>
        /// <returns></returns>
        public long ProcessString(string data, bool doWrite = false, bool doParallel = false)
        {
            long wordsAmount;
            if (doParallel) wordsAmount = CountWordsParallel(data.ToCharArray(), ' ', data.Length);
            else wordsAmount = CountWordsNonParallel(data.ToCharArray(), ' ', data.Length);
            string result = wordsAmount.ToString();
            if (doWrite) WriteData(result); // write data to output if needed
            // release resources
            return wordsAmount;
        }

        /// <summary>
        /// Starts the WordCounter using file as input. Reads and processes data, then writes (if doWrite is true) and returns it
        /// </summary>
        /// <param name="filePath">Name of the file used for input</param>
        /// <exception cref="FileNotFoundException">If the file doesn't exist</exception>
        /// <exception cref="ArgumentException">If the specified file is a directory</exception>
        public long ProcessFileInput(string filePath, bool doWrite = true, bool doParallel = true)
        {
            FileInfo fileInfo = FindFile(filePath);
            FileStream fileStream = fileInfo.OpenRead();
            StreamReader fileReader = new StreamReader(fileStream, TextEncoding);
            return ProcessText(fileReader, doWrite, doParallel);
        }

        private static FileInfo FindFile(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists) throw new FileNotFoundException($"File {filePath} was not found");
            if (fileInfo.Attributes.HasFlag(FileAttributes.Directory)) throw new ArgumentException($"Expected a file, got a directory from path {filePath}");
            return fileInfo;
        }

        /// <summary>
        /// Starts the WordCounter using console as input. Reads and processes data, then writes (if doWrite is true) and returns it
        /// </summary>
        /// <param name="doWrite">If true, writes the result to specified Output, true by default</param>
        /// <param name="doParallel">If true, processes data in parallel by partitioning the blocks of data. Recommended to use with a memory limit of at least 100 Kb, false by default</param>
        /// <returns>The result of data processing</returns>
        public long ProcessConsoleInput(bool doWrite = true, bool doParallel = false)
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
            StreamReader console = new StreamReader(stream);
            return ProcessText(console, doWrite, doParallel);
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
        /// Sets console as WordCounter output
        /// </summary>
        public void SetConsoleOutput() => Output = new StreamWriter(Console.OpenStandardOutput());
    }
}
