import io
import json
from uuid import uuid4

from PIL import Image
import pytest

from app import create_app


@pytest.fixture
def pool_app(tmp_path, monkeypatch):
    monkeypatch.delenv('SCAN_INFECTED_POOL', raising=False)
    return create_app({'TESTING': True, 'MODEL_PATH': '', 'DATABASE_PATH': str(tmp_path / 'pool.db')})


def test_default_pool_and_random_sessions(pool_app):
    assert pool_app.config['SCAN_INFECTED_POOL'] == 'cell phone,laptop,chair'
    store = pool_app.extensions['scan_store']
    for _ in range(30):
        body = store.create(str(uuid4()))
        with store.store.connection() as db:
            row = db.execute('SELECT infected_classes FROM scan_sessions WHERE id=?', (body['scan_session_id'],)).fetchone()
        classes = json.loads(row['infected_classes'])
        assert 1 <= len(classes) <= 2
        assert set(classes) <= {'cell phone', 'laptop', 'chair'}
        groups = list(dict.fromkeys(store.hints[label] for label in classes))
        assert body['hint'] == 'Tín hiệu nhiễu từ ' + ' và '.join(groups) + '.'


@pytest.mark.parametrize('pool', ['', ' , ', 'laptop,unknown', 'LAPTOP'])
def test_invalid_pool_fails_startup(tmp_path, monkeypatch, pool):
    monkeypatch.setenv('SCAN_INFECTED_POOL', pool)
    with pytest.raises(ValueError, match='SCAN_INFECTED_POOL'):
        create_app({'TESTING': True, 'MODEL_PATH': '', 'DATABASE_PATH': str(tmp_path / 'bad.db')})


def test_custom_single_pool_clamps_count_and_deduplicates(pool_app):
    from app.scan_store import ScanStore
    config = dict(pool_app.config, SCAN_INFECTED_POOL=' chair, chair ', SCAN_INFECTED_MIN=2)
    store = ScanStore(pool_app.extensions['event_store'], config)
    response = store.create(str(uuid4()))
    assert response['max_monsters'] == 1
    assert response['hint'] == 'Tín hiệu nhiễu từ thứ để ngồi.'


def test_two_electronics_one_hint(pool_app):
    from app.scan_store import ScanStore
    config = dict(pool_app.config, SCAN_INFECTED_POOL='laptop,cell phone', SCAN_INFECTED_MIN=2)
    body = ScanStore(pool_app.extensions['event_store'], config).create(str(uuid4()))
    assert body['max_monsters'] == 2
    assert body['hint'] == 'Tín hiệu nhiễu từ đồ điện tử.'


def test_pool_must_also_belong_to_configured_allowed_classes(pool_app):
    from app.scan_store import ScanStore
    config = dict(pool_app.config, ALLOWED_CLASSES=('book', 'laptop'))
    with pytest.raises(ValueError, match='SCAN_INFECTED_POOL'):
        ScanStore(pool_app.extensions['event_store'], config)


def test_persisted_outside_pool_class_is_clean_after_pool_change(pool_app):
    from app.scan_store import ScanStore
    legacy = ScanStore(pool_app.extensions['event_store'],
                       dict(pool_app.config, SCAN_INFECTED_POOL='book'))
    body = legacy.create(str(uuid4()))
    result = pool_app.extensions['scan_store'].reveal(body['scan_session_id'], 'book', False)
    assert result['result'] == 'clean'
    assert result['monster_token'] is None


@pytest.mark.parametrize('label', ['book', 'bottle', 'backpack'])
def test_outside_pool_predict_still_detects_but_clean(pool_app, monkeypatch, label):
    detector = pool_app.extensions['detector']
    pool_app.extensions['model_registry'].session = object()
    def predict(image, threshold, target):
        assert target == label and threshold == .70
        return {'detections': [{'label': label, 'confidence': .95}],
                'target_found': True, 'inference_time_ms': 1}
    monkeypatch.setattr(detector, 'predict', predict)
    client = pool_app.test_client()
    session = client.post('/scan/sessions', json={'session_id': str(uuid4())})
    assert session.status_code == 201
    image = io.BytesIO()
    Image.new('RGB', (4, 4)).save(image, format='PNG')
    image.seek(0)
    response = client.post('/predict', data={'image': (image, 'frame.png'), 'target_class': label,
                                            'scan_session_id': session.json['scan_session_id']})
    assert response.status_code == 200
    assert response.json['target_found'] is True
    assert response.json['scan']['result'] == 'clean'
    assert response.json['scan']['monster_token'] is None
