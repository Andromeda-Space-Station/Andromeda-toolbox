﻿using Content.Server.Administration.Managers;
using Content.Server.Database;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.Administration.Commands
{
    [AdminCommand(AdminFlags.Ban)]
    public sealed class PardonCommand : LocalizedCommands
    {
        [Dependency] private readonly IServerDbManager _dbManager = default!;
        
        private BanWebhookManager? _banWebhookManager;
        private BanWebhookManager GetBanWebhookManager()
        {
            if (_banWebhookManager == null)
            {
                _banWebhookManager = IoCManager.Resolve<BanWebhookManager>();
            }
            return _banWebhookManager;
        }

        public override string Command => "pardon";

        public override async void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            var player = shell.Player;

            if (args.Length != 1)
            {
                shell.WriteLine(Help);
                return;
            }

            if (!int.TryParse(args[0], out var banId))
            {
                shell.WriteLine(Loc.GetString($"cmd-pardon-unable-to-parse", ("id", args[0]), ("help", Help)));
                return;
            }

            var ban = await _dbManager.GetServerBanAsync(banId);

            if (ban == null)
            {
                shell.WriteLine($"No ban found with id {banId}");
                return;
            }

            if (ban.Unban != null)
            {
                if (ban.Unban.UnbanningAdmin != null)
                {
                    shell.WriteLine(Loc.GetString($"cmd-pardon-already-pardoned-specific",
                        ("admin", ban.Unban.UnbanningAdmin.Value),
                        ("time", ban.Unban.UnbanTime)));
                }

                else
                    shell.WriteLine(Loc.GetString($"cmd-pardon-already-pardoned"));

                return;
            }

            await _dbManager.AddServerUnbanAsync(new ServerUnbanDef(banId, player?.UserId, DateTimeOffset.Now));

            // Отправляем вебхук для разбана
            var adminName = player?.Name ?? Loc.GetString("system-user");
            
            _ = GetBanWebhookManager().SendUnbanWebhook(
                banId,
                player?.UserId,
                adminName
            );

            shell.WriteLine(Loc.GetString($"cmd-pardon-success", ("id", banId)));
        }
    }
}