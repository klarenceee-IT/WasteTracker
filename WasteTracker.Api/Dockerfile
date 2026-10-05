# Step 1: Build ASP.NET Core API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore dependencies
COPY ["WasteTracker.Api/WasteTracker.Api.csproj", "WasteTracker.Api/"]
RUN dotnet restore "WasteTracker.Api/WasteTracker.Api.csproj"

# Copy source code and publish release build
COPY WasteTracker.Api/ WasteTracker.Api/
WORKDIR "/src/WasteTracker.Api"
RUN dotnet publish "WasteTracker.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Step 2: Runtime image for Render
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose dynamic cloud port
ENV PORT=5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "WasteTracker.Api.dll"]