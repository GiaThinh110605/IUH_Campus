# Panel queries

## Request rate by endpoint/status

`sum by (path, status) (rate(flask_http_request_duration_seconds_count[1m]))`

## HTTP p50/p95

`histogram_quantile(0.5, sum by (le) (rate(flask_http_request_duration_seconds_bucket[1m])))`
`histogram_quantile(0.95, sum by (le) (rate(flask_http_request_duration_seconds_bucket[1m])))`

## Inference p50/p95

`histogram_quantile(0.5, sum by (le) (rate(inference_seconds_bucket[1m])))`
`histogram_quantile(0.95, sum by (le) (rate(inference_seconds_bucket[1m])))`

## Detections by label

`sum by (label) (increase(detections_total[5m]))`

## Target found ratio

`sum(rate(predict_target_found_total[1m])) / clamp_min(sum(rate(flask_http_request_duration_seconds_count{path="/predict",status="200"}[1m])), 0.000001)`

## Gameplay by type/result

`sum by (type, result) (increase(gameplay_events_total[5m]))`

## Cloud sync by result

`sum by (result) (cloud_sync_total)`

## Outbox pending

`outbox_pending`

## Model loaded

`model_loaded`