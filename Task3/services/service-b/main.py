import time
import random
from fastapi import FastAPI

app = FastAPI()

@app.get("/health")
def health():
    return {"status": "ok"}

@app.post("/calculations")
def calculate():
    time.sleep(random.uniform(0.05, 0.3))
    return {"calculation_id": random.randint(1, 1000), "status": "PRICE_CALCULATED"}