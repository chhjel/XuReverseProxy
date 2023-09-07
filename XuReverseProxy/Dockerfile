# docker build -f .\XuReverseProxy\Dockerfile . -t xureverseproxy-testbuild --progress=plain --no-cache

ARG BUILD_VERSION=1.0.0
ARG BUILD_CONFIG=Release
ARG BUILD_FRONTEND=true

##########################################################
# Build frontend
FROM node:16 as frontend-build
ARG BUILD_FRONTEND=true
WORKDIR /frontend

# Use BuildKit cache to persist yarn cache
COPY XuReverseProxy.Frontend/package.json XuReverseProxy.Frontend/yarn.lock ./
RUN --mount=type=cache,target=/root/.yarn if [ "$BUILD_FRONTEND" = "true" ]; then \
    YARN_CACHE_FOLDER=/root/.yarn yarn install;  \
  else \
    echo Skipping frontend restore; \
  fi

# Use BuildKit cache to persist webpack cache
COPY XuReverseProxy.Frontend/. .
RUN --mount=type=cache,target=./node_modules/.cache/webpack if [ "$BUILD_FRONTEND" = "true" ]; then \
    YARN_CACHE_FOLDER=/root/.yarn yarn build;  \
  else \
    mkdir build; \
    echo Skipping frontend build; \
  fi

##########################################################
# Build backend
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
ARG BUILD_VERSION=1.0.0
ARG BUILD_CONFIG=Release
WORKDIR /app

COPY *.sln ./
COPY XuReverseProxy/*.csproj ./XuReverseProxy/
COPY XuReverseProxy.Core/*.csproj ./XuReverseProxy.Core/
COPY XuReverseProxy.FrontendModelGeneration/*.csproj ./XuReverseProxy.FrontendModelGeneration/
RUN dotnet restore

COPY ./ ./
RUN dotnet publish "XuReverseProxy/XuReverseProxy.csproj" -c ${BUILD_CONFIG} -o out -p:Version=${BUILD_VERSION}
COPY --from=frontend-build /frontend/build/* /app/out/wwwroot/dist/

# Start backend
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build-env /app/out .
EXPOSE 80/TCP
EXPOSE 80/UDP
ENTRYPOINT ["dotnet", "XuReverseProxy.dll"]
