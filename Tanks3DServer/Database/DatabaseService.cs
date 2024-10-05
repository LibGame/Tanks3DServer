using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tanks3DServer.DTO;
using Tanks3DServer.Handlers;
using Tanks3DServer.LobbyScripts.LobbyDTO;
using Tanks3DServer.LobbyScripts;
using Tanks3DServer.ServerTCPScripts;
using Tanks3DServer.Utilities;
using Autofac;
using Tanks3DServer.MoiraiScripts;
using EnglishWordsServer.AutofacConfiig;
using static Tanks3DServer.Utilities.IdentifyCheck;

namespace Tanks3DServer.Database
{
    internal class DatabaseService
    {
        private AppDbContext _context;
        private LobbyModel _lobbyModel;
        private MoiraiService _moiraiService;

        public void Init()
        {
            _context = new AppDbContext();
            _context.Database.EnsureCreated();
            _lobbyModel = AutofacProjectContext.Container.Resolve<LobbyModel>();
            _moiraiService = AutofacProjectContext.Container.Resolve<MoiraiService>();
            Console.WriteLine("Database initialized");
        }

        public async Task RegisterUserAsync(string username, WebSocketBehavior webSocket)
        {
            Console.WriteLine("RegisterUserAsync started");
            AuthResultType authResultType = AuthResultType.Registred;
            User user = null;
            Participant participant = new Participant(webSocket, username);

            // Проверяем, существует ли уже пользователь с указанным email или username
            //bool isEmailTaken = await _context.Users.AnyAsync(u => u.Email == email).ConfigureAwait(false);
            Console.WriteLine("RegisterUserAsync started 2");

            bool isUsernameTaken = await _context.Users.AnyAsync(u => u.Username == username);

            Console.WriteLine("RegisterUserAsync started 3");

            //if (isEmailTaken)
            //{
            //    authResultType = AuthResultType.BusyMail;
            //}
            if (isUsernameTaken)
            {
                authResultType = AuthResultType.BusyUsername;
            }
            Console.WriteLine("RegisterUserAsync started 4");


            if (authResultType == AuthResultType.Registred)
            {
                Console.WriteLine("RegisterUserAsync started 5");

                user = new User
                {
                    Username = username,
                    //Email = email,
                    //PasswordHash = HashPassword(password),
                    AuthCode = UsernameEncryptor.Encrypt(username),
                    ParticipantID = participant.Id,
                    Balance = 0,
                    ProccesedTransactionsJSon = "",
                    InventoryJSon = JsonConvert.SerializeObject(new UserTankInventory())
                };
                Console.WriteLine("RegisterUserAsync started 6");

                // Асинхронное создание адреса и получения приватного ключа
                user.WalletAddres = await _moiraiService.CreateAddress().ConfigureAwait(false);
                Console.WriteLine("User Wallet Address: " + user.WalletAddres);
                user.Dumpprivkey = await _moiraiService.GetDumpPrivKeyAddress(user.WalletAddres).ConfigureAwait(false);
                Console.WriteLine("User Dump Priv Key: " + user.Dumpprivkey);

                // Добавляем пользователя и сохраняем изменения в базе данных
                _context.Users.Add(user);
                int result = await _context.SaveChangesAsync().ConfigureAwait(false);
                Console.WriteLine("result " + result);
            }
            
            _lobbyModel.AddParticipant(participant);

            RegisterResponse response = new RegisterResponse
            {
                AuthResultType = authResultType,
                User = user,
                id = participant.Id
            };

            string responseData = JsonConvert.SerializeObject(response);
            Console.WriteLine("RegisterUserAsync response created");

            Message message = new Message(Handlers.HandlerTypes.Lobby, (int)LobbyHandlersType.RegisterResponse, "", responseData);
            string messageData = JsonConvert.SerializeObject(message);

            // Отправляем ответ через WebSocket
            _ = webSocket.SendMessageAsync(messageData);
            Console.WriteLine("RegisterUserAsync completed");
        }

        public async Task LoginAsync(string authCode, WebSocketBehavior webSocket)
        {
            AuthResultType authResultType = AuthResultType.Logined;
            //IdentifyType identifyType = IdentifyCheck.IdentifyUsernameOrEmail(emailOrUsername);
            User user = null;
            Console.WriteLine("LoginAsync 1");
            Participant participant = null;

            //// Ищем пользователя по имени пользователя или email
            //if (identifyType == IdentifyType.ByUsername)
            //{
            //    user = await _context.Users.FirstOrDefaultAsync(u => u.Username == emailOrUsername);
            //    if (user == null)
            //    {
            //        authResultType = AuthResultType.AccoundNotFound;
            //    }
            //}
            //else
            //{
            //    user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailOrUsername);
            //    if (user == null)
            //    {
            //        authResultType = AuthResultType.AccoundNotFound;
            //    }
            //}
            Console.WriteLine("LoginAsync 2");

            user = await _context.Users.FirstOrDefaultAsync(u => u.AuthCode == authCode);

            // Проверка пароля
            if (user == null)
            {
                authResultType = AuthResultType.WrongPassword;
            }

            // Обновление баланса пользователя
            if (user != null)
            {
                participant = new Participant(webSocket, user.Username);
                _lobbyModel.AddParticipant(participant);

                decimal startBalance = user.Balance;

                List<Transaction> processedTransactions = JsonConvert.DeserializeObject<List<Transaction>>(user.ProccesedTransactionsJSon);
                List<Transaction> fullTransactions = await _moiraiService.GetAllTransactionForAddress(user.WalletAddres);

                if(processedTransactions == null)
                    processedTransactions = new List<Transaction>();

                foreach (var transaction in fullTransactions)
                {
                    if (!processedTransactions.Contains(transaction))
                    {

                        if (transaction.Category == "receive")
                        {
                            user.Balance += Math.Abs(transaction.Amount);
                        }
                        else if (transaction.Category == "send")
                        {
                            user.Balance -= Math.Abs(transaction.Amount);
                        }
                        Console.WriteLine("user.Balance " + user.Balance);

                        processedTransactions.Add(transaction);
                    }
                }
                if (user.Balance < 0)
                    user.Balance = 0;
                if (!startBalance.Equals(user.Balance))
                {
                    user.ProccesedTransactionsJSon = JsonConvert.SerializeObject(processedTransactions);
                    await _context.SaveChangesAsync().ConfigureAwait(false);
                }
            }

            Console.WriteLine("LoginAsync 6");

            // Формируем и отправляем ответ
            LoginResponse response = new LoginResponse
            {
                AuthResultType = authResultType,
                User = user,
                
            };
            if (participant != null)
                response.id = participant.Id;
            else
                response.id = "";

            string responseData = JsonConvert.SerializeObject(response);
            Message message = new Message(Handlers.HandlerTypes.Lobby, (int)LobbyHandlersType.LoginResponse, "", responseData);

            _ = webSocket.SendMessageAsync(JsonConvert.SerializeObject(message));
        }

        public async Task SaveProccesedTransaction()
        {
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        private string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }

        public async Task<User> GetUserAsync(int userId)
        {
            using (var context = new AppDbContext())
            {
                return await context.Users.FindAsync(userId).ConfigureAwait(false);
            }
        }

        public async Task<User> AddCurrencyAsync(int userId, decimal amount)
        {
            var user = await _context.Users.FindAsync(userId).ConfigureAwait(false);
            if (user == null) throw new Exception("User not found");

            user.Balance += amount;
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User> SubtractCurrencyAsync(int userId, decimal amount)
        {
            var user = await _context.Users.FindAsync(userId).ConfigureAwait(false);
            if (user == null) throw new Exception("User not found");

            user.Balance -= amount;
            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User> BuyTank(int userId, int tankID , decimal amount)
        {
            var user = await _context.Users.FindAsync(userId).ConfigureAwait(false);
            if (user == null)
            {
                return null;
            }

            if(user.Balance < amount)
            {
                return null;
            }

            UserTankInventory inventory = JsonConvert.DeserializeObject<UserTankInventory>(user.InventoryJSon);

            if (inventory == null)
            {
                inventory = new UserTankInventory();
            }

            inventory.tanksBuyedID.Add(tankID);
            user.Balance -= amount;
            user.InventoryJSon = JsonConvert.SerializeObject(inventory);

            await _context.SaveChangesAsync();

            return await _context.Users.FindAsync(userId).ConfigureAwait(false);
        }
    }
}