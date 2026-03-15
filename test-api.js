const fs = require("fs");
const path = require("path");

const BASE_URL = "http://localhost:8080/api";

async function main() {
  console.log("🚀 Starting Gateway API Test Script...\n");

  // --- Test Setup ---
  await testEndpoint("Checking Health", "GET", "/quotes/health");

  // Mock Data Payloads (3 Examples based on README)
  const mockRules = [
    {
      ruleName: "Peak_Hour_Discount_" + Date.now(),
      ruleType: "TimeWindowPromotion",
      priority: 1,
      configJson: JSON.stringify({
        Start: "00:00:00",
        End: "23:59:59",
        Discount: 0.1,
      }), // Active all day for test
      isActive: true,
      effectiveFrom: new Date().toISOString(),
    },
    {
      ruleName: "Bangkok_Surcharge_" + Date.now(),
      ruleType: "RemoteAreaSurcharge",
      priority: 2,
      // แก้ไขโครงสร้างให้ตรงกับ C# (Areas เป็น List)
      configJson: JSON.stringify({ Areas: ["Bangkok"], Surcharge: 150.0 }),
      isActive: true,
      effectiveFrom: new Date().toISOString(),
    },
    {
      ruleName: "Heavy_Weight_Tier_" + Date.now(),
      ruleType: "WeightTier",
      priority: 3,
      // แก้ไขโครงสร้างให้ตรงกับ C# (Tiers เป็น List ของช่วงน้ำหนัก)
      configJson: JSON.stringify({
        Tiers: [{ Min: 10.0, Max: 1000.0, Rate: 1.2 }],
      }),
      isActive: true,
      effectiveFrom: new Date().toISOString(),
    },
  ];

  const createdRuleIds = [];
  let createdRule = null;

  console.log("Creating mock rules...");
  for (const rule of mockRules) {
    const result = await testEndpoint(
      `1. Create Rule (${rule.ruleType})`,
      "POST",
      "/rules",
      rule,
    );
    if (result && result.id) {
      createdRuleIds.push(result.id);
      createdRule = result; // Keep the last one for update/get tests to maintain flow
    }
  }

  if (!createdRule || !createdRule.id) {
    console.error("❌ Critical: Rule creation failed. Aborting further tests.");
    return;
  }

  const ruleId = createdRule.id;

  try {
    // --- Test Execution ---
    console.log("\n--- Running Test Scenarios ---");

    // 2. ทดสอบ Get Rules และ Get by ID
    await testEndpoint("2a. Get All Rules", "GET", "/rules");
    await testEndpoint(
      `2b. Get Rule By ID (${ruleId})`,
      "GET",
      `/rules/${ruleId}`,
    );

    // 3. ทดสอบ Update Rule
    // const updatePayload = {
    //   ...createdRule,
    //   ruleName: createdRule.ruleName + "_Updated",
    //   priority: 10,
    // };
    // await testEndpoint(
    //   `3. Update Rule (${ruleId})`,
    //   "PUT",
    //   "/rules",
    //   updatePayload,
    // );

    // 4. ทดสอบคำนวณราคา (เพื่อให้กระทบกับ Rule ที่สร้าง)
    const quoteRequest = {
      weight: 15.5,
      area: "Bangkok", // พื้นที่นี้จะทำให้ Rule ทำงาน
      time: new Date().toISOString(),
      basePrice: 500.0,
    };
    await testEndpoint(
      "4. Calculate Price (should trigger the rule)",
      "POST",
      "/quotes/price",
      quoteRequest,
    );

    // 5. ทดสอบ Bulk Upload
    await testBulkUpload("5a. Bulk Upload from JSON", "test-data/test.json");
    await testBulkUpload("5b. Bulk Upload from CSV", "test-data/test.csv");
  } catch (error) {
    console.error("\n❌ An error occurred during the test sequence:", error);
  } finally {
    // --- Test Cleanup ---
    // console.log("\n--- Cleaning up test data ---");
    // for (const id of createdRuleIds) {
    //   await testEndpoint(`6. Delete Rule (${id})`, "DELETE", `/rules/${id}`);
    // }
  }

  console.log("\n✅ Test Sequence Finished.");
}

async function testEndpoint(label, method, endpoint, body = null) {
  console.log(`\n▶️  [${method}] ${label} -> ${endpoint}`);
  try {
    const headers = { "Content-Type": "application/json" };
    const options = { method, headers };
    if (body) options.body = JSON.stringify(body);

    const response = await fetch(`${BASE_URL}${endpoint}`, options);
    console.log(`   ✅ Status: ${response.status} ${response.statusText}`);

    if (response.status === 204) return null;

    const text = await response.text();
    try {
      const data = JSON.parse(text);
      const preview = JSON.stringify(data).substring(0, 100);
      console.log(
        `   📦 Response: ${preview}${preview.length >= 100 ? "..." : ""}`,
      );
      return data;
    } catch (err) {
      console.log(`   📦 Response (Text): ${text}`);
      return text;
    }
  } catch (error) {
    console.error(`   🔥 Connection Error: ${error.message}`);
    return null;
  }
}

async function testBulkUpload(label, filePath) {
  const endpoint = "/quotes/bulk";
  console.log(`\n▶️  [POST] ${label} -> ${endpoint} (File: ${filePath})`);
  try {
    const absolutePath = path.resolve(__dirname, filePath);
    if (!fs.existsSync(absolutePath)) {
      console.error(`   🔥 Error: File not found at ${absolutePath}`);
      return;
    }

    const fileContent = fs.readFileSync(absolutePath);
    const fileName = path.basename(filePath);
    const fileType = fileName.endsWith(".csv")
      ? "text/csv"
      : "application/json";

    const formData = new FormData();
    formData.append(
      "file",
      new Blob([fileContent], { type: fileType }),
      fileName,
    );

    const response = await fetch(`${BASE_URL}${endpoint}`, {
      method: "POST",
      body: formData,
    });

    console.log(`   ✅ Status: ${response.status} ${response.statusText}`);
    const text = await response.text();
    const preview = text.substring(0, 150);
    console.log(
      `   📦 Response: ${preview}${preview.length >= 150 ? "..." : ""}`,
    );
  } catch (error) {
    console.error(`   🔥 Connection Error: ${error.message}`);
  }
}

main();
