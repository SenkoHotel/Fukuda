FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
COPY ./Fukuda /src/bot
COPY ./HotelLib /src/HotelLib
WORKDIR /src/bot
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish Fukuda -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS final
RUN apk add --no-cache ffmpeg
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Fukuda.dll"]
