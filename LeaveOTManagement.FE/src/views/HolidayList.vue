<script setup>
import { ref, onMounted, watch } from "vue"
import {
  getHolidayByYear,
  importHoliday,
  syncHolidayByYear,
  createHoliday
} from "../services/holidayService"
import HolidayCalendar from "./HolidayCalendar.vue"

const holidays = ref([])
const selectedFile = ref(null)
const loading = ref(false)
const syncing = ref(false)
const creating = ref(false)
const viewMode = ref("list")

const currentYear = new Date().getFullYear()
const selectedYear = ref(currentYear)

const years = Array.from({ length: 51 }, (_, i) => 2000 + i)

const showAddModal = ref(false)

const manualForm = ref({
  holidayDate: "",
  name: "",
  isDayOff: true,
  isLeaveBlocked: false,
  note: ""
})

const resetManualForm = () => {
  manualForm.value = {
    holidayDate: "",
    name: "",
    isDayOff: true,
    isLeaveBlocked: false,
    note: ""
  }
}

const openAddModal = (date = "") => {
  resetManualForm()
  manualForm.value.holidayDate = date || ""
  showAddModal.value = true
}

const closeAddModal = () => {
  showAddModal.value = false
}

const handleCalendarDateClick = (date) => {
  openAddModal(date)
}

const loadHolidays = async () => {
  try {
    loading.value = true
    const res = await getHolidayByYear(selectedYear.value)
    holidays.value = res.data || []
  } catch (error) {
    console.error("Load failed:", error)
    holidays.value = []
    window.$toast?.("Failed to load holidays.", "error")
  } finally {
    loading.value = false
  }
}

const handleSync = async () => {
  try {
    syncing.value = true
    await syncHolidayByYear(selectedYear.value)
    await loadHolidays()
    window.$toast?.(`Vietnam holidays synced successfully for ${selectedYear.value}.`, "success")
  } catch (error) {
    console.error(error)
    window.$toast?.(`Holiday sync failed for ${selectedYear.value}.`, "error")
  } finally {
    syncing.value = false
  }
}

const handleImport = async () => {
  if (!selectedFile.value) {
    window.$toast?.("Please select a file.", "warning")
    return
  }

  try {
    loading.value = true
    await importHoliday(selectedFile.value)
    await loadHolidays()
    selectedFile.value = null
    window.$toast?.("Holiday import successful.", "success")
  } catch (error) {
    console.error(error)
    window.$toast?.("Holiday import failed.", "error")
  } finally {
    loading.value = false
  }
}

const createManualHoliday = async () => {
  if (!manualForm.value.holidayDate) {
    window.$toast?.("Please select a date.", "warning")
    return
  }

  if (!manualForm.value.name.trim()) {
    window.$toast?.("Please enter a holiday name.", "warning")
    return
  }

  try {
    creating.value = true

    await createHoliday({
      holidayDate: manualForm.value.holidayDate,
      name: manualForm.value.name.trim(),
      isDayOff: manualForm.value.isDayOff,
      isLeaveBlocked: manualForm.value.isLeaveBlocked,
      source: "Manual",
      note: manualForm.value.note?.trim() || ""
    })

    window.$toast?.("Holiday created successfully.", "success")
    closeAddModal()
    await loadHolidays()
  } catch (error) {
    console.error(error)
    const message =
      error?.response?.data?.message ||
      error?.response?.data ||
      "Failed to create holiday."
    window.$toast?.(String(message), "error")
  } finally {
    creating.value = false
  }
}

watch(selectedYear, async () => {
  closeAddModal()
  await loadHolidays()
})

onMounted(loadHolidays)
</script>

<template>
  <div class="holiday-wrapper">
    <div class="card">
      <div class="card-header">
        <div class="header-left">
          <div class="year-filter-card">
            <span class="year-label">Holiday Management</span>
            <div class="year-select-wrap">
              <select v-model="selectedYear">
                <option v-for="y in years" :key="y" :value="y">
                  {{ y }}
                </option>
              </select>
            </div>
          </div>
        </div>

        <div class="header-right">
          <div class="toolbar-grid">
            <div class="toolbar-item view-toggle">
              <button :class="{ active: viewMode === 'list' }" @click="viewMode = 'list'">
                List
              </button>
              <button :class="{ active: viewMode === 'calendar' }" @click="viewMode = 'calendar'">
                Calendar
              </button>
            </div>

            <label class="toolbar-item file-label">
              <input type="file" @change="e => selectedFile = e.target.files[0]" />
              Choose File
            </label>

            <button class="toolbar-item btn-action" :disabled="loading" @click="handleImport">
              {{ loading ? "Importing..." : "Import" }}
            </button>

            <button class="toolbar-item btn-action" :disabled="syncing" @click="handleSync">
              {{ syncing ? "Syncing..." : "Sync Vietnam Holidays" }}
            </button>

            <button class="toolbar-item btn-action" @click="openAddModal()">
              Add Holiday
            </button>
          </div>
        </div>
      </div>

      <div class="card-body">
        <HolidayCalendar v-if="viewMode === 'calendar'" :holidays="holidays" @date-click="handleCalendarDateClick" />

        <table v-else-if="holidays.length > 0" class="holiday-table">
          <thead>
            <tr>
              <th>#</th>
              <th>Date</th>
              <th>Name</th>
              <th>Source</th>
              <th>Day Off</th>
              <th>Leave Blocked</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(h, index) in holidays" :key="h.id">
              <td>{{ index + 1 }}</td>
              <td>{{ h.holidayDate }}</td>
              <td>{{ h.name }}</td>
              <td>{{ h.source }}</td>
              <td>{{ h.isDayOff ? "Yes" : "No" }}</td>
              <td>{{ h.isLeaveBlocked ? "Yes" : "No" }}</td>
            </tr>
          </tbody>
        </table>

        <div v-else class="empty">
          No holidays found.
        </div>
      </div>
    </div>

    <div v-if="showAddModal" class="modal-overlay" @click.self="closeAddModal">
      <div class="modal-card">
        <div class="modal-header">
          <h3>Add Holiday</h3>
          <button class="modal-close" @click="closeAddModal">✕</button>
        </div>

        <div class="modal-body">
          <div class="modal-form-grid">
            <div class="form-field">
              <label>Date</label>
              <input type="date" v-model="manualForm.holidayDate" />
            </div>

            <div class="form-field">
              <label>Holiday Name</label>
              <input type="text" v-model="manualForm.name" placeholder="Enter holiday name" />
            </div>

            <div class="form-field checkbox-field">
              <label>
                <input type="checkbox" v-model="manualForm.isDayOff" />
                Company Day Off
              </label>
            </div>

            <div class="form-field checkbox-field">
              <label>
                <input type="checkbox" v-model="manualForm.isLeaveBlocked" />
                Block Leave Requests
              </label>
            </div>

            <div class="form-field full-width">
              <label>Note</label>
              <input type="text" v-model="manualForm.note" placeholder="Optional note" />
            </div>
          </div>
        </div>

        <div class="modal-footer">
          <button class="btn-cancel" @click="closeAddModal">
            Cancel
          </button>

          <button class="btn-save" :disabled="creating" @click="createManualHoliday">
            {{ creating ? "Saving..." : "Save Holiday" }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<style scoped>
.holiday-wrapper {
  margin-top: 20px;
}

.card {
  background: white;
  border-radius: 20px;
  padding: 24px;
  box-shadow: 14px 17px 40px 4px rgba(112, 144, 176, 0.08);
}

.card-header {
  display: grid;
  grid-template-columns: 280px 1fr;
  gap: 24px;
  margin-bottom: 24px;
  padding: 20px 24px;
  border-radius: 18px;
  background: #f8fbff;
  border: 1px solid #eef2f7;
}

.header-left {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.header-left h3 {
  font-size: 22px;
  color: #2b3674;
  margin: 0;
  line-height: 1.2;
}

.year-filter-card {
  background: white;
  border: 1px solid #dbe6f3;
  border-radius: 16px;
  padding: 14px 16px;
  box-shadow: 0 4px 12px rgba(112, 144, 176, 0.06);
  width: 100%;
}

.year-label {
  display: block;
  font-size: 13px;
  font-weight: 700;
  color: #000000;
  margin-bottom: 10px;
  text-transform: uppercase;
  letter-spacing: 0.4px;
}

.year-select-wrap {
  position: relative;
}

.year-select-wrap select {
  width: 100%;
  height: 44px;
  padding: 0 14px;
  border: 1px solid #dbe3f0;
  border-radius: 12px;
  outline: none;
  background: #f9fbff;
  color: #2b3674;
  font-size: 15px;
  font-weight: 600;
  cursor: pointer;
  transition: 0.2s ease;
}

.year-select-wrap select:focus {
  border-color: #4318ff;
  box-shadow: 0 0 0 3px rgba(67, 24, 255, 0.08);
  background: white;
}

.header-right {
  display: flex;
  align-items: flex-start;
}

.toolbar-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(150px, 1fr));
  gap: 14px;
  width: 100%;
}

.toolbar-item {
  height: 52px;
  min-width: 0;
}

.view-toggle {
  display: flex;
  border-radius: 14px;
  overflow: hidden;
  border: 1px solid #dbe3ff;
  background: #eef2ff;
}

.view-toggle button {
  flex: 1;
  border: none;
  background: transparent;
  cursor: pointer;
  font-weight: 700;
  color: #707eae;
  transition: 0.25s;
  font-size: 15px;
}

.view-toggle button.active {
  background: linear-gradient(135deg, #4318ff 0%, #3182ce 100%);
  color: white;
}

.file-label {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  padding: 0 18px;
  border-radius: 14px;
  cursor: pointer;
  font-size: 15px;
  color: #2b3674;
  border: 1px solid #dbe3f0;
  background: white;
  box-sizing: border-box;
  font-weight: 600;
  transition: 0.2s ease;
}

.file-label:hover {
  background: #f8fbff;
}

.file-label input {
  display: none;
}

.btn-action {
  background: linear-gradient(135deg, #4318ff 0%, #3182ce 100%);
  color: white;
  border: none;
  padding: 0 20px;
  border-radius: 14px;
  cursor: pointer;
  transition: 0.25s;
  font-weight: 700;
  font-size: 15px;
  box-sizing: border-box;
  box-shadow: 0 8px 18px rgba(67, 24, 255, 0.16);
}

.btn-action:hover {
  opacity: 0.94;
  transform: translateY(-1px);
}

.btn-action:disabled {
  opacity: 0.6;
  cursor: not-allowed;
  transform: none;
}

.card-body {
  width: 100%;
}

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

.empty {
  text-align: center;
  color: #a3aed0;
  padding: 30px 0;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
  z-index: 2000;
}

.modal-card {
  width: 640px;
  max-width: 95vw;
  background: white;
  border-radius: 18px;
  box-shadow: 0 20px 45px rgba(0, 0, 0, 0.16);
  overflow: hidden;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 20px 24px;
  border-bottom: 1px solid #e2e8f0;
}

.modal-header h3 {
  margin: 0;
  color: #2b3674;
}

.modal-close {
  border: none;
  background: transparent;
  font-size: 18px;
  cursor: pointer;
  color: #64748b;
}

.modal-body {
  padding: 24px;
}

.modal-form-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16px;
}

.form-field {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-field label {
  font-size: 14px;
  font-weight: 600;
  color: #2b3674;
}

.form-field input[type="date"],
.form-field input[type="text"] {
  padding: 10px 12px;
  border: 1px solid #dbe3f0;
  border-radius: 10px;
  outline: none;
  background: white;
}

.checkbox-field {
  justify-content: end;
}

.checkbox-field label {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 500;
}

.full-width {
  grid-column: 1 / -1;
}

.modal-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 18px 24px 24px;
}

.btn-cancel {
  padding: 10px 16px;
  border-radius: 10px;
  border: 1px solid #dbe3f0;
  background: white;
  color: #334155;
  cursor: pointer;
}

.btn-save {
  padding: 10px 18px;
  border-radius: 10px;
  border: none;
  background: linear-gradient(135deg, #4318ff 0%, #3182ce 100%);
  color: white;
  font-weight: 600;
  cursor: pointer;
}

.btn-save:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

@media (max-width: 1200px) {
  .card-header {
    grid-template-columns: 1fr;
  }

  .toolbar-grid {
    grid-template-columns: repeat(2, minmax(150px, 1fr));
  }
}

@media (max-width: 768px) {
  .card {
    padding: 18px;
  }

  .card-header {
    padding: 16px;
    gap: 18px;
  }

  .toolbar-grid {
    grid-template-columns: 1fr;
  }

  .year-filter-card {
    padding: 12px;
  }

  .modal-form-grid {
    grid-template-columns: 1fr;
  }

  .modal-footer {
    flex-direction: column-reverse;
  }

  .btn-cancel,
  .btn-save {
    width: 100%;
  }
}
</style>