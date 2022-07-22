using Task3;

if (args.Length > 0)
{
    string path = args[0];
    bool doParallel = args.Length > 1 && args[1] == "-p";
    try
    {
        WordCounter wordCounter = new WordCounter(path);
        wordCounter.ProcessText(doParallel: doParallel);
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
else
{
    WordCounter wordCounter = new WordCounter();
    wordCounter.ProcessTextConsole();
}

