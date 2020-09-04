using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

public static class MyExtensions
{
    public static async Task<T?> ReceiveUnlessCompleteAsync<T>(this IReceivableSourceBlock<T> source)
        where T: struct
    {
        try
        {
            return await source.ReceiveAsync();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }
}

internal static class Program
{
    private static void StayBusy(double seconds)
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        while (stopwatch.Elapsed.TotalSeconds < seconds) ;
    }

    private static void Main()
    {
        var chan = new BufferBlock<int>(new DataflowBlockOptions { BoundedCapacity = 4 });
        Task.Factory.StartNew(async delegate
            {
                for (int i = 1; i <= 7; ++i)
                {
                    await chan.SendAsync(i);
                    Console.WriteLine($"produced {i}");
                    StayBusy(0.75);
                }
                chan.Complete();
                Console.WriteLine("stop producing");
            },
            CancellationToken.None,
            TaskCreationOptions.None,
            TaskScheduler.Default).Unwrap();

        var task = Task.Factory.StartNew(async delegate
            {
                while ((await chan.ReceiveUnlessCompleteAsync()) is int i)
                {
                    Console.WriteLine($"consumed {i}");
                    StayBusy(1.25);
                }
                Console.WriteLine("stop consuming");
            },
            CancellationToken.None,
            TaskCreationOptions.None,
            TaskScheduler.Default).Unwrap();
        task.Wait();
        Console.WriteLine("stop waiting");
    }
}