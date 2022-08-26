# Backend
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY ./SheepHerding.csproj ./SheepHerding.csproj
RUN dotnet restore ./SheepHerding.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine-amd64
WORKDIR /app
RUN addgroup -S prodgroup && adduser -S prod -G prodgroup
USER prod
COPY --from=build-env /app/out .
EXPOSE 5000
ENV ASPNETCORE_URLS=http://*:5000
ENTRYPOINT ["dotnet", "SheepHerding.dll"]