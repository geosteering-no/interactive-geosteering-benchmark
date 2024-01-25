# Project containing the web server

To run the project, build (publish) and execute **GameServer2.csproj**

Refer to main **README** for full instructions.

## Known issue

Note, [server/GameServer2/Controllers/GeoController.cs](server/GameServer2/Controllers/GeoController.cs) contains a non-encrypted string `private const string ADMIN_SECRET_USER_NAME`. 
It is a user_name that redirects the client to the admin view. 
It is checked when accessing the following functions 
- ResetAllScores
- LevelDescription
- LoadNextUserDataFromFile

It is recommended to change them if you want to keep this information from the users. **Fixing it is an open issue**.
