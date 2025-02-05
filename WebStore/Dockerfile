# Use the official .NET SDK 7.0 image for building the application
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy project files
COPY . .

# Restore dependencies
RUN dotnet restore

# Build and publish the app
RUN dotnet publish -c Release -o out

# Use the .NET 7 runtime image to run the application
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app

# Copy built files from the previous stage
COPY --from=build /app/out .

# Set environment variable for Data Protection keys (Fixes Warning)
ENV ASPNETCORE_DATA_PROTECTION="/app/dataprotection-keys"

# Expose the correct port where your backend runs
EXPOSE 5205

# Run the application with the correct port
CMD ["dotnet", "WebStore.dll", "--urls", "http://+:5205"]
