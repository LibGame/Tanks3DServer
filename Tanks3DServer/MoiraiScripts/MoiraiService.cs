using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Tanks3DServer.MoiraiScripts
{
    internal class MoiraiService
    {
        private readonly string MoiraiCliPath = @"C:\Users\VIP\source\repos\Tanks3DServer\MoiraiAPI\moirai-cli.exe";
        private readonly string MoiraiConfPath = @"C:\Users\VIP\AppData\Roaming\Moirai\moirai.conf";

        public string ExecuteMoiraiCliCommand(string[] command)
        {
            try
            {
                // Формируем команду
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = MoiraiCliPath,
                    Arguments = $"-conf={MoiraiConfPath} {string.Join(" ", command)}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(processStartInfo))
                {
                    if (process == null)
                        throw new Exception("Не удалось запустить moirai-cli");

                    process.WaitForExit();
                    string result = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    if (process.ExitCode != 0)
                        throw new Exception($"Ошибка выполнения команды: {error}");

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка: {ex.Message}");
            }
        }

        public async Task<string> HandleCommand(string command , object[] parametrs = null)
        {
            string rpcUrl = "http://localhost:22555";
            string rpcUser = "horizontal";
            string rpcPassword = "labrador75";
            string result = "";

            List<Transaction> transactionList = new List<Transaction>();

            try
            {
                using (var client = new HttpClient())
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, rpcUrl);

                    // Добавляем Basic Auth для RPC
                    var byteArray = Encoding.ASCII.GetBytes($"{rpcUser}:{rpcPassword}");
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    string jsonRpcString = "";

                    if (parametrs != null)
                    {
                        var jsonRpcRequest = new
                        {
                            jsonrpc = "1.0",
                            id = "curltest",
                            method = command,
                            @params = parametrs
                        };
                        jsonRpcString = Newtonsoft.Json.JsonConvert.SerializeObject(jsonRpcRequest);
                    }
                    else
                    {
                        var jsonRpcRequest = new
                        {
                            jsonrpc = "1.0",
                            id = "curltest",
                            method = command
                        };
                        jsonRpcString = Newtonsoft.Json.JsonConvert.SerializeObject(jsonRpcRequest);
                    }
                    // Конвертируем запрос в JSON
                    request.Content = new StringContent(jsonRpcString, Encoding.UTF8, "application/json");

                    // Отправляем запрос и получаем ответ
                    var response = await client.SendAsync(request);
                    var responseString = await response.Content.ReadAsStringAsync();

                    JObject jsonResponse = JObject.Parse(responseString);
                    result = jsonResponse["result"].ToString();
                    Console.WriteLine("jsonResponse " + jsonResponse["error"].ToString());
                    if (!string.IsNullOrEmpty(jsonResponse["error"].ToString()))
                    {
                        result = "error";
                    }

                    Console.WriteLine("result " + result);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");

            }
            return result;
        }

        //public async Task<List<Transaction>> GetTransactions(string walletAddress)
        //{
        //    string rpcUrl = "http://localhost:22555";
        //    string rpcUser = "kiberuzbek";
        //    string rpcPassword = "labrador75";

        //    List<Transaction> transactionList = new List<Transaction>();

        //    using (var client = new HttpClient())
        //    {
        //        var request = new HttpRequestMessage(HttpMethod.Post, rpcUrl);

        //        // Добавляем Basic Auth для RPC
        //        var byteArray = Encoding.ASCII.GetBytes($"{rpcUser}:{rpcPassword}");
        //        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

        //        // Формируем JSON-RPC запрос
        //        var jsonRpcRequest = new
        //        {
        //            jsonrpc = "1.0",
        //            id = "curltest",
        //            method = "listtransactions",
        //            @params = new object[] { walletAddress, 5 }
        //        };

        //        // Конвертируем запрос в JSON
        //        string jsonRpcString = Newtonsoft.Json.JsonConvert.SerializeObject(jsonRpcRequest);
        //        request.Content = new StringContent(jsonRpcString, Encoding.UTF8, "application/json");

        //        // Отправляем запрос и получаем ответ
        //        var response = await client.SendAsync(request);
        //        var responseString = await response.Content.ReadAsStringAsync();

        //        // Парсим JSON ответ
        //        JObject jsonResponse = JObject.Parse(responseString);
        //        var transactions = jsonResponse["result"];

        //        // Создаем список для транзакций

        //        // Обрабатываем каждую транзакцию
        //        foreach (var tx in transactions)
        //        {
        //            var transaction = new Transaction
        //            {
        //                txid = tx["txid"]?.ToString(),
        //                category = tx["category"]?.ToString(),
        //                amount = tx["amount"]?.ToObject<decimal>() ?? 0,
        //            };

        //            // Добавляем транзакцию в список
        //            transactionList.Add(transaction);
        //        }
        //    }
        //    return transactionList;
        //}

        public async Task<string> CreateAddress()
        {
            return await HandleCommand("getnewaddress");
        }
        public async Task<string> GetDumpPrivKeyAddress(string address)
        {
            return await HandleCommand("dumpprivkey", [address]);
        }
        public async Task<string> GetBalance(string address)
        {
            // Вызов команды 'getreceivedbyaddress'
            return await HandleCommand("getbalance");
        }

        public async Task<List<Transaction>> GetTransactions(string txid , bool onlyRecieve = false)
        {
            string json = await HandleCommand("listtransactions");
            List<Transaction> transactionResponse = JsonConvert.DeserializeObject<List<Transaction>>(json);

            Console.WriteLine("transactionResponse " + transactionResponse.Count);

            List<Transaction> transactionsResult = new List<Transaction>();

            foreach (var item in transactionResponse)
            {
                if (item.Address.Trim().Equals(txid.Trim()))
                {
                    if (onlyRecieve)
                    {
                        if(item.Category == "receive")
                        {
                            transactionsResult.Add(item);
                        }
                    }
                    else
                    {
                        transactionsResult.Add(item);
                    }
                }
            }

            return transactionsResult;
        }


        public async Task<string> Send(string address, decimal amount)
        {
            string result = await HandleCommand("sendtoaddress" , [address, amount]);
            Console.WriteLine("Send result " + result);
            return result;
        }

        public async Task<List<Transaction>> GetAllTransactionForAddress(string targetAddress)
        {
            // Параметры подключения к RPC
            string rpcUrl = "http://localhost:22555";
            string rpcUser = "kiberuzbek";
            string rpcPassword = "labrador75";

            // Укажите адрес, по которому нужно фильтровать транзакции

            using (var client = new HttpClient())
            {
                var transactionList = new List<Transaction>();
                int count = 100;  // Количество транзакций за запрос
                int skip = 0;     // Параметр пропуска (пагинация)
                bool moreTransactions = true; // Флаг для завершения цикла, если транзакции закончились

                while (moreTransactions)
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, rpcUrl);

                    // Добавляем Basic Auth для RPC
                    var byteArray = Encoding.ASCII.GetBytes($"{rpcUser}:{rpcPassword}");
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    // Формируем JSON-RPC запрос
                    var jsonRpcRequest = new
                    {
                        jsonrpc = "1.0",
                        id = "curltest",
                        method = "listtransactions",
                        @params = new object[] { "*", count, skip }  // Указываем количество и пропуск
                    };

                    // Конвертируем запрос в JSON
                    string jsonRpcString = Newtonsoft.Json.JsonConvert.SerializeObject(jsonRpcRequest);
                    request.Content = new StringContent(jsonRpcString, Encoding.UTF8, "application/json");

                    // Отправляем запрос и получаем ответ
                    var response = await client.SendAsync(request);
                    var responseString = await response.Content.ReadAsStringAsync();

                    // Парсим JSON ответ
                    JObject jsonResponse = JObject.Parse(responseString);
                    var transactions = jsonResponse["result"] as JArray;

                    // Если транзакции вернулись, обрабатываем их
                    if (transactions != null && transactions.Count > 0)
                    {
                        foreach (var tx in transactions)
                        {
                            // Фильтруем транзакции по адресу
                            if (tx["address"]?.ToString().Trim() == targetAddress.Trim())
                            {
                                var transaction = new Transaction
                                {
                                    Txid = tx["txid"]?.ToString(),
                                    Category = tx["category"]?.ToString(),
                                    Amount = tx["amount"]?.ToObject<decimal>() ?? 0,
                                    Address = tx["address"]?.ToString(),
                                };

                                transactionList.Add(transaction);
                            }
                        }

                        // Увеличиваем параметр skip для следующей выборки
                        skip += count;
                    }
                    else
                    {
                        // Если транзакций больше нет, выходим из цикла
                        moreTransactions = false;
                    }
                }
                Console.WriteLine($"Всего транзакций для адреса {targetAddress}: {transactionList.Count}");
                return transactionList;
            }
        }
    }
}
