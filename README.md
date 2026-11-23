[Русское описание / Russian description](#off-with-their-nuts-в-разработке)

# Off With Their Nuts! (in development)

Regular boars messing up your two-star breeding plans?  
Have the wolves guarding your base eaten all the deer and filled the woods with their pups?  
Can't get out of the house because of a herd of lox?  
**Just cut their nuts off!**

You can sterilize any tamed animal that can breed. A sterilized animal can neither produce offspring nor impregnate another animal. Sterilization is reversible by default.

Seems to work in multiplayer. I built it with multiplayer in mind and tested it in two-player co-op.

### Features

- Aim at an animal and press `U` to sterilize it or undo sterilization. You can change the key or key combination through Valheim Configuration Manager.
- A status line appears when you aim at a sterilized animal.
- You can suspend all breeding restrictions in the mod settings. The mod remembers which animals were sterilized, and the restrictions take effect again when you re-enable it.
- The mod settings also have a button to undo sterilization for every animal in the world, including those in unloaded areas. The button is available in single-player and to all admins in multiplayer.
- The mod supports English and Russian. It uses English if your game is set to any other language.

### Status

The mod is still in development.

Mod version: `v0.2.0-beta`

Tested on the Deep North Update and confirmed compatible.

Target versions:
- Valheim `1.0.21`
- BepInEx `5.4.23.5`
- Jötunn `2.30.2`

I tested the mod on Windows and on macOS with a custom Apple Silicon (ARM64) build of BepInEx.

Post bug reports and suggestions in Issues.

### Installation

Manual installation only for now.

Extract the archive and move the "OffWithTheirNuts" folder into your plugins folder:

- Windows: `C:\Program Files (x86)\Steam\steamapps\common\Valheim\BepInEx\plugins`
- macOS: `~/Library/Application Support/Steam/steamapps/common/Valheim/BepInEx/plugins`

In multiplayer, the server and every player need the mod installed.

[Download version 0.2.0-beta](https://github.com/rehallowed/Off-With-Their-Nuts/releases/download/v0.2.0-beta/OffWithTheirNuts_v0.2.0-beta.zip)

The mod requires BepInEx and Jötunn. Valheim Configuration Manager is optional.

### To do

- Add a toggle for the sterilization sound and visual effects.
- Show the current shortcut and what it does when aiming at an animal.
- Add a button for the host or single-player user to remove all mod data from the world save.
- Add an admin option to make sterilization irreversible.
- Add support for more languages.
- Publish on Thunderstore and maybe on Nexus Mods.

</details>


# Off With Their Nuts! (в разработке)

Обычные свиньи мешают размножаться двухзвёздочным?  
Охраняющие базу волки сожрали всех оленей и заполонили леса своими отпрысками?  
Толпа быкоящеров не даёт выйти из дома?   
**Почему бы вам не отрезать им яйца!**

Мод позволяет выборочно запретить размножение любому приручённому животному, которое может размножаться. Стерилизованное животное не может дать потомство или оплодотворить другую особь. По умолчанию стерилизация обратима.

Вроде как работает в мультиплеере. По крайней мере, писал я его с расчётом на мультиплеер, а потом проверил в коопе на двоих.

### Функции

- Стерилизация или её отмена делается нажатием клавиши `U` при наведении на животное. Клавишу или сочетание можно сменить в конфиге (доступен через Valheim Configuration Manager).
- При наведении на животное, если оно стерилизовано, отображается соответствующий статус.
- В настройках мода можно глобально отключить работу эффекта стерилизации. При этом статусы стерилизации животных сохраняются и продолжат действовать при включении.
- Также в настройках мода есть кнопка сброса статуса стерилизации у всех животных в мире, втч в выгруженных зонах. Кнопка доступна в одиночной игре, а также всем админам в многопользовательской игре.
- Мод поддерживает английский и русский языки. Если в игре выбран другой язык, мод будет отображаться на английском.

### Статус

Мод пока в разработке.

Проверен и совместим с Deep North Update.

Версия мода: `v0.2.0-beta`

Целевые версии:
 - Valheim `1.0.21`
 - BepInEx `5.4.23.5`
 - Jötunn `2.30.2`

Мод проверял на macOS с кастомной сборкой BepInEx под Apple Silicon (ARM64), а также на Windows.

Багрепорты и предложения кидайте в issues.

### Установка

Пока что только ручками.

Надо распаковать скачанный архив и перетащить папку "OffWithTheirNuts" в папку плагинов:
- На Windows: `C:\Program Files (x86)\Steam\steamapps\common\Valheim\BepInEx\plugins`.
- На macOS: `~/Library/Application Support/Steam/steamapps/common/Valheim/BepInEx/plugins`.

Для многопользовательской игры мод надо ставить и на сервер, и каждому игроку.

[Скачать версию 0.2.0-beta](https://github.com/rehallowed/Off-With-Their-Nuts/releases/download/v0.2.0-beta/OffWithTheirNuts_v0.2.0-beta.zip)

Для работы мода требуются BepInEx, Jötunn и опционально Valheim Configuration Manager.

### Туду

- Настройка для включения и отключения эффектов стерилизации.
- Подсказка с текущей клавишей и доступным действием при наведении на животное.
- Кнопка полной очистки данных мода из сохранения мира для хоста и одиночной игры.
- Админская опция необратимости стерилизации.
- Локализации на другие языки.
- Залить на thunderstore и мб на nexus.
