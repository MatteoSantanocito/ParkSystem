# Utilizziamo l'immagine SDK di .NET per compilare l'applicazione
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copiamo il file di progetto e ripristiniamo le dipendenze
COPY *.csproj ./
RUN dotnet restore

# Copiamo il resto del codice e compiliamo l'applicazione
COPY . ./
RUN dotnet publish -c Release -o out

# Utilizziamo l'immagine runtime di .NET per eseguire l'applicazione
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app

# Copiamo l'applicazione compilata dall'immagine di build
COPY --from=build /app/out ./

# Espone la porta 80
EXPOSE 80

# Comando di avvio dell'applicazione
ENTRYPOINT ["dotnet", "UserInterface.dll"]