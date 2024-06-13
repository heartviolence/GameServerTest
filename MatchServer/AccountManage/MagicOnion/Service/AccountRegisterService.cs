using MagicOnion.Server;
using MagicOnion;
using Microsoft.AspNetCore.Identity;
using Shared.Server.GameServer.Interface.Services;
using Server.EFcore.Models;
using Server.EFcore.Contexts;
using ServerShared.Database;

namespace ServerShared.AccountManage.MagicOnion.Service
{
    public class AccountRegisterService : ServiceBase<IAccountRegisterService>, IAccountRegisterService
    {
        readonly DbSettings dbSettings;
        public AccountRegisterService(DbSettings dbSettings)
        {
            this.dbSettings = dbSettings;
        }

        public async UnaryResult<bool> RegisterAccount(string loginId, string loginPassword)
        {
            var hasher = new PasswordHasher<object>();
            var hashPassword = hasher.HashPassword(null, loginPassword);

            try
            {
                using (var context = new GameDbContext(dbSettings.ConnectionString))
                {
                    context.Accounts.Add(new GameAccount(loginId, hashPassword));
                    await context.SaveChangesAsync();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }
    }
}
