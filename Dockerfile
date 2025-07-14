# Get env image
FROM mcr.microsoft.com/dotnet/sdk:8.0

# Install additional packages for development
RUN apt-get update && apt-get install -y \
    git \
    curl \
    wget \
    vim \
    unzip \
    && rm -rf /var/lib/apt/lists/*

# Set working directory
WORKDIR /workspace

# Copy project files
# COPY . .

# Set common environment variables (others set in docker-compose)
ENV NUGET_XMLDOC_MODE=skip

# Create non-root user for security
RUN useradd -m -s /bin/bash devuser && \
    chown -R devuser:devuser /workspace
USER devuser

# Expose ports for HTTP and HTTPS
EXPOSE 5000 5001

# Keep container running for development
CMD ["sleep", "infinity"]