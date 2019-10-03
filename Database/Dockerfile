FROM microsoft/mssql-server-linux:2017-latest
COPY ./CREATE_DB.sql .
ENV ACCEPT_EULA Y
ENV SA_PASSWORD @RmcDP2019!
ENV MSSQL_PID Developer
RUN sqlcmd -i CREATE_DB.sql 