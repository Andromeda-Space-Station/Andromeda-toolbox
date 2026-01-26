using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Content.Server.Administration.Managers;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Asynchronous;
using Robust.Shared.Log;
using Robust.Server.Player;

namespace Content.Server.Administration.Managers;

public sealed class BanWebhookManager
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly ITaskManager _taskManager = default!;
    
    private ISawmill _sawmill = default!;
    private readonly HttpClient _httpClient = new();
    
    private bool _initialized = false;
    
    private void EnsureInitialized()
    {
        if (_initialized)
            return;
            
        _sawmill = _logManager.GetSawmill("admin.bans.webhook");
        _initialized = true;
    }
    
    private async Task SendWebhook(string title, string description, string colorHex, Dictionary<string, string> fields)
    {
        EnsureInitialized();
        
        var webhookUrl = _cfg.GetCVar(CCVars.DiscordBanWebhook);
        if (string.IsNullOrWhiteSpace(webhookUrl))
            return;
        
        try
        {
            var embedFields = new List<object>();
            foreach (var kvp in fields)
            {
                embedFields.Add(new
                {
                    name = kvp.Key,
                    value = kvp.Value,
                    inline = true
                });
            }
            
            var embed = new
            {
                title = title,
                description = description,
                color = Convert.ToInt32(colorHex.TrimStart('#'), 16),
                timestamp = DateTimeOffset.UtcNow.ToString("o"),
                fields = embedFields
            };
            
            var payload = new
            {
                embeds = new[] { embed }
            };
            
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var response = await _httpClient.PostAsync(webhookUrl, content);
            
            if (!response.IsSuccessStatusCode)
            {
                _sawmill.Error($"Failed to send ban webhook: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _sawmill.Error($"Exception while sending ban webhook: {ex}");
        }
    }
    
    public async Task SendBanWebhook(
        NetUserId? targetUserId,
        string? targetUsername,
        NetUserId? adminUserId,
        string? adminUsername,
        uint? minutes,
        string reason,
        string severity,
        string? role = null)
    {
        EnsureInitialized();
        
        var isRoleBan = !string.IsNullOrEmpty(role);
        var colorHex = isRoleBan 
            ? _cfg.GetCVar(CCVars.DiscordRoleBanEmbedColor)
            : _cfg.GetCVar(CCVars.DiscordBanEmbedColor);
        
        var duration = minutes.HasValue && minutes.Value > 0 
            ? $"{minutes.Value} минут" 
            : "Навсегда";
        
        var adminName = adminUsername ?? (adminUserId?.ToString() ?? "Система");
        var targetName = targetUsername ?? (targetUserId?.ToString() ?? "N/A");
        
        var title = isRoleBan 
            ? $"Блокировка роли: {role}" 
            : "Игровая блокировка";
        
        var fields = new Dictionary<string, string>
        {
            ["Администратор"] = adminName,
            ["Цель"] = targetName,
            ["Причина"] = reason,
            ["Длительность"] = duration,
            ["Серьёзность"] = severity
        };
        
        var description = isRoleBan
            ? $"Роль **{role}** заблокирована для игрока"
            : "Игрок заблокирован на сервере";
        
        await SendWebhook(title, description, colorHex, fields);
    }
    
    public async Task SendUnbanWebhook(
        int banId,
        NetUserId? adminUserId,
        string? adminUsername,
        string? role = null)
    {
        EnsureInitialized();
        
        var colorHex = _cfg.GetCVar(CCVars.DiscordUnbanEmbedColor);
        var isRoleUnban = !string.IsNullOrEmpty(role);
        
        var adminName = adminUsername ?? (adminUserId?.ToString() ?? "Система");
        
        var title = isRoleUnban 
            ? $"Разблокировка роли: {role}" 
            : "Разблокировка игрока";
        
        var fields = new Dictionary<string, string>
        {
            ["Администратор"] = adminName,
            ["ID бана"] = banId.ToString()
        };
        
        if (isRoleUnban)
        {
            fields["Роль"] = role!;
        }
        
        var description = isRoleUnban
            ? $"Роль **{role}** разблокирована для игрока"
            : "Игрок разблокирован на сервере";
        
        await SendWebhook(title, description, colorHex, fields);
    }
}