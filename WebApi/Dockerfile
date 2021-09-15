#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ../Cert .
COPY ../Library .
COPY ../WebApi .
RUN dotnet restore "WebApi/ApiDemo.NET5.csproj"
WORKDIR "/src/WebApi"
RUN dotnet build "ApiDemo.NET5.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ApiDemo.NET5.csproj" -c Release -o /app/publish
RUN echo "1.0.0" > /app/publish/wwwroot/file/version.txt

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ApiDemo.NET5.dll"]

# docker build -t WebApi/ApiDemo.NET5:dev .