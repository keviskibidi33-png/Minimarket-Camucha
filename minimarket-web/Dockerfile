# Build stage
# Usar versión específica de Node Alpine (más pequeña y rápida de descargar)
FROM node:20-alpine AS build
WORKDIR /app

# Configurar npm para mejor manejo de red
RUN npm config set fetch-retries 5 && \
    npm config set fetch-retry-mintimeout 20000 && \
    npm config set fetch-retry-maxtimeout 120000

# Copy package files
COPY package*.json ./

# Install dependencies con reintentos automáticos
RUN npm ci --legacy-peer-deps || (sleep 10 && npm ci --legacy-peer-deps)

# Copy source code
COPY . .

# Build the app with production configuration
RUN npm run build -- --configuration=production --project=minimarket-web

# Production stage
# Usar imagen de GitHub Container Registry para evitar timeouts con Docker Hub
FROM ghcr.io/nginxinc/nginx-unprivileged:1.27-alpine AS final

# Copy built app to nginx
COPY --from=build /app/dist/minimarket-web/browser /usr/share/nginx/html

# Copy nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expose port
EXPOSE 80

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=15s --retries=3 \
  CMD wget --spider --quiet http://127.0.0.1/ || exit 1

# Start nginx
CMD ["nginx", "-g", "daemon off;"]

