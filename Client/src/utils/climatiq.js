const CLIMATIQ_API_KEY = import.meta.env.VITE_CLIMATIQ_API_KEY;

export async function fetchCarbonImpact(category, value) {
  console.log("🌍 Calling fetchCarbonImpact");
  console.log("→ Category:", category);
  console.log("→ Value:", value, "| Type:", typeof value);

  const dataVersion = "24.24";

  const activityMap = {
    energy: {
      activity_id: "electricity-energy_source_grid_mix",
      unit_type: "energy",
      unit: "kWh",
      convert: (v) => v
    },
    water: {
      activity_id: "water-supply_treatment-distribution",
      unit_type: "volume",
      unit: "m3", // ✅ MUST BE m3
      convert: (liters) => liters / 1000 // ✅ convert L → m3
    },
    waste: {
      activity_id: "waste-disposal_landfill-mixed_municipal_solid",
      unit_type: "weight",
      unit: "kg",
      convert: (v) => v
    },
    transportation: {
      activity_id: "passenger_vehicle-vehicle_type_car-fuel_source_petrol-engine_size_medium-vehicle_age_na",
      unit_type: "distance",
      unit: "km",
      convert: (v) => v
    },
    food: {
      activity_id: "food-supply_beef-farming_method_na-region_europe",
      unit_type: "weight",
      unit: "kg",
      convert: (v) => v
    }
  };

  const config = activityMap[category];
  if (!config) throw new Error(`❌ Unsupported category: ${category}`);

  const convertedValue = config.convert(value);

  const payload = {
    emission_factor: {
      activity_id: config.activity_id,
      region: "GB",
      data_version: dataVersion
    },
    parameters: {
      [config.unit_type]: convertedValue,
      [`${config.unit_type}_unit`]: config.unit
    }
  };

  console.log("📦 Payload to Climatiq:", JSON.stringify(payload, null, 2));

  const response = await fetch("https://api.climatiq.io/estimate", {
    method: "POST",
    headers: {
      Authorization: `Bearer ${CLIMATIQ_API_KEY}`,
      "Content-Type": "application/json"
    },
    body: JSON.stringify(payload)
  });

  if (!response.ok) {
    const error = await response.json();
    console.error("❌ Climatiq API error:", error);
    throw new Error(`Climatiq error: ${error.message || "Unknown error"}`);
  }

  const result = await response.json();
  console.log("✅ Climatiq API success:", result);
  return result;
}
