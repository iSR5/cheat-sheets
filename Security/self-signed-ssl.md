# Generate Trusted Self-signed SSL Certificate

**Prerequisites:** OpenSSL

### 1. Generate Certificate Authority (CA):

1. Generate Private Key

```bash
openssl genrsa -aes256 -out ca-key.pem 4096
```

2. Generate Public Certificate

```bash
openssl req -new -x509 -sha256 -days 365 -key ca-key.pem -out ca.pem
```

### 2. Generate Self-signed Certificate:

1. Generate Private Key

```bash
openssl genrsa -out cert-key.pem 4096
```

2. Generate a Certificate Signing Request (CSR)

```bash
openssl req -new -sha256 -subj "/CN=localhost" -key cert-key.pem -out cert.csr
```

3. Generate a `extfile` with all the alternative names

```bash
echo "subjectAltName=DNS:localhost,IP:127.0.0.1" >> extfile.cnf
```

4. Generate Public Certificate (using our CA)

```bash
openssl x509 -req -sha256 -days 365 -in cert.csr -CA ca.pem -CAkey ca-key.pem -out cert.pem -extfile extfile.cnf -CAcreateserial
```

## Certificate Formats Conversions

X.509 Certificates exist in Base64 Formats **PEM (.pem, .crt, .ca-bundle)**, **PKCS#7 (.p7b, p7s)** and Binary Formats **DER (.der, .cer)**, **PKCS#12 (.pfx, p12)**.

### Convert Certs

| COMMAND                                                | CONVERSION |
| ------------------------------------------------------ | ---------- |
| `openssl x509 -outform der -in cert.pem -out cert.der` | PEM to DER |
| `openssl x509 -inform der -in cert.der -out cert.pem`  | DER to PEM |
| `openssl pkcs12 -in cert.pfx -out cert.pem -nodes`     | PFX to PEM |

## Verify Certificates

```bash
openssl verify -CAfile ca.pem -verbose cert.pem
```

## Bundling Certificates (Chaining)

Chain Order

1. Certificate
2. Intermediate
3. Root

Unix :

```bash
cat cert.pem ca-key.pem > cert-bundle.pem

```

Windows :

```bash
copy /A cert.pem+ca-key.pem cert-bundle.pem /A

```

Verify Bundle :

```bash
openssl verify cert-bundle.pem

```

## Install the CA Cert as a trusted root CA

### On Debian & Derivatives

- Move the CA certificate (`ca.pem`) into `/usr/local/share/ca-certificates/ca.crt`.

- Update the Cert Store

```bash
sudo update-ca-certificates
```

Refer the documentation [here](https://wiki.debian.org/Self-Signed_Certificate) and [here.](https://manpages.debian.org/buster/ca-certificates/update-ca-certificates.8.en.html)

### On Fedora

- Move the CA certificate (`ca.pem`) to `/etc/pki/ca-trust/source/anchors/ca.pem` or `/usr/share/pki/ca-trust-source/anchors/ca.pem`

- Now run (with sudo if necessary)

```bash
update-ca-trust
```

Refer the documentation [here.](https://docs.fedoraproject.org/en-US/quick-docs/using-shared-system-certificates/)

### On Arch

System-wide – Arch(p11-kit)
(From arch wiki)
Run (As root)

```bash
trust anchor --store myCA.crt
```

- The certificate will be written to /etc/ca-certificates/trust-source/myCA.p11-kit and the "legacy" directories automatically updated.
- If you get "no configured writable location" or a similar error, import the CA manually:
- Copy the certificate to the /etc/ca-certificates/trust-source/anchors directory.

Run (As root)

```bash
update-ca-trust
```

wiki page [here](https://wiki.archlinux.org/title/User:Grawity/Adding_a_trusted_CA_certificate)

### On Windows

Assuming the path to your generated CA certificate as `C:\ca.pem`, run:

```powershell
Import-Certificate -FilePath "C:\ca.pem" -CertStoreLocation Cert:\LocalMachine\Root
```

- Set `-CertStoreLocation` to `Cert:\CurrentUser\Root` in case you want to trust certificates only for the logged in user.

OR

In Command Prompt, run:

```sh
certutil.exe -addstore root C:\ca.pem
```

- `certutil.exe` is a built-in tool (classic `System32` one) and adds a system-wide trust anchor.

### On Android

The exact steps vary device-to-device, but here is a generalised guide:

1. Open Phone Settings
2. Locate `Encryption and Credentials` section. It is generally found under `Settings > Security > Encryption and Credentials`
3. Choose `Install a certificate`
4. Choose `CA Certificate`
5. Locate the certificate file `ca.pem` on your SD Card/Internal Storage using the file manager.
6. Select to load it.
7. Done!
