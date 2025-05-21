# 1. Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["VRPMS.Api/VRPMS.Api.csproj", "VRPMS.Api/"]
COPY ["VRPMS.BusinessLogic/VRPMS.BusinessLogic.csproj", "VRPMS.BusinessLogic/"]
COPY ["VRPMS.BusinessLogic.Interfaces/VRPMS.BusinessLogic.Interfaces.csproj", "VRPMS.BusinessLogic.Interfaces/"]
COPY ["VRPMS.Common/VRPMS.Common.csproj", "VRPMS.Common/"]
COPY ["VRPMS.Composition/VRPMS.Composition.csproj", "VRPMS.Composition/"]"]
COPY ["VRPMS.DataAccess/VRPMS.DataAccess.csproj", "VRPMS.DataAccess/"]
COPY ["VRPMS.DataAccess.Interfaces/VRPMS.DataAccess.Interfaces.csproj", "VRPMS.DataAccess.Interfaces/"]
COPY ["VRPMS.DataContracts/VRPMS.DataContracts.csproj", "VRPMS.DataContracts/"]"]
COPY ["VRPMS Backend.sln", "./"]

RUN dotnet restore

COPY . .
RUN dotnet publish "VRPMS.Api/VRPMS.Api.csproj" -c Release -o /app

# 2. Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app ./

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet","VRPMS.Api.dll"]
