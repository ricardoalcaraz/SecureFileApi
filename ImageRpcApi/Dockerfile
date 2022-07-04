FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ImageRpcApi/ImageRpcApi.csproj", "ImageRpcApi/"]
RUN dotnet restore "ImageRpcApi/ImageRpcApi.csproj"
COPY . .
WORKDIR "/src/ImageRpcApi"
RUN dotnet build "ImageRpcApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ImageRpcApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ImageRpcApi.dll"]
