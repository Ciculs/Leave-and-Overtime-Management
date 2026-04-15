<template>
  <div class="leave-request-container">
    <div class="header-section">
      <h2>Submit Leave Request</h2>
      <p>Please fill out the form below to request time off.</p>
    </div>

    <div class="content-wrapper">
      <div class="form-card">
        <form @submit.prevent="submitLeave">
          <div class="form-group">
            <label>Leave Type</label>
            <select v-model.number="form.leaveTypeId" required>
              <option value="" disabled>Select leave type</option>
              <option v-for="type in leaveTypes" :key="type.id" :value="type.id">
                {{ type.name }}
              </option>
            </select>
          </div>

          <div class="date-row">
            <div class="form-group">
              <label>Start Date</label>
              <input type="date" v-model="form.startDate" :min="today" @change="e => validateDate(e, 'start')"
                required />
            </div>

            <div class="form-group">
              <label>End Date</label>
              <input type="date" v-model="form.endDate" :min="form.startDate || today"
                @change="e => validateDate(e, 'end')" required />
            </div>
          </div>

          <div class="form-group">
            <label>Reason</label>
            <textarea v-model="form.reason" rows="4" placeholder="Provide a detailed reason..." required></textarea>
          </div>

          <div v-if="daysRequested > 0" :class="['validation-box', isBalanceError ? 'error' : 'success']">
            <span class="icon">{{ isBalanceError ? "⚠️" : "✅" }}</span>

            <div class="val-text">
              <strong>Requested Days: {{ daysRequested }}</strong>

              <p v-if="isBalanceError">
                Exceeds available balance ({{ currentBalance }} days)
              </p>

              <p v-else>
                Valid request length
              </p>
            </div>
          </div>

          <div class="action-buttons">
            <button type="button" class="btn-return" @click="router.push('/my-leaves')">
              <i class="fas fa-chevron-left me-2"></i>
              Return to List
            </button>

            <button type="submit" class="btn-submit" :disabled="isBalanceError || isSubmitting">
              {{ isSubmitting ? "Submitting..." : "Submit Request" }}
            </button>
          </div>
        </form>
      </div>

      <div class="balance-card">
        <h3>Your Leave Balances</h3>

        <div class="balance-list">
          <div v-for="bal in balances" :key="bal.leaveTypeId" :class="[
            'balance-item',
            { active: form.leaveTypeId === bal.leaveTypeId }
          ]">
            <div class="bal-info">
              <span class="bal-name">
                {{ bal.leaveTypeName }}
              </span>

              <span class="bal-used">
                Used: {{ bal.usedDays }} days
              </span>
            </div>

            <div class="bal-total">
              <span class="remaining">
                {{ bal.remainingDays }}
              </span>

              <span class="lbl">
                Left
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from "vue"
import api from "@/services/api"
import { useRouter } from "vue-router"
import { getHolidays } from "@/services/holidayService"

/* ========================
   DATA
======================== */

const leaveTypes = ref([])
const balances = ref([])
const holidays = ref([])

const today = new Date().toISOString().split("T")[0]

const isSubmitting = ref(false)
const apiError = ref("")

const form = reactive({
  leaveTypeId: null,
  startDate: "",
  endDate: "",
  reason: ""
})

const router = useRouter()

/* ========================
   COMPUTED
======================== */

const currentBalance = computed(() => {
  if (!form.leaveTypeId) return 0

  const bal = balances.value.find(
    b => b.leaveTypeId === form.leaveTypeId
  )

  return bal ? bal.remainingDays : 0
})

const daysRequested = computed(() => {
  if (!form.startDate || !form.endDate) return 0

  const start = new Date(form.startDate + "T00:00:00")
  const end = new Date(form.endDate + "T00:00:00")

  if (end < start) return 0

  let count = 0
  let current = new Date(start)

  while (current <= end) {
    const day = current.getDay()

    const y = current.getFullYear()
    const m = String(current.getMonth() + 1).padStart(2, "0")
    const d = String(current.getDate()).padStart(2, "0")
    const dateString = `${y}-${m}-${d}`

    if (day !== 0 && day !== 6 && !holidays.value.includes(dateString)) {
      count++
    }

    current.setDate(current.getDate() + 1)
  }

  return count
})

const isBalanceError = computed(() => {
  if (!form.leaveTypeId) return false

  // Unpaid Leave = unlimited
  if (form.leaveTypeId === 3) return false

  return daysRequested.value > currentBalance.value
})

/* ========================
   HOLIDAY DATA
======================== */

const fetchHolidays = async () => {
  try {
    const res = await getHolidays()
    holidays.value = (res.data || []).map(h =>
      String(h.holidayDate || h.date).split("T")[0]
    )
  } catch (err) {
    console.error("Failed to load holidays:", err)
  }
}

/* ========================
   DATE HELPERS
======================== */

const isWeekendDate = (dateStr) => {
  if (!dateStr) return false
  const date = new Date(dateStr + "T00:00:00")
  const day = date.getDay()
  return day === 0 || day === 6
}

const isHolidayDate = (dateStr) => {
  if (!dateStr) return false
  return holidays.value.includes(dateStr)
}

const hasRestrictedDateInRange = (startDate, endDate) => {
  if (!startDate || !endDate) return false

  let current = new Date(startDate + "T00:00:00")
  const end = new Date(endDate + "T00:00:00")

  while (current <= end) {
    const day = current.getDay()

    const y = current.getFullYear()
    const m = String(current.getMonth() + 1).padStart(2, "0")
    const d = String(current.getDate()).padStart(2, "0")
    const dateStr = `${y}-${m}-${d}`

    const isWeekend = day === 0 || day === 6
    const isHoliday = holidays.value.includes(dateStr)

    if (isWeekend || isHoliday) {
      return true
    }

    current.setDate(current.getDate() + 1)
  }

  return false
}

/* ========================
   DATE VALIDATION
======================== */

const validateDate = (event, field) => {
  const dateStr = event.target.value
  if (!dateStr) return

  // Check single selected date immediately
  if (isWeekendDate(dateStr)) {
    window.$toast("Weekend dates are not allowed.", "warning")

    if (field === "start") {
      form.startDate = ""
    } else {
      form.endDate = ""
    }
    return
  }

  if (isHolidayDate(dateStr)) {
    window.$toast("Public holidays are not allowed.", "warning")

    if (field === "start") {
      form.startDate = ""
    } else {
      form.endDate = ""
    }
    return
  }

  // Check date order
  if (form.startDate && form.endDate && new Date(form.endDate) < new Date(form.startDate)) {
    window.$toast("End date cannot be earlier than start date.", "warning")

    if (field === "end") {
      form.endDate = ""
    } else {
      form.startDate = ""
    }
    return
  }

  // Check full range only when both dates exist
  if (form.startDate && form.endDate && hasRestrictedDateInRange(form.startDate, form.endDate)) {
    window.$toast(
      "The selected date range contains weekends or public holidays. Please choose working days only.",
      "warning"
    )

    if (field === "end") {
      form.endDate = ""
    } else {
      form.startDate = ""
    }
  }
}

/* ========================
   SUBMIT
======================== */

const submitLeave = async () => {
  apiError.value = ""

  if (!form.leaveTypeId) {
    window.$toast("Please select a leave type.", "warning")
    return
  }

  if (!form.startDate || !form.endDate) {
    window.$toast("Please select both start date and end date.", "warning")
    return
  }

  if (new Date(form.endDate) < new Date(form.startDate)) {
    apiError.value = "End date cannot be earlier than start date."
    window.$toast(apiError.value, "warning")
    return
  }

  if (isWeekendDate(form.startDate) || isWeekendDate(form.endDate)) {
    apiError.value = "Weekend dates are not allowed."
    window.$toast(apiError.value, "warning")
    return
  }

  if (isHolidayDate(form.startDate) || isHolidayDate(form.endDate)) {
    apiError.value = "Public holidays are not allowed."
    window.$toast(apiError.value, "warning")
    return
  }

  if (hasRestrictedDateInRange(form.startDate, form.endDate)) {
    apiError.value =
      "The selected date range contains weekends or public holidays. Please choose working days only."
    window.$toast(apiError.value, "warning")
    return
  }

  if (daysRequested.value <= 0) {
    apiError.value = "The selected date range does not contain any valid working days."
    window.$toast(apiError.value, "warning")
    return
  }

  isSubmitting.value = true

  try {
    await api.post("/Leave", {
      leaveTypeId: form.leaveTypeId,
      fromDate: form.startDate,
      toDate: form.endDate,
      totalDays: daysRequested.value,
      reason: form.reason
    })

    window.$toast("Leave request submitted successfully.", "success")

    setTimeout(() => {
      router.push("/my-leaves")
    }, 800)
  } catch (error) {
    if (error.response?.data?.message) {
      apiError.value = error.response.data.message
      window.$toast(apiError.value, "error")
    } else {
      apiError.value = "An error occurred while submitting."
      window.$toast("Failed to submit leave request.", "error")
    }
  } finally {
    isSubmitting.value = false
  }
}

/* ========================
   DATA FETCH
======================== */

const fetchData = async () => {
  try {
    const balRes = await api.get("/Leave/balances")

    balances.value = balRes.data || []

    leaveTypes.value = balances.value.map(b => ({
      id: b.leaveTypeId,
      name: b.leaveTypeName
    }))
  } catch (err) {
    console.error("Cannot load balances", err)
    window.$toast("Failed to load leave balances.", "error")
  }
}

onMounted(() => {
  fetchData()
  fetchHolidays()
})
</script>

<style scoped>
.leave-request-container {
  max-width: 1000px;
}

.header-section {
  margin-bottom: 30px;
}

.header-section h2 {
  color: #2b3674;
  margin-bottom: 5px;
}

.header-section p {
  color: #707eae;
  font-size: 15px;
}

.content-wrapper {
  display: flex;
  gap: 30px;
  align-items: flex-start;
}

.form-card {
  flex: 2;
  background: white;
  padding: 30px;
  border-radius: 20px;
  box-shadow: 0 10px 20px rgba(112, 144, 176, 0.05);
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 20px;
}

.date-row {
  display: flex;
  gap: 20px;
}

.date-row .form-group {
  flex: 1;
}

label {
  font-weight: 600;
  color: #2b3674;
  font-size: 14px;
}

input,
select,
textarea {
  padding: 12px 15px;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  font-size: 14px;
  outline: none;
  background: #f4f7fe;
}

input:focus,
select:focus,
textarea:focus {
  border-color: #4318ff;
  background: white;
  box-shadow: 0 0 0 3px rgba(67, 24, 255, 0.1);
}

.btn-submit {
  flex: 1;
  padding: 14px;
  background: #4318ff;
  color: white;
  border: none;
  border-radius: 999px;
  font-weight: 700;
  font-size: 16px;
  cursor: pointer;
}

.btn-submit:disabled {
  background: #a3aed0;
  cursor: not-allowed;
}

.validation-box {
  display: flex;
  align-items: flex-start;
  gap: 12px;
  padding: 15px;
  border-radius: 12px;
  margin-bottom: 20px;
}

.validation-box.error {
  background: #fee2e2;
  color: #991b1b;
}

.validation-box.success {
  background: #dcfce7;
  color: #166534;
}

.balance-card {
  flex: 1;
  background: white;
  padding: 25px;
  border-radius: 20px;
  box-shadow: 0 10px 20px rgba(112, 144, 176, 0.05);
}

.balance-item {
  display: flex;
  justify-content: space-between;
  padding: 15px;
  border-radius: 12px;
  background: #f4f7fe;
  margin-bottom: 12px;
}

.balance-item.active {
  border: 2px solid #4318ff;
  background: rgba(67, 24, 255, 0.05);
}

.remaining {
  font-size: 20px;
  font-weight: 700;
  color: #4318ff;
}

.lbl {
  font-size: 11px;
  color: #707eae;
  text-transform: uppercase;
}

.action-buttons {
  display: flex;
  gap: 12px;
  margin-top: 10px;
}

.btn-return {
  padding: 14px 20px;
  border-radius: 999px;
  border: 2px solid #4318ff;
  background: white;
  color: #4318ff;
  font-weight: 700;
  cursor: pointer;
  transition: 0.25s;
}

.btn-return:hover {
  background: #4318ff;
  color: white;
}
</style>