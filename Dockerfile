# استخدم .NET SDK لبناء التطبيق
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# نسخ ملفات المشروع
COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

# استخدم ASP.NET Runtime لتشغيل التطبيق
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# المنفذ الذي سيستمع عليه التطبيق
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "WebApplicationFlowSync.dll"]
