FROM mysql/mysql-server:8.0

MAINTAINER Weifen Luo | DevZest

ENV MYSQL_DATABASE=AdventureWorksLT \
    MYSQL_ALLOW_EMPTY_PASSWORD=1 \
    MYSQL_ROOT_HOST=172.17.0.1
    
ADD adventureworkslt.sql /docker-entrypoint-initdb.d/

EXPOSE 3306