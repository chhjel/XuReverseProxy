FROM node:16 as frontend-build
WORKDIR /frontend

# Use BuildKit cache to persist yarn cache
COPY XuReverseProxy.Frontend/package.json XuReverseProxy.Frontend/yarn.lock ./
RUN --mount=type=cache,target=/root/.yarn YARN_CACHE_FOLDER=/root/.yarn yarn install

# Use BuildKit cache to persist webpack cache
COPY XuReverseProxy.Frontend/. .
RUN --mount=type=cache,target=./node_modules/.cache/webpack yarn build
CMD ["yarn", "watch"]

# FROM nginx:stable-alpine as dev-server

# COPY --from=frontend-build /XuReverseProxy/wwwroot/dist/* /usr/share/nginx/html
# CMD ["nginx", "-g", "daemon off;"]
