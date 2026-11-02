FROM python:3.11-slim
WORKDIR /app
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt
COPY . .
RUN useradd --create-home --uid 10001 guardian \
    && mkdir -p /tmp/iuh-guardian-prometheus \
    && chown guardian:guardian /tmp/iuh-guardian-prometheus
ENV PYTHONUNBUFFERED=1 \
    PYTHONDONTWRITEBYTECODE=1 \
    PROMETHEUS_MULTIPROC_DIR=/tmp/iuh-guardian-prometheus \
    PORT=5001 \
    MODEL_PATH=/app/models/yolov8n.onnx
USER guardian
EXPOSE 5001
HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
    CMD curl -f "http://localhost:${PORT}/health" || exit 1
CMD ["gunicorn", "-c", "gunicorn.conf.py", "wsgi:app"]
