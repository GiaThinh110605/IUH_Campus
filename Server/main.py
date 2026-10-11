from fastapi import FastAPI, File, UploadFile, HTTPException
from fastapi.middleware.cors import CORSMiddleware
from pydantic import BaseModel
import time
import io
from PIL import Image

app = FastAPI(
    title="IUH Campus AI & Cloud IO Backend",
    description="Microservice chạy trên Docker phục vụ suy luận Deep Learning nhận diện Thẻ Sinh Viên IUH và đồng bộ Cloud cho Unity Mobile",
    version="1.0.0"
)

# Enable CORS for Unity Client & Web Dashboard
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

class HealthResponse(BaseModel):
    status: str
    uptime: float
    service: str

class PredictResponse(BaseModel):
    status: str
    label: str
    confidence: float
    department: str
    student_id_detected: bool
    inference_time_ms: float

@app.get("/health", response_model=HealthResponse)
def health_check():
    return {
        "status": "healthy",
        "uptime": time.time(),
        "service": "IUH Deep Learning & Docker Backend"
    }

@app.post("/predict/student-card", response_model=PredictResponse)
async def predict_student_card(file: UploadFile = File(...)):
    """
    Endpoint nhận ảnh từ Camera điện thoại / Unity AR và chạy mô hình Deep Learning
    để nhận diện Thẻ Sinh Viên IUH, phân loại Khoa viện.
    """
    start_time = time.time()
    try:
        contents = await file.read()
        image = Image.open(io.BytesIO(contents)).convert("RGB")
        
        # Giả lập hoặc chạy ONNX Runtime suy luận
        inference_time = (time.time() - start_time) * 1000

        # Kết quả nhận diện (sẽ nạp model ONNX thực tế khi triển khai)
        return {
            "status": "success",
            "label": "The_Sinh_Vien_IUH",
            "confidence": 0.965,
            "department": "Khoa Cong Nghe Thong Tin (FIT - IUH)",
            "student_id_detected": True,
            "inference_time_ms": round(inference_time, 2)
        }
    except Exception as e:
        raise HTTPException(status_code=400, detail=f"Lỗi xử lý ảnh: {str(e)}")

@app.get("/iot/parking-status")
def get_parking_iot_telemetry():
    """
    Endpoint mô phỏng cảm biến IoT gửi trạng thái chỗ trống bãi xe IUH
    lên Cloud thời gian thực cho Minigame của Nhật.
    """
    return {
        "zone": "Bai_Xe_Sinh_Vien_IUH",
        "total_slots": 500,
        "occupied_slots": 420,
        "available_slots": 80,
        "traffic_congestion_level": "Medium",
        "timestamp": time.time()
    }

if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app, host="0.0.0.0", port=8000)
