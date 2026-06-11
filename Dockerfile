# STAGE 1: Build the application using .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

# Enable support for the new .slnx solution format
ENV DOTNET_FEATURES=SLNX

# Copy everything and restore dependencies
COPY . ./
RUN dotnet restore

# Build and publish a release package
RUN dotnet publish -c Release -o out

# STAGE 2: Run the application using .NET 10 Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build-env /app/out .

# Expose the port Render expects - Fix
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Double-check that your entry point matches your actual Web API .csproj output name
ENTRYPOINT ["dotnet", "CRMApi.dll"]