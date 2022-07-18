Console.WriteLine("Вычисление среднего 3-ех чисел");
while (true)
{
    double[] userNumbers = new double[3];

    for (int i = 0; i < 3; i++)
    {
        Console.Write($"Введите {i + 1} число: ");
        string? input = Console.ReadLine();
        bool isInputNumeric = double.TryParse(input, out userNumbers[i]);
        if (!isInputNumeric)
        {
            Console.WriteLine($"Ошибка: '{input}' не является числом");
            i--;
        }
    }

    double result = Task2Lib.Calculations.AverageOf3(userNumbers[0], userNumbers[1], userNumbers[2]);
    Console.WriteLine($"Результат вычислений: {result}");
    Console.WriteLine();
}
