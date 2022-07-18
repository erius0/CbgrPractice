Console.WriteLine("Вычисление квадратного корня");
while (true)
{
    Console.Write("Введите число: ");
    string? input = Console.ReadLine();

    bool isInputNumeric = double.TryParse(input, out double inputNumber);
    if (!isInputNumeric)
    {
        Console.WriteLine($"Ошибка: '{input}' не является числом");
        continue;
    }
    if (inputNumber < 0)
    {
        Console.WriteLine("Ошибка: корень отрицательного числа вычислить невозможно");
        continue;
    }

    double result = Math.Sqrt(inputNumber);
    Console.WriteLine($"Результат вычислений: {result}");
    Console.WriteLine();
}
