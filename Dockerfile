FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY BookingSystemAI.sln ./
COPY BookingSystemAI.Api/BookingSystemAI.Api.csproj BookingSystemAI.Api/
COPY BookingSystemAI.Application/BookingSystemAI.Application.csproj BookingSystemAI.Application/
COPY BookingSystemAI.Infrastructure/BookingSystemAI.Infrastructure.csproj BookingSystemAI.Infrastructure/
COPY BookingSystemAI.Domain/BookingSystemAI.Domain.csproj BookingSystemAI.Domain/

RUN dotnet restore BookingSystemAI.Api/BookingSystemAI.Api.csproj

COPY BookingSystemAI.Api/ BookingSystemAI.Api/
COPY BookingSystemAI.Application/ BookingSystemAI.Application/
COPY BookingSystemAI.Infrastructure/ BookingSystemAI.Infrastructure/
COPY BookingSystemAI.Domain/ BookingSystemAI.Domain/

RUN dotnet publish BookingSystemAI.Api/BookingSystemAI.Api.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "BookingSystemAI.Api.dll"]
