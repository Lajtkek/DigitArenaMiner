using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace DigitArenaBot.Services;

public class HelperService
{
    private IConfigurationRoot _config;

    private ulong _privilegedRoleId;
    private List<ulong> _privilegedChannels;
    
    public HelperService(IConfigurationRoot config)
    {
        _config = config;

        _privilegedRoleId = config.GetSection("PrivilegedRole").Get<ulong>();
        _privilegedChannels = config.GetSection("AllowedChannels").Get<List<ulong>>() ?? new ();
    }

    public Task<bool> IsUserPrivileged(SocketUser user)
    {
        if (user is SocketGuildUser _user)
        {
            return Task.FromResult(_user.Roles.Select(x => x.Id).Contains(_privilegedRoleId));
        }

        return Task.FromResult(false);
    }

    public Task<bool> IsChannelPrivileged(ulong idChannel)
    {
        return Task.FromResult(_privilegedChannels.Contains(idChannel));
    }
}