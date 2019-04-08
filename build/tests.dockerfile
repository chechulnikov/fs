FROM mcr.microsoft.com/dotnet/core/sdk:2.2

WORKDIR /app

COPY ./src/. .

RUN dotnet restore
RUN dotnet test
