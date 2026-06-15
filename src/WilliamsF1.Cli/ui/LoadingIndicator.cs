namespace WilliamsF1.Cli.ConsoleUi;

public static class LoadingIndicator
{
    public static async Task<T> RunAsync<T>(string message, Func<Task<T>> action)
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        var spinnerTask = ShowSpinnerAsync(message, cancellationTokenSource.Token);

        try
        {
            return await action();
        }
        finally
        {
            await cancellationTokenSource.CancelAsync();

            try
            {
                await spinnerTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when the spinner is cancelled.
            }

            ClearCurrentConsoleLine();
        }
    }

    private static async Task ShowSpinnerAsync(string message, CancellationToken cancellationToken)
    {
        var spinner = new[] { '|', '/', '-', '\\' };
        var index = 0;

        while (!cancellationToken.IsCancellationRequested)
        {
            Console.Write($"\r{message} {spinner[index++ % spinner.Length]}");
            await Task.Delay(100, cancellationToken);
        }
    }

    private static void ClearCurrentConsoleLine()
    {
        Console.Write("\r");
        Console.Write(new string(' ', Console.WindowWidth - 1));
        Console.Write("\r");
    }
}