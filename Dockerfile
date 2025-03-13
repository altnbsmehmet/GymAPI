# Step 1: Use the .NET SDK image to build the project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy solution and project files
# Copy solution and project files
COPY API.sln .
COPY API.csproj .

WORKDIR /app/API

# Restore dependencies
RUN dotnet restore

# Copy remaining files and build the project
COPY . .
RUN dotnet publish -c Release -o /app/out

# Step 2: Use the ASP.NET Core Runtime image to run the app
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .

# Expose the port your app runs on
EXPOSE 5410

# Command to run the application
ENTRYPOINT ["dotnet", "API.dll"]