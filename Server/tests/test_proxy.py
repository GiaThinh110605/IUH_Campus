from app import create_app


def test_one_trusted_proxy(tmp_path):
    app = create_app({"TESTING": True, "MODEL_PATH": str(tmp_path / "missing.onnx")})
    @app.get("/proxy-test")
    def proxy_test():
        from flask import request
        return {"ip": request.remote_addr, "scheme": request.scheme}
    response = app.test_client().get("/proxy-test", headers={
        "X-Forwarded-For": "192.0.2.99, 192.0.2.10", "X-Forwarded-Proto": "https"})
    assert response.json == {"ip": "192.0.2.10", "scheme": "https"}


def test_metrics_stays_available_for_internal_scraper(tmp_path):
    app = create_app({"TESTING": True, "MODEL_PATH": str(tmp_path / "missing.onnx")})
    assert app.test_client().get("/metrics").status_code == 200
