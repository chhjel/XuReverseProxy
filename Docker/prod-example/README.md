# Example server configuration

This is an example of how you could configure XuReverseProxy on a windows server.

---

## XuReverseProxy

### Running in IIS

`// ToDo`

### Running in Docker

See example (docker-compose.yaml)[docker-compose.yaml].

---

## Caddy (optional)

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
  log {
    level DEBUG
  }

  # Enable selected dns plugin
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

* Configure caddy to run as a service: `sc.exe create caddy start= auto binPath= "d:\path_to\caddy_windows_amd64_custom.exe run"`
