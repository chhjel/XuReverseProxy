# docker build -f .\XuReverseProxy\Dockerfile . -t xureverseproxy-testbuild --progress=plain --no-cache#

##########################################################
# Build frontend
FROM node:16 as frontend-build
WORKDIR /frontend

# Use BuildKit cache to persist yarn cache
COPY XuReverseProxy.Frontend/package.json XuReverseProxy.Frontend/yarn.lock ./
RUN --mount=type=cache,target=/root/.yarn YARN_CACHE_FOLDER=/root/.yarn yarn install

# Use BuildKit cache to persist webpack cache
COPY XuReverseProxy.Frontend/. .
RUN --mount=type=cache,target=./node_modules/.cache/webpack yarn build

##########################################################
# Build backend
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

COPY *.sln ./
COPY XuReverseProxy/*.csproj ./XuReverseProxy/
COPY XuReverseProxy.Core/*.csproj ./XuReverseProxy.Core/
COPY XuReverseProxy.FrontendModelGeneration/*.csproj ./XuReverseProxy.FrontendModelGeneration/
RUN dotnet restore

COPY ./ ./
RUN dotnet publish "XuReverseProxy/XuReverseProxy.csproj" -c Release -o out
COPY --from=frontend-build /frontend/build/* /app/out/wwwroot/dist/

# Start backend
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build-env /app/out .
EXPOSE 80/TCP
EXPOSE 80/UDP
ENTRYPOINT ["dotnet", "XuReverseProxy.dll"]
