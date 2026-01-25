using System.Numerics;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules;
using Content.Server.Station.Systems;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using Content.Shared.Random.Helpers;
using Robust.Server.Audio;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server.StationEvents.Events;

public sealed class MeteorSwarmSystem : GameRuleSystem<MeteorSwarmComponent>
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly StationSystem _station = default!;

    protected override void Added(EntityUid uid, MeteorSwarmComponent component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        component.WaveCounter = component.Waves.Next(RobustRandom);
        
        // Инициализируем время первой волны
        component.NextWaveTime = Timing.CurTime + TimeSpan.FromSeconds(component.WaveCooldown.Next(RobustRandom));

        // we don't want to send to players who aren't in game (i.e. in the lobby)
        Filter allPlayersInGame = Filter.Empty().AddWhere(GameTicker.UserHasJoinedGame);

        // Текстовый анонс начала
        if (component.Announcement is { } locId)
            _chat.DispatchFilteredAnnouncement(allPlayersInGame, Loc.GetString(locId), playSound: false, colorOverride: Color.Gold);

        // Звук начала
        _audio.PlayGlobal(component.AnnouncementSound, allPlayersInGame, true);
    }

    protected override void ActiveTick(EntityUid uid, MeteorSwarmComponent component, GameRuleComponent gameRule, float frameTime)
    {
        // Тройная защита от повторного вызова анонса завершения обстрела велекорусской спермой.
        // 
        // 1. Проверяем что правило активно
        if (!GameTicker.IsGameRuleActive(uid, gameRule))
            return;
        
        // 2. Проверяем что сущность не удалена
        if (MetaData(uid).EntityDeleted)
            return;
            
        // 3. Проверяем что мы еще не начали завершение
        if (component.Ending)
        {
            // Если уже начали завершение, ждем таймер
            if (Timing.CurTime >= component.EndSoundTime)
            {
                // ГООООЛ, запускаем анонс
                Filter allPlayersInGame = Filter.Empty().AddWhere(GameTicker.UserHasJoinedGame);
                
                // Текстовый анонс завершения
                if (component.EndAnnouncement is { } endLocId)
                    _chat.DispatchFilteredAnnouncement(allPlayersInGame, Loc.GetString(endLocId), playSound: false, colorOverride: Color.Gold);
                
                // Звук завершения
                if (component.EndSound != null)
                {
                    _audio.PlayGlobal(component.EndSound, allPlayersInGame, true);
                }
                
                ForceEndSelf(uid, gameRule);
            }
            return;
        }

        // Ждем следующую волну
        if (Timing.CurTime < component.NextWaveTime)
            return;

        component.NextWaveTime += TimeSpan.FromSeconds(component.WaveCooldown.Next(RobustRandom));

        if (_station.GetStations().Count == 0)
            return;

        var station = RobustRandom.Pick(_station.GetStations());
        if (_station.GetLargestGrid(station) is not { } grid)
            return;

        var mapId = Transform(grid).MapID;
        var playableArea = _physics.GetWorldAABB(grid);

        var minimumDistance = (playableArea.TopRight - playableArea.Center).Length() + 50f;
        var maximumDistance = minimumDistance + 100f;

        var center = playableArea.Center;

        var meteorsToSpawn = component.MeteorsPerWave.Next(RobustRandom);
        for (var i = 0; i < meteorsToSpawn; i++)
        {
            var spawnProto = RobustRandom.Pick(component.Meteors);

            var angle = component.NonDirectional
                ? RobustRandom.NextAngle()
                : new Random(uid.Id).NextAngle();

            var offset = angle.RotateVec(new Vector2((maximumDistance - minimumDistance) * RobustRandom.NextFloat() + minimumDistance, 0));

            // the line at which spawns occur is perpendicular to the offset.
            // This means the meteors are less likely to bunch up and hit the same thing.
            var subOffsetAngle = RobustRandom.Prob(0.5f)
                ? angle + Math.PI / 2
                : angle - Math.PI / 2;
            var subOffset = subOffsetAngle.RotateVec(new Vector2( (playableArea.TopRight - playableArea.Center).Length() / 3 * RobustRandom.NextFloat(), 0));

            var spawnPosition = new MapCoordinates(center + offset + subOffset, mapId);
            var meteor = Spawn(spawnProto, spawnPosition);
            var physics = Comp<PhysicsComponent>(meteor);
            _physics.ApplyLinearImpulse(meteor, -offset.Normalized() * component.MeteorVelocity * physics.Mass, body: physics);
        }

        component.WaveCounter--;
        
        // ВАЖНО: Проверяем кончились ли волны
        if (component.WaveCounter <= 0)
        {
            // Процесс завершения
            component.Ending = true;
            // Устанавливаем таймер на 1 минуту (EndDelay = 60) или другое значение, если поменяют
            component.EndSoundTime = Timing.CurTime + TimeSpan.FromSeconds(component.EndDelay);
        }
    }
}