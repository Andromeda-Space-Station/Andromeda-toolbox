#!/bin/bash
# Копирует config.toml из Scripts/Server/ в bin/Content.Server/
# Работает с любым именем корневой папки

SRC="$(dirname "$0")/server_config.toml"
DST="$(dirname "$0")/../../bin/Content.Server/server_config.toml"

# Останавливаем сервер, если мешает
pkill -f Content.Server 2>/dev/null

# Удаляем если это симлинк или обычный файл
if [ -e "$DST" ]; then
    rm -f "$DST" 2>/dev/null
fi

# Создаём папку если нет
mkdir -p "$(dirname "$DST")"

# Копируем (или создаём если нет)
if [ -f "$SRC" ]; then
    cp -f "$SRC" "$DST"
    echo "Config updated"
else
    echo "Source file not found"
fi