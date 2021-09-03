# Basic idea:
- Create an Azure Bot Service and an App Service.
- Talk to Bot in Teams, Bot will give you a HeroCard UI with buttons showing some music albums.
- Click a button, a Task Module (Modal Dialog) will pop up showing a webpage with Javascript inside to play musics online.
- Backend data is sent to Teams Bot UI from App Service including music streaming.

# The documents we referenced:

- Create the Azure resources: https://docs.microsoft.com/en-us/learn/modules/msteams-task-modules/7-exercise-use-task-modules-bots
- Task Module Demo (C#): https://github.com/microsoft/BotBuilder-Samples/tree/main/samples/csharp_dotnetcore/54.teams-task-module
- Javascript Music Player: https://github.com/sayantanm19/js-music-player
