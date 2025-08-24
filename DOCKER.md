# Docker Setup for Zenon Wallet API

This guide provides instructions for running the Zenon Wallet API using Docker and Docker Compose.

## Prerequisites

- Docker Engine 20.10 or later
- Docker Compose 2.0 or later
- A running Zenon node (optional, can use public nodes)
- PlasmaBot API key (optional, required for Fuse plasma mode)

## Quick Start

1. **Clone the repository and checkout the Docker branch:**
   ```bash
   git clone https://github.com/hypercore-one/znn_walletapi_csharp.git
   cd znn_walletapi_csharp
   git checkout feature/docker-support
   ```

2. **Set up environment variables:**
   ```bash
   cp .env.example .env
   ```
   Edit `.env` and configure at minimum:
   - `API_JWT_SECRET` - Generate a secure secret key
   - `API_PLASMABOT_APIKEY` - If using PlasmaBot for plasma generation

3. **Build and run the container:**
   ```bash
   docker-compose up -d
   ```

4. **Verify the API is running:**
   ```bash
   curl http://localhost:8080/api/wallet/status
   ```

5. **Access Swagger UI:**
   Open http://localhost:8080/swagger in your browser

## Configuration

### Environment Variables

All configuration is done through environment variables. See `.env.example` for a complete list with descriptions.

Key variables:
- `API_JWT_SECRET` - JWT signing key (required)
- `API_NODE_URL` - Zenon node WebSocket URL
- `API_NODE_CHAIN_ID` - Chain ID (1 for mainnet, 3 for testnet)
- `API_HTTP_PORT` - HTTP port on host (default: 8080)
- `API_HTTPS_PORT` - HTTPS port on host (default: 8443)

### Generate JWT Secret

```python
python3 -c "import secrets; import hashlib; print(hashlib.sha256(secrets.token_bytes(32)).hexdigest())"
```

### Node Configuration

For connecting to different Zenon nodes:

**Local node (on host machine):**
```
API_NODE_URL=ws://host.docker.internal:35998
```

**Testnet:**
```
API_NODE_URL=wss://syrius-testnet.zenon.community:443
API_NODE_CHAIN_ID=3
```

**Mainnet (requires your own node):**
```
API_NODE_URL=ws://your-node-url:35998
API_NODE_CHAIN_ID=1
```

## Development Setup

The repository includes a `docker-compose.override.yml` file that automatically applies development settings:

- Uses testnet by default
- Enables debug logging
- Disables auto-lock
- Exposes ports 5000 (HTTP) and 5001 (HTTPS)
- Sets JWT token expiration to 30 minutes

To run in development mode:
```bash
docker-compose up
```

To run in production mode (ignoring override):
```bash
docker-compose -f docker-compose.yml up -d
```

## API Authentication

### Create Admin User Token

1. Use the default admin credentials (change password in production):
   ```bash
   curl -X POST http://localhost:8080/api/users/authenticate \
     -H "Content-Type: application/json" \
     -d '{"username": "admin", "password": "admin"}'
   ```

2. Save the returned token and use in subsequent requests:
   ```bash
   curl -H "Authorization: Bearer YOUR_TOKEN" \
     http://localhost:8080/api/wallet/status
   ```

### Initialize Wallet

```bash
curl -X POST http://localhost:8080/api/wallet/init \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"password": "your-wallet-password"}'
```

## Integration with External Services

### Connecting from Another Docker Container

If you have a Telegram bot or other service in another Docker container that needs to connect to the Wallet API:

1. **Same docker-compose file:**
   Use the service name directly:
   ```
   http://znn-wallet-api:80
   ```

2. **Different docker-compose file:**
   Connect containers to the same network:
   ```yaml
   # In your telegram bot docker-compose.yml
   services:
     telegram-bot:
       networks:
         - zenon-network
   
   networks:
     zenon-network:
       external: true
       name: zenon-network
   ```
   Then connect using: `http://znn-wallet-api:80`

3. **From host machine:**
   ```
   http://localhost:8080
   ```

### Example: Telegram Bot Integration

```python
# Example Python code for telegram bot
import requests

API_URL = "http://znn-wallet-api:80"  # When in same network
TOKEN = "your-jwt-token"

headers = {
    "Authorization": f"Bearer {TOKEN}",
    "Content-Type": "application/json"
}

# Get wallet status
response = requests.get(f"{API_URL}/api/wallet/status", headers=headers)
```

## HTTPS Configuration

### Development Certificates

Generate development certificates:
```bash
mkdir certs
dotnet dev-certs https -ep ./certs/aspnetapp.pfx -p YourPassword
```

Update `.env`:
```
ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx
ASPNETCORE_Kestrel__Certificates__Default__Password=YourPassword
```

### Production Certificates

For production, use proper SSL certificates:

1. Place your certificate files in the `certs/` directory
2. Update the certificate path and password in `.env` or `docker-compose.yml`
3. Ensure proper file permissions

## Data Persistence

Wallet data is stored in a Docker volume named `wallet_data`. This ensures data persists across container restarts.

### Backup Wallet Data

```bash
# Create backup
docker run --rm -v znn_walletapi_csharp_wallet_data:/data \
  -v $(pwd):/backup alpine tar czf /backup/wallet-backup.tar.gz -C /data .

# Restore backup
docker run --rm -v znn_walletapi_csharp_wallet_data:/data \
  -v $(pwd):/backup alpine tar xzf /backup/wallet-backup.tar.gz -C /data
```

## Monitoring

### View Logs

```bash
# View logs
docker-compose logs -f znn-wallet-api

# View last 100 lines
docker-compose logs --tail=100 znn-wallet-api
```

### Health Check

The container includes a health check that verifies the API is responding:

```bash
docker-compose ps
```

### Resource Usage

```bash
docker stats znn-wallet-api
```

## Troubleshooting

### Container won't start

1. Check logs: `docker-compose logs znn-wallet-api`
2. Verify environment variables: `docker-compose config`
3. Ensure ports aren't already in use: `netstat -tulpn | grep 8080`

### Cannot connect to Zenon node

1. Verify node URL is correct
2. For local node, ensure it's running and accessible
3. Check firewall rules
4. Try using `host.docker.internal` for local nodes

### Authentication failures

1. Ensure JWT secret is set correctly
2. Check token expiration settings
3. Verify user credentials in configuration

### Wallet issues

1. Check wallet data volume permissions
2. Ensure wallet path is correctly configured
3. Verify wallet password is correct

## Security Considerations

### Production Deployment

1. **Change default passwords:**
   - Update user passwords in `appsettings.Docker.json`
   - Use strong wallet password

2. **Secure JWT secret:**
   - Generate a strong, unique secret
   - Never commit `.env` files with real secrets

3. **Network isolation:**
   - Use Docker networks to isolate services
   - Only expose necessary ports

4. **Use HTTPS:**
   - Configure proper SSL certificates
   - Redirect HTTP to HTTPS

5. **Regular updates:**
   - Keep Docker images updated
   - Monitor security advisories

### Environment Variables

Never commit sensitive data. Use:
- Docker secrets for production
- Environment files with proper permissions
- External secret management systems

## Advanced Configuration

### Custom User Configuration

Edit `src/ZenonWalletApi/appsettings.Docker.json` to add users:

```json
"Api": {
  "Users": [
    {
      "Id": "unique-guid-here",
      "Username": "newuser",
      "PasswordHash": "bcrypt-hash-here",
      "Roles": ["User"]
    }
  ]
}
```

Generate password hash:
```python
import bcrypt
password = "yourpassword".encode()
salt = bcrypt.gensalt(rounds=11)
hashed = bcrypt.hashpw(password, salt)
print(hashed.decode())
```

### Resource Limits

Add resource constraints in `docker-compose.yml`:

```yaml
services:
  znn-wallet-api:
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 1G
        reservations:
          cpus: '0.5'
          memory: 256M
```

## Support

For issues related to:
- Docker setup: Check this documentation
- API functionality: See main [README.md](README.md)
- Zenon Network: Visit [Zenon Community](https://forum.hypercore.one)