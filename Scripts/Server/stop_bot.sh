#!/bin/bash
# Остановка бота

cd "$(dirname "$0")"

echo "Останавливаю бота..."

# Ищем процесс бота
PIDS=$(ps aux | grep "discord_server_bot.py" | grep -v grep | awk '{print $2}')

if [ -z "$PIDS" ]; then
    echo "Бот не запущен"
    exit 0
fi

echo "Найдены процессы: $PIDS"

# Останавливаем корректно
for PID in $PIDS; do
    echo "Останавливаю процесс $PID..."
    kill $PID 2>/dev/null
done

# Ждём 3 секунды
sleep 3

# Проверяем остались ли процессы
PIDS=$(ps aux | grep "discord_server_bot.py" | grep -v grep | awk '{print $2}')
if [ -n "$PIDS" ]; then
    echo "Принудительная остановка..."
    kill -9 $PIDS 2>/dev/null
fi

echo "Бот остановлен"