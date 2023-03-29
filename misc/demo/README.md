# Demo information

## Start demo

Run `docker compose up` in the root directory of this repository.
Then open `http://localhost:6011` with your web browser.

## Certificate

The docker-compose.yml in the root directory is already set up to use `demo-cert.pfx`.

The certificate is created using `dotnet dev-certs https -ep ./demo-cert.pfx -p 1234`

:bangbang: Don't use the certificate in a production environment!
