#!/usr/bin/env bash

set -euo pipefail

REPO_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$REPO_ROOT"

CERT_DIR="infrastructure/mosquitto/certificates"
CA_DIR="$CERT_DIR/ca"
SERVER_DIR="$CERT_DIR/server"
CLIENT_DIR="$CERT_DIR/client"

echo "Preparing certificate directories..."
mkdir -p -- "$CA_DIR" "$SERVER_DIR" "$CLIENT_DIR"

echo "Preparing broker certificate configuration..."
sed 's/<BROKER_LAN_IP>/127.0.0.1/' \
    "$SERVER_DIR/server.cnf.example" \
    > "$SERVER_DIR/server.cnf"

echo "Generating CA certificate..."
openssl req \
    -x509 \
    -newkey rsa:4096 \
    -sha256 \
    -days 1 \
    -nodes \
    -keyout "$CA_DIR/ca.key" \
    -out "$CA_DIR/ca.crt" \
    -config "$CA_DIR/ca.cnf" \
    -extensions v3_ca

echo "Generating broker certificate..."
openssl req \
    -new \
    -newkey rsa:2048 \
    -sha256 \
    -nodes \
    -keyout "$SERVER_DIR/server.key" \
    -out "$SERVER_DIR/server.csr" \
    -config "$SERVER_DIR/server.cnf"

openssl x509 \
    -req \
    -in "$SERVER_DIR/server.csr" \
    -CA "$CA_DIR/ca.crt" \
    -CAkey "$CA_DIR/ca.key" \
    -CAcreateserial \
    -out "$SERVER_DIR/server.crt" \
    -days 1 \
    -sha256 \
    -extfile "$SERVER_DIR/server.cnf" \
    -extensions server_cert

echo "Generating MQTT client certificate..."
openssl req \
    -new \
    -newkey rsa:2048 \
    -sha256 \
    -nodes \
    -keyout "$CLIENT_DIR/client.key" \
    -out "$CLIENT_DIR/client.csr" \
    -subj "/CN=ci-client"

openssl x509 \
    -req \
    -in "$CLIENT_DIR/client.csr" \
    -CA "$CA_DIR/ca.crt" \
    -CAkey "$CA_DIR/ca.key" \
    -CAcreateserial \
    -out "$CLIENT_DIR/client.crt" \
    -days 1 \
    -sha256 \
    -extfile "$CA_DIR/ca.cnf" \
    -extensions client_cert

echo "Setting certificate permissions..."
chmod 600 "$CA_DIR/ca.key"
chmod 644 \
    "$CA_DIR/ca.crt" \
    "$SERVER_DIR/server.crt" \
    "$SERVER_DIR/server.key" \
    "$CLIENT_DIR/client.crt" \
    "$CLIENT_DIR/client.key"

echo "CI MQTT certificates generated successfully."