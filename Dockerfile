FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
EXPOSE 8080
EXPOSE 8081

WORKDIR /src

COPY BlazorAppHuellero.csproj .
RUN dotnet restore BlazorAppHuellero.csproj


COPY . .
RUN dotnet build BlazorAppHuellero.csproj -c Release -o /app/build


FROM build AS publish
RUN dotnet publish BlazorAppHuellero.csproj -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /user/share/nginx/html

# Instalar nano
RUN apk update && apk add nano

COPY --from=publish /app/publish/wwwroot .
COPY  nginx.conf /etc/nginx/nginx.conf