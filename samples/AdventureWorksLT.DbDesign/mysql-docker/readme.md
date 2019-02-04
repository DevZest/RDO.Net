# AdventureworksLT MySQL Database Image

This repo build MySQL database image of AdventureworksLT sample database, for development and testing purpose.

- To build Docker image `adventureworkslt`:

```cmd
.\build.bat
```

- To start Docker container `adventureworkslt`:

```cmd
.\start.bat
```

After docker container `adventureworkslt` started, client can connect to MySQL server localhost:3306, using user name `root` and empty password, to use database `AdventureWorksLT`.

- To stop Docker container `adventureworkslt`:

```cmd
.\stop.bat
```
