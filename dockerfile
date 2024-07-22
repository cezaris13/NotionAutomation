# --- Sample C# .NET App Dockerfile---

### Builder Image ###
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS sdk

# Add Any Builder Dependencies Here
# RUN apt-get update && apt-get --no-install-recommends -y install <package>

### Dev Environment ###
## To Build: docker build --no-cache --target devenv -t app-dev .
## To Run: docker run -it --rm -v $(pwd):/app app-dev
# ASSUMES Volume mounted source
# Runs as Root
FROM sdk AS devenv
# At least install vim (git is already present)
RUN apt-get update && apt-get --no-install-recommends -y install vim
# ASSUME project source is volume mounted into the container at path /app
WORKDIR /app
CMD bash

### Deployment ###
## To Build: docker build --no-cache -t app .
## To Run: docker run -it --rm app

## Builder Stage ##
FROM sdk AS builder
WORKDIR /app
# Copy everything
COPY . ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish -c Release -o out

## Deployment Image ##
# USE Runtime base image
FROM mcr.microsoft.com/dotnet/runtime:6.0

# Add a non-root user (deployer) to run the app
RUN adduser --disabled-password --gecos '' deployer
USER deployer

# Copy the source to /app
WORKDIR /app
COPY --from=builder --chown=deployer /app/out .
CMD ["dotnet", "MyDockerProj.Con.dll"]