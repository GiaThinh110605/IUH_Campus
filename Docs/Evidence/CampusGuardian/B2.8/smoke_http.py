"""Run from repository root. Uses only a temporary database and localhost HTTP."""
from datetime import datetime, timedelta, timezone
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import threading
from uuid import uuid4

sys.path.insert(0, str(Path('Server').resolve()))
import requests
from werkzeug.serving import make_server
from app import create_app

with tempfile.TemporaryDirectory() as directory:
    app = create_app({'TESTING': False, 'MODEL_PATH': str(Path('Server/models/yolov8n.onnx').resolve()),
                      'DATABASE_PATH': str(Path(directory) / 'smoke.db'),
                      'ADAFRUIT_AIO_KEY': '', 'ADAFRUIT_AIO_USERNAME': ''})
    server = make_server('127.0.0.1', 0, app)
    thread = threading.Thread(target=server.serve_forever, daemon=True)
    thread.start()
    try:
        samples = []
        with requests.Session() as client:
            client.trust_env = False
            for _ in range(3):
                payload = {'session_id': str(uuid4())}
                response = client.post(f'http://127.0.0.1:{server.server_port}/scan/sessions', json=payload, timeout=10)
                assert response.status_code == 201
                assert response.json()['hint'] in [
                    'Tín hiệu nhiễu từ đồ điện tử.', 'Tín hiệu nhiễu từ thứ để ngồi.',
                    'Tín hiệu nhiễu từ đồ điện tử và thứ để ngồi.', 'Tín hiệu nhiễu từ thứ để ngồi và đồ điện tử.']
                samples.append({'request': {'method': 'POST', 'path': '/scan/sessions', 'json': payload},
                                'status': response.status_code, 'response': response.json()})
        evidence = {'base_commit': subprocess.check_output(['git', 'rev-parse', 'HEAD'], text=True).strip(),
                    'date': datetime.now(timezone(timedelta(hours=7))).isoformat(),
                    'environment': 'Windows local HTTP/Werkzeug, temporary SQLite; real Flask/ONNX loaded, no mocks, no Docker/cloud',
                    'samples': samples}
        Path(__file__).with_name('scan-sessions.json').write_text(json.dumps(evidence, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
        print(json.dumps(evidence, ensure_ascii=False, indent=2))
    finally:
        server.shutdown()
        thread.join(timeout=5)
        server.server_close()
