using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Роль, которая будет упомянута, если поступает новый аларм помощи (SOS ahelp).
    /// </summary>
    public static readonly CVarDef<string> DiscordAhelpMention =
        CVarDef.Create("discord.on_call_ping", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    ///     URL вебхука Discord для ретрансляции неотвеченных сообщений помощи (ahelp).
    /// </summary>
    public static readonly CVarDef<string> DiscordOnCallWebhook =
        CVarDef.Create("discord.on_call_webhook", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    ///     URL вебхука Discord, который будет ретранслировать все сообщения помощи (ahelp).
    /// </summary>
    public static readonly CVarDef<string> DiscordAHelpWebhook =
        CVarDef.Create("discord.ahelp_webhook", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    ///     Иконка сервера для использования в подвале (footer) встраиваемого сообщения (embed) Discord помощи (ahelp).
    ///     Допустимые значения указаны на https://discord.com/developers/docs/resources/channel#embed-object-embed-footer-structure.
    /// </summary>
    public static readonly CVarDef<string> DiscordAHelpFooterIcon =
        CVarDef.Create("discord.ahelp_footer_icon", string.Empty, CVar.SERVERONLY);

    /// <summary>
    ///     Аватар для использования вебхуком. Должен быть URL.
    /// </summary>
    public static readonly CVarDef<string> DiscordAHelpAvatar =
        CVarDef.Create("discord.ahelp_avatar", string.Empty, CVar.SERVERONLY);

    /// <summary>
    ///     URL вебхука Discord, который будет ретранслировать все пользовательские голосования. Если оставить пустым, отключает вебхук.
    /// </summary>
    public static readonly CVarDef<string> DiscordVoteWebhook =
        CVarDef.Create("discord.vote_webhook", string.Empty, CVar.SERVERONLY);

    /// <summary>
    ///     URL вебхука Discord, который будет ретранслировать все голосования за кик (votekick). Если оставить пустым, отключает вебхук.
    /// </summary>
    public static readonly CVarDef<string> DiscordVotekickWebhook =
        CVarDef.Create("discord.votekick_webhook", string.Empty, CVar.SERVERONLY);

    /// <summary>
    ///     URL вебхука Discord, который будет ретранслировать сообщения о перезапуске раунда.
    /// </summary>
    public static readonly CVarDef<string> DiscordRoundUpdateWebhook =
        CVarDef.Create("discord.round_update_webhook", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    ///     ID роли Discord для упоминания вебхуком при окончании раунда.
    /// </summary>
    public static readonly CVarDef<string> DiscordRoundEndRoleWebhook =
        CVarDef.Create("discord.round_end_role", string.Empty, CVar.SERVERONLY);


    /// <summary>
    ///     Токен, используемый для аутентификации в Discord. Для работы бота установите: discord.token, discord.guild_id и discord.prefix.
    ///     Если это пусто, бот не подключится.
    /// </summary>
    public static readonly CVarDef<string> DiscordToken =
        CVarDef.Create("discord.token", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    ///     ID гильдии Discord для использования команд, а также для нескольких других функций.
    ///     Если это пусто, бот не подключится.
    /// </summary>
    public static readonly CVarDef<string> DiscordGuildId =
        CVarDef.Create("discord.guild_id", string.Empty, CVar.SERVERONLY);

    /// <summary>
    ///     Префикс, используемый для команд Discord бота.
    ///     Если это пусто, бот не подключится.
    /// </summary>
    public static readonly CVarDef<string> DiscordPrefix =
        CVarDef.Create("discord.prefix", "!", CVar.SERVERONLY);

    /// <summary>
    ///     URL вебхука Discord, который будет ретранслировать уведомления о подключениях из списка наблюдения (watchlist). Если оставить пустым, отключает вебхук.
    /// </summary>
    public static readonly CVarDef<string> DiscordWatchlistConnectionWebhook =
        CVarDef.Create("discord.watchlist_connection_webhook", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    ///     Как долго буферизировать подключения из списка наблюдения, в секундах.
    ///     Все подключения в течение этого времени с момента первого будут объединены в пакет и отправлены как одно
    ///     уведомление Discord. Если ноль, всегда отправляет отдельное уведомление для каждого подключения (не рекомендуется).
    /// </summary>
    public static readonly CVarDef<float> DiscordWatchlistConnectionBufferTime =
        CVarDef.Create("discord.watchlist_connection_buffer_time", 5f, CVar.SERVERONLY);

    /// <summary>
    ///     URL вебхука Discord, который будет получать статьи станционных новостей в конце раунда.
    ///     Если оставить пустым, отключает вебхук.
    /// </summary>
    public static readonly CVarDef<string> DiscordNewsWebhook =
        CVarDef.Create("discord.news_webhook", string.Empty, CVar.SERVERONLY);

    /// <summary>
    ///     HEX цвет встраиваемого сообщения (embed) вебхука Discord станционных новостей.
    /// </summary>
    public static readonly CVarDef<string> DiscordNewsWebhookEmbedColor =
        CVarDef.Create("discord.news_webhook_embed_color", Color.LawnGreen.ToHex(), CVar.SERVERONLY);

    /// <summary>
    ///     Следует ли отправлять статьи в середине раунда вместо того, чтобы отправлять все сразу в конце раунда.
    /// </summary>
    public static readonly CVarDef<bool> DiscordNewsWebhookSendDuringRound =
        CVarDef.Create("discord.news_webhook_send_during_round", false, CVar.SERVERONLY);

    /// <summary>
    /// URL вебхука Discord для уведомлений о банах/разбанах/блокировках ролей.
    /// </summary>
    public static readonly CVarDef<string> DiscordBanWebhook =
        CVarDef.Create("discord.ban_webhook", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    /// Цвет embed'а для банов (HEX).
    /// </summary>
    public static readonly CVarDef<string> DiscordBanEmbedColor =
        CVarDef.Create("discord.ban_embed_color", "#FF0000", CVar.SERVERONLY);

    /// <summary>
    /// Цвет embed'а для блокировок ролей (HEX).
    /// </summary>
    public static readonly CVarDef<string> DiscordRoleBanEmbedColor =
        CVarDef.Create("discord.roleban_embed_color", "#FFA500", CVar.SERVERONLY);

    /// <summary>
    /// Цвет embed'а для разбанов (HEX).
    /// </summary>
    public static readonly CVarDef<string> DiscordUnbanEmbedColor =
        CVarDef.Create("discord.unban_embed_color", "#04af04", CVar.SERVERONLY);
}