<script setup>
import { ref, onMounted } from "vue";
import { getHolidays, importHoliday } from "../services/holidayService";
import HolidayCalendar from "./HolidayCalendar.vue";

const holidays = ref([]);
const selectedFile = ref(null);
const loading = ref(false);
const viewMode = ref("list"); // list | calendar

const loadHolidays = async () => {
  try {
    const res = await getHolidays();
    holidays.value = res.data;
  } catch (error) {
    console.error("Load failed:", error);
  }
};

const handleImport = async () => {
  if (!selectedFile.value) {
    window.$toast("Please select a file", "warning");
    return;
  }

  try {
    loading.value = true;
    await importHoliday(selectedFile.value);
    await loadHolidays();

    window.$toast("Holiday import successful", "success");
  } catch (error) {
    console.error(error);
    window.$toast("Holiday import failed", "error");
  } finally {
    loading.value = false;
  }
};

onMounted(async () => {
  try {
    await loadHolidays()
  } catch (error) {
    console.error("Holiday load error:", error)
  }
})
</script>

<template>
  <div class="holiday-wrapper">

    <div class="card">

      <!-- HEADER -->
      <div class="card-header">

        <div>
          <h3>Holiday Management</h3>
          <p class="sub">Import and manage public holidays</p>
        </div>

        <div class="header-actions">

          <!-- VIEW TOGGLE -->
          <div class="view-toggle">
            <button :class="{ active: viewMode === 'list' }" @click="viewMode = 'list'">
              List
            </button>

            <button :class="{ active: viewMode === 'calendar' }" @click="viewMode = 'calendar'">
              Calendar
            </button>
          </div>

          <!-- IMPORT -->
          <div class="import-section">
            <label class="file-label">
              <input type="file" @change="e => selectedFile = e.target.files[0]" />
              Choose File
            </label>

            <button class="btn-import" :disabled="loading" @click="handleImport">
              {{ loading ? "Importing..." : "Import" }}
            </button>
          </div>

        </div>
      </div>

      <!-- BODY -->
      <div class="card-body">

        <!-- CALENDAR VIEW -->
        <HolidayCalendar v-if="viewMode === 'calendar'" :holidays="holidays" />

        <!-- LIST VIEW -->
        <table v-else-if="holidays.length > 0" class="holiday-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Date</th>
              <th>Name</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(h, index) in holidays" :key="h.id">
              <td>{{ index + 1 }}</td>
              <td>{{ h.holidayDate }}</td>
              <td>{{ h.name }}</td>
            </tr>
          </tbody>
        </table>

        <div v-else class="empty">
          No holidays found.
        </div>

      </div>

    </div>

  </div>
</template>

<style scoped>
.holiday-wrapper {
  margin-top: 20px;
}

/* CARD */
.card {
  background: white;
  border-radius: 20px;
  padding: 30px;
  box-shadow: 14px 17px 40px 4px rgba(112, 144, 176, 0.08);
}

/* HEADER */
.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25px;
}

.card-header h3 {
  font-size: 22px;
  color: #2b3674;
}

.sub {
  font-size: 14px;
  color: #707eae;
  margin-top: 5px;
}

.header-actions {
  display: flex;
  align-items: center;
  gap: 25px;
}

/* VIEW TOGGLE */
.view-toggle {
  display: flex;
  background: #f4f7fe;
  border-radius: 12px;
  overflow: hidden;
}

.view-toggle button {
  border: none;
  background: transparent;
  padding: 8px 18px;
  cursor: pointer;
  font-weight: 600;
  color: #707eae;
  transition: 0.3s;
}

.view-toggle button.active {
  background: linear-gradient(135deg, #4318ff 0%, #3182ce 100%);
  color: white;
}

/* IMPORT */
.import-section {
  display: flex;
  align-items: center;
  gap: 10px;
}

.file-label {
  background: #f4f7fe;
  padding: 8px 18px;
  border-radius: 12px;
  cursor: pointer;
  font-size: 14px;
  color: #2b3674;
  border: 1px solid #e2e8f0;
}

.file-label input {
  display: none;
}

.btn-import {
  background: linear-gradient(135deg, #4318ff 0%, #3182ce 100%);
  color: white;
  border: none;
  padding: 8px 22px;
  border-radius: 12px;
  cursor: pointer;
  transition: 0.3s;
  font-weight: 600;
}

.btn-import:hover {
  opacity: 0.9;
}

.btn-import:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

/* TABLE */
.holiday-table {
  width: 100%;
  border-collapse: collapse;
}

.holiday-table th {
  text-align: left;
  padding: 12px;
  font-size: 14px;
  color: #707eae;
  border-bottom: 1px solid #e2e8f0;
}

.holiday-table td {
  padding: 12px;
  border-bottom: 1px solid #f1f1f1;
  font-size: 14px;
}

.holiday-table tr:hover {
  background: #f9fbff;
}

/* EMPTY */
.empty {
  text-align: center;
  color: #a3aed0;
  padding: 30px 0;
}

/* RESPONSIVE FIX */
@media (max-width: 768px) {

  .card-header {
    flex-direction: column;
    align-items: flex-start;
    gap: 15px;
  }

  .header-actions {
    width: 100%;
    flex-direction: column;
    align-items: stretch;
    gap: 10px;
  }

  .import-section {
    flex-direction: column;
    width: 100%;
  }

  .file-label,
  .btn-import {
    width: 100%;
    text-align: center;
  }
}
</style>