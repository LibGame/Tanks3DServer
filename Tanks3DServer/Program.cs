using Autofac;
using EnglishWordsServer.AutofacConfiig;
using System.Net.Sockets;
using Tanks3DServer.Database;
using Tanks3DServer.GameSessionScripts;
using Tanks3DServer.MoiraiScripts;
using Tanks3DServer.ServerTCPScripts;
using Tanks3DServer.ServerUDPScripts;

public class Program
{
    public static async Task Main(string[] args)
    {
        AutofacProjectContext autofacProjectContext = new AutofacProjectContext();
        autofacProjectContext.RegisterInstalls();

        AutofacProjectContext.Container.Resolve<DatabaseService>().Init();

        ServerTCP serverTCP = AutofacProjectContext.Container.Resolve<ServerTCP>();
        ServerUDP serverUDP = AutofacProjectContext.Container.Resolve<ServerUDP>();
        AutofacProjectContext.Container.Resolve<GameHandler>().Init();
        MoiraiService moiraiService = AutofacProjectContext.Container.Resolve<MoiraiService>();

        var cts = new CancellationTokenSource();
        var token = cts.Token;

        // Запуск серверов в отдельных задачах
        Task serverTCPTask = Task.Run(() => serverTCP.Start(), token);
        Task serverUDPTask = Task.Run(() => serverUDP.Start(), token);

        // Создаем TaskCompletionSource для ожидания завершения работы
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

        // Подписываемся на событие отмены
        token.Register(() =>
        {
            Console.WriteLine("Cancellation requested.");
            tcs.SetResult(true);
        });

        // Отображаем сообщение и ждем завершения работы
        Console.WriteLine("Server is running. Press Ctrl+C to exit...");

        await tcs.Task; // Ожидаем завершения работы

        // Дополнительные действия перед завершением, если необходимо
        Console.WriteLine("Shutting down servers...");
        cts.Cancel(); // Отправляем сигнал для завершения работы

        // Ожидание завершения серверных задач
        await Task.WhenAll(serverTCPTask, serverUDPTask);
        Console.WriteLine("Server stopped.");
    }


}