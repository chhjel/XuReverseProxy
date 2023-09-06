# Example server configuration

This is an example of how you could configure XuReverseProxy on your server.

---

## 1. XuReverseProxy

### Setup

See example [docker-compose.yaml](docker-compose.yaml) for a server+db+dbadmin setup. The setup uses a self-signed certificate, use e.g. an additional reverse proxy (see Caddy below) for propper certs. Run `docker compose up -d` and you should have the server running.

On the first visit to the admin interface you must create the admin account. Enabling 2FA is highly recomended. Currently no ui exists to change password etc. Should you loose access to the account you can delete the user in the database from the db admin interface and create a new user again by visiting the admin interface.

### Auto-update (optional)

[Watchtower](https://containrrr.dev/watchtower) or other similar projects can be used to auto-update the containers.

---

## 2. Caddy (optional)

Caddy can be used as a wrapper around the proxy to provide automatic SSL certificates.

* Go to https://caddyserver.com/download, add a dns plugin for your domain registrar and download.
    * A dns plugin is needed to solve the ACME challenges required for a wildcard cert needed for the subdomains.
* Create a file named `Caddyfile` in the same folder as the downloaded .exe with contents similar to the below.
    * Replace `your-domain.com` with your domain.
    * Replace port `6666` with your exposed http port and `7777` with your exposed https port.
    * Replace port `8888` with the port XuReverseProxy is running on.
    * Update the `tls` section to reflect your selected dns plugin and api key.

```Caddyfile
# Strip some headers
(common) {
  header /* {
    -Server
    -x-powered-by
    [defer]
  }
}

# Https redirect
http://*.your-domain.com:6666, http://your-domain.com:6666 {
  redir https://{host}{uri}
}

# Forward everything targeting your domain to XuReverseProxy running locally
https://*.your-domain.com:7777, https://your-domain.com:7777 {
  # Replace this section with your dns plugin config. This one is for https://gandi.net
  tls {
    dns gandi your_api_key
  }

  # Replace https://localhost:8888 with the correct host/port to where XuReverseProxy is running.
  reverse_proxy https://localhost:8888 {
    transport http {
      # Ignore self-signed cert
      tls_insecure_skip_verify
      tls_server_name {host}
    }
  }
  import common
}

```

* Configure caddy to run as a service.
  * E.g. on windows: `sc.exe create caddy start= auto binPath= "d:\path_to\caddy_windows_amd64_custom.exe run"`

---

## 3. Configure your domain

Make sure your domain DNS records are configured to point both the root domain and wildcard subdomains to your server.
