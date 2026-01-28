import discord
import os
import subprocess
import asyncio
import time
import aiohttp
from pathlib import Path
from discord.ext import commands

# ========== НАСТРОЙКИ ==========
TOKEN = 'BotToken' # Токен бота
CHANNEL_ID = IdChannel  # ID канала для команд
ALLOWED_USERS = [IdUsers]  # ID пользователей

# Автоматическое определение путей
SCRIPT_DIR = Path(__file__).parent.absolute()  # Scripts/Server/
PROJECT_ROOT = SCRIPT_DIR.parent.parent        # Andromeda-toolbox/       У других может быть другое название, если вы форк
COMPILE_SCRIPT = PROJECT_ROOT / 'Scripts' / 'compile-release.sh'
RUN_SCRIPT = SCRIPT_DIR / 'runserver.sh'
# ===============================

intents = discord.Intents.default()
intents.message_content = True
bot = commands.Bot(command_prefix='!', intents=intents, help_command=None)

def is_allowed(ctx):
    """Проверка прав пользователя"""
    return ctx.author.id in ALLOWED_USERS

@bot.event
async def on_ready():
    print(f'Бот {bot.user} запущен!')
    print(f'Проект: {PROJECT_ROOT}')
    print(f'Компилятор: {COMPILE_SCRIPT}')
    print(f'Запуск: {RUN_SCRIPT}')
    print('=' * 50)

# ========== КОМАНДЫ ==========

@bot.command(name='update')
@commands.check(is_allowed)
async def update_cmd(ctx):
    """Остановить сервер, сделать git pull, скомпилировать и запустить"""
    if ctx.channel.id != CHANNEL_ID:
        return
    
    status_msg = await ctx.send('**Начинаю обновление сервера...**')
    results = []
    
    await status_msg.edit(content='**Останавливаю сервер...**')
    stop_result = stop_server()
    results.append(f"**Остановка:**\n```{stop_result[:1000]}```")
    await asyncio.sleep(3)
    
    await status_msg.edit(content='**Выполняю git pull...**')
    git_result = run_git_pull()
    results.append(f"**Git pull:**\n```{git_result[:1000]}```")
    
    await status_msg.edit(content='**Компилирую Release...**')
    compile_result = compile_release()
    results.append(f"**Компиляция:**\n```{compile_result[:1500]}```")
    
    await status_msg.edit(content='**Запускаю сервер...**')
    start_result = start_server()
    results.append(f"**Запуск:**\n```{start_result}```")
    
    await status_msg.edit(content='**Обновление завершено!**')
    
    report = "\n".join(results)
    if len(report) > 1900:
        for i in range(0, len(report), 1900):
            await ctx.send(f"```\n{report[i:i+1900]}\n```")
    else:
        await ctx.send(report)

@bot.command(name='stop')
@commands.check(is_allowed)
async def stop_cmd(ctx):
    """Остановить сервер"""
    result = stop_server()
    await ctx.send(f'```\n{result}\n```')

@bot.command(name='start')
@commands.check(is_allowed)
async def start_cmd(ctx):
    """Запустить сервер"""
    result = start_server()
    await ctx.send(f'```\n{result}\n```')

@bot.command(name='stop_bot')
@commands.check(is_allowed)
async def shutdown_bot(ctx):
    """Остановить Discord бота"""
    if ctx.channel.id != CHANNEL_ID:
        return
    
    await ctx.send('**Останавливаю бота...**')
    
    # Логируем отключение
    print(f"Бот остановлен по команде от {ctx.author}")
    
    # Закрываем соединение с Discord
    await bot.close()

@bot.command(name='restart')
@commands.check(is_allowed)
async def restart_cmd(ctx):
    """Перезапустить сервер"""
    stop_result = stop_server()
    await ctx.send(f'```\nОстановка:\n{stop_result}\n```')
    await asyncio.sleep(2)
    start_result = start_server()
    await ctx.send(f'```\nЗапуск:\n{start_result}\n```')

@bot.command(name='status')
async def status_cmd(ctx):
    """Показать статус сервера"""
    result = check_status()
    await ctx.send(f'```\n{result}\n```')

@bot.command(name='git')
@commands.check(is_allowed)
async def git_cmd(ctx, *, git_command='status'):
    """Выполнить git команду"""
    result = run_git_command(git_command)
    await ctx.send(f'```bash\n$ git {git_command}\n{result[:1500]}\n```')

@bot.command(name='compile')
@commands.check(is_allowed)
async def compile_cmd(ctx):
    """Только компиляция (без остановки/запуска)"""
    result = compile_release()
    await ctx.send(f'```\n{result[:1500]}\n```')

@bot.command(name='update_config')
@commands.check(is_allowed)
async def update_config_cmd(ctx, *, config_content=None):
    """Обновить конфиг из сообщения или файла, скомпилировать и запустить сервер"""
    if ctx.channel.id != CHANNEL_ID:
        return
    
    status_msg = await ctx.send('**Проверяю конфиг...**')
    
    try:
        config_text = None
        
        if ctx.message.attachments:
            for attachment in ctx.message.attachments:
                if attachment.filename.endswith(('.txt', '.toml', '.cfg', '.conf')):
                    await status_msg.edit(content=f'**Скачиваю {attachment.filename}...**')
                    config_text = await download_attachment(attachment)
                    break
        
        if not config_text and ctx.message.content:
            config_text = extract_config_from_message(ctx.message.content)
        
        if not config_text:
            async for message in ctx.channel.history(limit=5):
                if message.author == ctx.author:
                    if message.attachments:
                        for attachment in message.attachments:
                            if attachment.filename.endswith(('.txt', '.toml', '.cfg', '.conf')):
                                await status_msg.edit(content=f'**Скачиваю {attachment.filename} из предыдущего сообщения...**')
                                config_text = await download_attachment(attachment)
                                break
                    
                    if not config_text and message.content:
                        extracted = extract_config_from_message(message.content)
                        if extracted:
                            config_text = extracted
                            break
        
        if not config_text:
            await status_msg.edit(content='**Не найден конфиг.**\nОтправьте файл .txt/.toml или конфиг в код-блоке.')
            return
        
        await status_msg.edit(content='**Сохраняю конфиг...**')
        save_result = save_config(config_text)
        
        if "Ошибка" in save_result:
            await ctx.send(f'```\n{save_result}\n```')
            return
        
        await ctx.send(f'```\n{save_result}\n```')
        
        await status_msg.edit(content='**Применяю конфиг...**')
        config_update_result = run_existing_config_update()
        await ctx.send(f'```\n{config_update_result}\n```')
        
        await status_msg.edit(content='**Компилирую...**')
        compile_result = compile_release()
        await ctx.send(f'```\nКомпиляция:\n{compile_result[:1500]}\n```')
        
        await status_msg.edit(content='**Запускаю сервер...**')
        start_result = start_server()
        await ctx.send(f'```\n{start_result}\n```')
        
        await status_msg.edit(content='**Конфиг обновлён, проект скомпилирован и сервер запущен!**')
        
    except Exception as e:
        await status_msg.edit(content=f'**Ошибка:** {str(e)[:100]}')
        await ctx.send(f'```\nОшибка: {e}\n```')

# ========== ВСПОМОГАТЕЛЬНЫЕ ФУНКЦИИ ==========

async def download_attachment(attachment):
    """Скачивает и читает содержимое прикреплённого файла"""
    async with aiohttp.ClientSession() as session:
        async with session.get(attachment.url) as resp:
            if resp.status == 200:
                data = await resp.read()
                return data.decode('utf-8', errors='ignore')
    return None

def extract_config_from_message(content):
    """Извлекает конфиг из текста сообщения"""
    if not content:
        return None
    
    if '```' in content:
        lines = content.split('\n')
        in_code_block = False
        result_lines = []
        
        for line in lines:
            stripped = line.strip()
            
            if stripped.startswith('```'):
                if in_code_block:
                    break
                in_code_block = True
                continue
            
            if in_code_block:
                result_lines.append(line)
        
        if result_lines:
            return '\n'.join(result_lines)
    
    lines = content.split('\n')
    if lines and (lines[0].strip().startswith('[') or any('=' in line for line in lines[:5])):
        return content
    
    if content.startswith('!update_config'):
        parts = content.split(' ', 1)
        if len(parts) > 1:
            return parts[1].strip()
    
    return None

def save_config(config_content):
    """Сохраняет конфиг в файл"""
    try:
        config_path = SCRIPT_DIR / 'server_config.toml'
        config_content = config_content.strip()
        config_path.write_text(config_content, encoding='utf-8')
        
        if config_path.exists():
            size = config_path.stat().st_size
            lines = len(config_content.split('\n'))
            return f'Конфиг сохранён\nСтрок: {lines}\nРазмер: {size} байт\nПуть: {config_path}'
        else:
            return 'Ошибка: файл не сохранён'
            
    except Exception as e:
        return f'Ошибка сохранения: {e}'

def run_existing_config_update():
    """Запускает существующий config_update.sh"""
    try:
        config_update_script = SCRIPT_DIR / 'config_update.sh'
        
        if not config_update_script.exists():
            return f'Ошибка: файл не найден - {config_update_script}'
        
        if not os.access(config_update_script, os.X_OK):
            config_update_script.chmod(0o755)
        
        result = subprocess.run(['bash', str(config_update_script)],
                              cwd=SCRIPT_DIR,
                              capture_output=True, text=True,
                              timeout=30)
        
        output = f"Код выхода: {result.returncode}\n"
        if result.stdout:
            output += f"{result.stdout}\n"
        if result.stderr and result.stderr.strip():
            output += f"Ошибки:\n{result.stderr}\n"
        
        return output
        
    except subprocess.TimeoutExpired:
        return 'Таймаут: скрипт занял больше 30 секунд'
    except Exception as e:
        return f'Ошибка запуска скрипта: {e}'

def stop_server():
    """Остановить Content.Server"""
    try:
        pgrep = subprocess.run(['pgrep', '-f', 'Content.Server'],
                              capture_output=True, text=True)
        
        if pgrep.returncode != 0:
            return "Сервер не запущен"
        
        pids = pgrep.stdout.strip().split()
        
        for pid in pids:
            subprocess.run(['kill', '-SIGTERM', pid])
        
        time.sleep(2)
        
        pgrep2 = subprocess.run(['pgrep', '-f', 'Content.Server'],
                               capture_output=True, text=True)
        
        if pgrep2.returncode == 0:
            subprocess.run(['pkill', '-9', '-f', 'Content.Server'])
            return f"Сервер остановлен (PID: {', '.join(pids)}) - использован SIGKILL"
        else:
            return f"Сервер корректно остановлен (PID: {', '.join(pids)})"
            
    except Exception as e:
        return f"Ошибка остановки: {e}"

def run_git_pull():
    """Выполнить git pull"""
    try:
        result = subprocess.run(['git', 'pull'],
                              cwd=PROJECT_ROOT,
                              capture_output=True, text=True,
                              timeout=30)
        return f"{result.stdout}\n{result.stderr}"
    except subprocess.TimeoutExpired:
        return "Таймаут выполнения git pull"
    except Exception as e:
        return f"Ошибка: {e}"

def compile_release():
    """Запустить compile-release.sh"""
    try:
        if not COMPILE_SCRIPT.exists():
            return f"Ошибка: файл не найден - {COMPILE_SCRIPT}"
        
        COMPILE_SCRIPT.chmod(0o755)
        
        result = subprocess.run(['bash', str(COMPILE_SCRIPT)],
                              cwd=COMPILE_SCRIPT.parent,
                              capture_output=True, text=True,
                              timeout=300)
        
        return f"Код выхода: {result.returncode}\n{result.stdout}\n{result.stderr}"
        
    except subprocess.TimeoutExpired:
        return "Таймаут: компиляция заняла больше 5 минут"
    except Exception as e:
        return f"Ошибка компиляции: {e}"

def start_server():
    """Запустить сервер в screen сессии"""
    try:
        # Проверяем не запущен ли уже
        pgrep = subprocess.run(['pgrep', '-f', 'Content.Server'],
                              capture_output=True, text=True)
        
        if pgrep.returncode == 0:
            return "Сервер уже запущен"
        
        # Запускаем в новой screen сессии
        result = subprocess.run(
            ['screen', '-dmS', 'ss14-server', 'bash', str(RUN_SCRIPT)],
            capture_output=True,
            text=True
        )
        
        time.sleep(3)
        
        # Проверяем запустился ли
        pgrep2 = subprocess.run(['pgrep', '-f', 'Content.Server'],
                               capture_output=True, text=True)
        
        if pgrep2.returncode == 0:
            pids = pgrep2.stdout.strip().split('\n')
            return f"Сервер запущен в screen 'ss14-server'\nPID: {', '.join(pids)}"
        else:
            return "Сервер не запустился"
            
    except Exception as e:
        return f"Ошибка: {e}"

def check_status():
    """Проверить статус сервера"""
    try:
        pgrep = subprocess.run(['pgrep', '-f', 'Content.Server'],
                              capture_output=True, text=True)
        
        if pgrep.returncode != 0:
            return "Сервер не запущен"
        
        pids = pgrep.stdout.strip().split()
        
        ps_info = subprocess.run(['ps', '-p', ','.join(pids), '-o', 'pid,time,%cpu,%mem,cmd'],
                                capture_output=True, text=True)
        
        return f"""Сервер запущен
Количество процессов: {len(pids)}
PID: {', '.join(pids)}

{ps_info.stdout}"""
        
    except Exception as e:
        return f"Ошибка проверки статуса: {e}"

def run_git_command(cmd):
    """Выполнить произвольную git команду"""
    try:
        cmd_list = ['git'] + cmd.split()
        result = subprocess.run(cmd_list,
                              cwd=PROJECT_ROOT,
                              capture_output=True, text=True,
                              timeout=30)
        return f"{result.stdout}\n{result.stderr}"
    except Exception as e:
        return f"Ошибка: {e}"

# ========== ОБРАБОТКА ОШИБОК ==========

@bot.event
async def on_command_error(ctx, error):
    if isinstance(error, commands.CheckFailure):
        await ctx.send('У вас нет прав для выполнения этой команды!')
    elif isinstance(error, commands.CommandNotFound):
        pass
    else:
        await ctx.send(f'Ошибка: {error}')

# ========== ЗАПУСК ==========

if __name__ == '__main__':
    print("Запуск Discord бота для управления SS14 сервером...")
    bot.run(TOKEN)