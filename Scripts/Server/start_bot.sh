#!/bin/bash
# Запуск бота в фоне

cd "$(dirname "$0")"

# Проверяем не запущен ли уже
if ps aux | grep -v grep | grep -q "discord_server_bot.py"; then
    echo "Бот уже запущен!"
    exit 1
fi

source venv/bin/activate

# Запускаем в фоне с nohup
nohup python3 discord_server_bot.py > /dev/null 2>&1 &
BOT_PID=$!

echo "Бот запущен в фоне"
echo "PID: $BOT_PID"
echo "Для остановки: ./stop_bot.sh"