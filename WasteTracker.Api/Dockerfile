# Step 1: Build ASP.NET Core API
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file inside API directory
COPY ["WasteTracker.Api.csproj", "./"]
RUN dotnet restore "WasteTracker.Api.csproj"

# Copy remaining source code
COPY . .
RUN dotnet publish "WasteTracker.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Step 2: Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV PORT=5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "WasteTracker.Api.dll"]