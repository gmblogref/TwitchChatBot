﻿namespace TwitchChatBot.Core.Services.Contracts
{
    public interface IExcludedUsersService
    {
        Task<bool> IsUserExcludedAsync(string username);
    }
}