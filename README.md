# Zeabix Dynamic Pricing Engine

ระบบคำนวณราคาขนส่งแบบ Dynamic Pricing ที่รองรับการจัดการกฎ (Rules) และการคำนวณราคาแบบ Real-time รวมถึงรองรับการประมวลผลข้อมูลจำนวนมาก (Bulk Processing) ผ่านระบบ Asynchronous Messaging

## 🏗 System Architecture

โปรเจกต์นี้ออกแบบด้วยสถาปัตยกรรม **Microservices** โดยมีการสื่อสารกันผ่าน **HTTP (Sync)** และ **Message Broker (Async)**

### Components

1.  **Gateway API** (`/api/gateway`)
    - จุดรับ Request เดียวจากภายนอก (Entry Point)
    - Routing ไปยัง Service ต่างๆ
    - จัดการ File Upload สำหรับ Bulk Request

2.  **Rule Management API** (`/api/rules`)
    - จัดการ CRUD ของ Pricing Rules
    - เก็บกฎต่างๆ เช่น:
      - `TimeWindowPromotion`: ส่วนลดตามช่วงเวลา
      - `RemoteAreaSurcharge`: บวกราคาเพิ่มตามพื้นที่
      - `WeightTier`: ตัวคูณราคาตามน้ำหนัก

3.  **Pricing API** (`/api/quotes`)
    - Core Logic สำหรับคำนวณราคา
    - ดึง Rule ที่ Active จาก Rule Service มาประมวลผล
    - **Background Worker**: ทำงานเป็น Consumer รับ Job จาก RabbitMQ เพื่อประมวลผล Bulk Quotes

4.  **Infrastructure**
    - **RabbitMQ**: ใช้สำหรับส่งต่อ Job การคำนวณราคาจำนวนมาก (Bulk Processing) เพื่อไม่ให้ Blocking Main Thread
      - Exchange: `pricing-exchange`
      - Queue: `pricing-queue`
      - Dead Letter Queue (DLX) & Retry Mechanism รองรับกรณีระบบล่ม

---

## 📂 Project Structure

```
zeabix-test/
├── Gateway.API/           # API Gateway Service
├── Pricing.API/           # Pricing Calculation Service & RabbitMQ Consumer
├── RuleManagement.API/    # Rule CRUD Service
├── test-api.js            # Automated Test Script (Mock Data & E2E Test)
├── test-data/             # Sample files for Bulk Upload
│   ├── test.json
│   └── test.csv
└── docker-compose.yml     # Container Orchestration
```

---

## 🚀 Getting Started

### Prerequisites

- Docker Desktop
- Node.js (สำหรับรัน Script Test)

### How to Run

1.  **Start Services** ด้วย Docker Compose:

    ```bash
    docker-compose up --build -d
    ```

    _ระบบจะทำการ Build และ Start Services ทั้งหมด (Gateway, Pricing, Rules, RabbitMQ)_

2.  **Verify Status**:
    - Gateway API: `http://localhost:8080/swagger`
    - RabbitMQ Management: `http://localhost:15672` (User/Pass: `guest`/`guest`)

---

## 🧪 Testing & Mock Data

ในโปรเจกต์มีเตรียม Script สำหรับการทำ **Automated Test** และ **Mock Data** ไว้ให้แล้ว

### 1. Automated Test Script (`test-api.js`)

สคริปต์นี้จะทำการ:

1.  เช็ค Health ของระบบ
2.  **Mock Data**: สร้าง Rule ตัวอย่าง 3 แบบ (TimeWindow, AreaSurcharge, WeightTier) เข้าสู่ระบบ
3.  ทดสอบยิง Request คำนวณราคา (Single Price) เพื่อตรวจสอบความถูกต้องของ Rule
4.  ทดสอบ **Bulk Upload** (JSON/CSV) ผ่าน Gateway
5.  (Optional) ลบข้อมูลขยะหลังทดสอบเสร็จ

**วิธีใช้งาน:**

```bash
node test-api.js
```

### 2. Bulk Insert Files

สำหรับการทดสอบ Upload ไฟล์จำนวนมาก สามารถใช้ไฟล์ตัวอย่างที่เตรียมไว้ในโฟลเดอร์ `test-data/`:

- **JSON Format** (`test-data/test.json`):

  ```json
  [
    {
      "Weight": 15.5,
      "Area": "Bangkok",
      "Time": "2026-03-15T14:00:00Z",
      "BasePrice": 500
    },
    ...
  ]
  ```

- **CSV Format** (`test-data/test.csv`):
  ```csv
  Weight,Area,Time,BasePrice
  15.5,Bangkok,2026-03-15T14:00:00Z,500
  2.0,ChiangMai,2026-03-15T09:00:00Z,300
  ```

---

> **Note:** ระบบใช้ **In-Memory Storage** สำหรับการเก็บข้อมูล Rule และ Job Status ข้อมูลจะหายไปเมื่อทำการ Restart Docker Containers
