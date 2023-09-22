# XuReverseProxy

XuReverseProxy is a wrapper around [YARP](https://github.com/microsoft/reverse-proxy) that provides a user interface to configure proxies with configurable authentication.

It is meant to be used for e.g. home servers so that it's only needed to expose a single set of ports to the outside world (this server), and use subdomains that forward to other services you run at home or elsewhere.

![Screenshot](/docs/proxies.png?raw=true "Screenshot")

Currently supported proxy authentication methods:

* OTP code through webhook
* Manual approval through webhook with link to included ui
* Login with username/password/TOTP
* Secret in querystring

## Setup

[Example server configuration](docs/docker-example/README.md)

## Known issues

* Project is a work in progress. It should be functional and safe to use but lacking some UX and features.
* IP block CIDR range logic does not handle all cases yet.

## Local development setup

* Database
  * Run `docker-compose up -d` to start database and db admin interface.

* Backend
  * Use your IDE of choice to modify the backend. If you only want to run the backend without modifying it, you can run `docker-compose --profile backend up -d`.
  * Entity Framework
    * Add migrations using `dotnet ef migrations add <migration_name> --project XuReverseProxy.Core -s XuReverseProxy --verbose`
  
* Frontend
  * To build frontend run `yarn watch` in `XuReverseProxy.Frontend` directory.
  * Or to build frontend from container run `docker-compose --profile frontend-watcher up -d`

* Hosts
  * Since the solution uses multiple domains, some hosts file additions should be added.
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
