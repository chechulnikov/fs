FROM mcr.microsoft.com/dotnet/core/sdk:2.2

WORKDIR /app

COPY ./src/vfs/. .

RUN dotnet restore
RUN dotnet test