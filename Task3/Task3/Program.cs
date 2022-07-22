using Task3;
using Mono.Options;

string? path = null;
int memoryLimit = WordCounter.DEFAULT_MEMORY_LIMIT;
bool doParallel = false, showHelp = false;
var options = new OptionSet()
{
    { "f|file=", "the {FILE} to use as input", v => path = v },
    { "m|memory=", "the max amount of {MEMORY} used while reading data", v =>
        {
            bool success = int.TryParse(v, out int memory);
            if (success) memoryLimit = memory;
        }
    },
    { "p", "count words using multiple threads", v => doParallel = v != null },
    { "h", "show help", v => showHelp = v != null }
};
options.Parse(args);

if (showHelp)
{
    ShowHelp(options);
    return;
}

WordCounter wordCounter = new WordCounter(memoryLimit);
long result = 0;
if (path != null)
{
    try
    {
        result = wordCounter.ProcessFileInput(path, doParallel: doParallel);
    }
    catch (FileNotFoundException ex)
    {
        Console.WriteLine(ex.Message);
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine(ex.Message);
    }
}
else result = wordCounter.ProcessConsoleInput(doParallel: doParallel);
Console.WriteLine(result);

static void ShowHelp(OptionSet options)
{
    Console.WriteLine("Usage: ./Task3.exe [OPTIONS]+ message");
    Console.WriteLine("Counts amount of words in the specified input.");
    Console.WriteLine();
    Console.WriteLine("Options:");
    options.WriteOptionDescriptions(Console.Out);
}
