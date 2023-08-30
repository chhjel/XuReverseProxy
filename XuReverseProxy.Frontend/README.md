# XuReverseProxy.Frontend

XuReverseProxy is a wrapper around [YARP](https://github.com/microsoft/reverse-proxy) that provides an interface to configure proxies with configurable authentication.

It is meant to be used for e.g. home servers so that it's only needed to expose a single port to the world (this server), and use subdomains that forward to other services you run at home or elsewhere.

Currently supported proxy authentication methods:

* OTP code through webhook
* Manual approval through webhook with link to included ui
* Login with username/password/TOTP
* Secret in querystring

## Configuration

Configuration is divided into two parts, the static and dynamic config. The static config resides in `appsettings.json` while the dynamic one can be changed at runtime from the admin interface. In `appsettings.json`/`docker-compose.yaml` you must configure e.g. desired domains.

## Local development setup

* Database
  * Run `docker-compose up -d` to start database and backend. Backend can be stopped unless you don't plan on making changes.
  * Optionally run `docker-compose up -d --build adminer` to start the db admin interface.

* Backend
  * Backend is not currently developed in a container, use any IDE.
  * Entity Framework
    * Add migrations using `dotnet ef migrations add <migration_name> --project XuReverseProxy.Core -s XuReverseProxy --verbose`
  
* Frontend
  * To build frontend outside container, run `yarn watch` in `XuReverseProxy.Frontend` directory.
  * To build frontend in container run `docker-compose up -d --build frontend-watcher`

* Hosts
  * Since the solution uses multiple domains, some hosts file additions are convenient.
    * Path: `C:\Windows\System32\drivers\etc\hosts`
    * Example:

        ```hosts
        127.0.0.1 localdev.test
        127.0.0.1 test1.localdev.test
        127.0.0.1 test2.localdev.test
        127.0.0.1 test3.localdev.test
        127.0.0.1 test4.localdev.test
        127.0.0.1 admin.localdev.test
        ```
