using Microsoft.Extensions.Logging;
using TwitchChatBot.Core.Services.Contracts;
using TwitchChatBot.Data.Contracts;

namespace TwitchChatBot.Core.Services
{
    public class ExcludedUsersService : IExcludedUsersService
    {
        private readonly IExcludedUsersRepository _excludedUsersRepository;
        private readonly ILogger<ExcludedUsersService> _logger;
        
        public ExcludedUsersService(IExcludedUsersRepository excludedUsersRepository, ILogger<ExcludedUsersService> logger)
        {
            _excludedUsersRepository = excludedUsersRepository;
            _logger = logger;
        }

        public async Task<bool> IsUserExcludedAsync(string username)
        {
            return await _excludedUsersRepository.IsUserExcludedAsync(username.ToLower());
        }
    }
}