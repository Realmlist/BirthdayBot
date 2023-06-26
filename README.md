# BirthdayBot
 All other birthday bots are either barely working or ask money for stupid features. I got tired of it so I made my own.

[![Docker Image CI](https://github.com/Realmlist/BirthdayBot/actions/workflows/docker-image.yml/badge.svg)](https://github.com/Realmlist/BirthdayBot/actions/workflows/docker-image.yml)

 ### Docker Run:
 ```bash
 docker run realmlist/birthdaybot:latest `
  -e DOTNET_RUNNING_IN_CONTAINER=true `
  -e SQLSERVER=localhost `
  -e PORT=3306  `
  -e USER=dbuser `
  -e PASSWORD=password `
  -e DATABASE=dbname `
  -e TOKEN=discordbottoken
 ```

### Docker Compose:
```yaml
version: '3.3'
services:
    birthdaybot:
        environment:
            - DOTNET_RUNNING_IN_CONTAINER=true
            - SQLSERVER=<MariaDB/MySQLhost>
            - PORT=3306
            - USER=<dbuser>
            - PASSWORD=<password>
            - DATABASE=<dbname>
            - TOKEN=<discordbottoken>
        image: 'realmlist/birthdaybot:latest'
```
