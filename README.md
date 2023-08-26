# leaguetracker
A simple dedicated playtime tracker for League of Legends

League of Legends doesn't track your playtime which is simply infuriating. I have tried multiple clients to track the game's playtime, but over time they always seem to break.

Clients I've tried:
- LaunchBox (playtime tracker seems to be able to track when the game is opened and closed but the playtime counter is always inaccurate)
- GOG Galaxy Client (Does not recognize when the league of legends client closes. I suspect it's due to child processes triggered from the Riot client window)

This simple console app tracks the parent app LeagueOfLegends.exe and all child processes created by the app. Once all processes have closed, the consoleapp will automatically log the playtime in seconds, the start date/time and the end date/time.

## Roadmap
- I plan on expanding this to work with any game client autonomously by adding an add-game option.
- The app is a simple console app currently as I'm focusing on functionality. a window form app could be desirable in the future.
- This app is currently hardcoded to league of legends.
