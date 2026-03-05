<template>
  <div class="leave-request-container">
    <div class="header-section">
      <h2>Submit Leave Request</h2>
      <p>Please fill out the form below to request time off.</p>
    </div>

    <div class="content-wrapper">
      <!-- FORM -->
      <div class="form-card">
        <form @submit.prevent="submitLeave">
          
          <!-- Leave Type -->
          <div class="form-group">
            <label>Leave Type</label>
            <select v-model.number="form.leaveTypeId" required>
              <option value="" disabled>Select leave type</option>
              <option
                v-for="type in leaveTypes"
                :key="type.id"
                :value="type.id"
              >
                {{ type.name }}
              </option>
            </select>
          </div>

          <!-- Dates -->
          <div class="date-row">
            <div class="form-group">
              <label>Start Date</label>
              <input
                type="date"
                v-model="form.startDate"
                :min="today"
                :max="endOfYear"
                required
              />
            </div>

            <div class="form-group">
              <label>End Date</label>
              <input
                type="date"
                v-model="form.endDate"
                :min="form.startDate || today"
                :max="endOfYear"
                required
              />
            </div>
          </div>

          <!-- Reason -->
          <div class="form-group">
            <label>Reason</label>
            <textarea
              v-model="form.reason"
              rows="4"
              placeholder="Provide a detailed reason..."
              required
            ></textarea>
          </div>

          <!-- Validation Box -->
          <div
            v-if="daysRequested > 0"
            :class="['validation-box', isBalanceError ? 'error' : 'success']"
          >
            <span class="icon">{{ isBalanceError ? "⚠️" : "✅" }}</span>
            <div class="val-text">
              <strong>Requested Days: {{ daysRequested }}</strong>
              <p v-if="isBalanceError">
                Exceeds available balance ({{ currentBalance }} days).
              </p>
              <p v-else>Valid request length.</p>
            </div>
          </div>

          <!-- API Error -->
          <div v-if="apiError" class="validation-box error">
            <span class="icon">❌</span>
            <div class="val-text">{{ apiError }}</div>
          </div>

          <!-- Submit -->
          <button
            type="submit"
            class="btn-submit"
            :disabled="isBalanceError || isSubmitting"
          >
            {{ isSubmitting ? "Submitting..." : "Submit Request" }}
          </button>
        </form>
      </div>

      <!-- BALANCE CARD -->
      <div class="balance-card">
        <h3>Your Leave Balances</h3>
        <div class="balance-list">
          <div
            v-for="bal in balances"
            :key="bal.leaveTypeId"
            :class="[
              'balance-item',
              { active: form.leaveTypeId === bal.leaveTypeId }
            ]"
          >
            <div class="bal-info">
              <span class="bal-name">{{ bal.leaveTypeName }}</span>
              <span class="bal-used">
                Used: {{ bal.usedDays }} days
              </span>
            </div>
            <div class="bal-total">
              <span class="remaining">
                {{ bal.remainingDays }}
              </span>
              <span class="lbl">Left</span>
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

const leaveTypes = ref([])
const balances = ref([])

const today = new Date().toISOString().split("T")[0]
const currentYear = new Date().getFullYear()
const endOfYear = `${currentYear}-12-31`

const isSubmitting = ref(false)
const apiError = ref("")

const form = reactive({
  leaveTypeId: null,
  startDate: "",
  endDate: "",
  reason: ""
})

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

  const diffTime = end - start
  return Math.floor(diffTime / (1000 * 60 * 60 * 24)) + 1
})

const isBalanceError = computed(() => {
  if (!form.leaveTypeId) return false

  // ID = 3 là Unpaid Leave (không giới hạn)
  if (form.leaveTypeId === 3) return false

  return daysRequested.value > currentBalance.value
})

/* ========================
   SUBMIT
======================== */

const submitLeave = async () => {
  apiError.value = ""

  if (new Date(form.endDate) < new Date(form.startDate)) {
    apiError.value = "End date cannot be before start date."
    return
  }

  isSubmitting.value = true

  try {
    await api.post("/Leave", {
      leaveTypeId: form.leaveTypeId,
      fromDate: form.startDate,
      toDate: form.endDate,
      reason: form.reason
    })

    alert("Leave request submitted successfully!")

    resetForm()
    await fetchData()

  } catch (error) {
    if (error.response?.data?.message) {
      apiError.value = error.response.data.message
    } else {
      apiError.value = "An error occurred while submitting."
    }
  } finally {
    isSubmitting.value = false
  }
}

/* ========================
   HELPERS
======================== */

const resetForm = () => {
  form.leaveTypeId = null
  form.startDate = ""
  form.endDate = ""
  form.reason = ""
}

const fetchData = async () => {
  try {
    const balRes = await api.get("/Leave/balances")
    balances.value = balRes.data

    leaveTypes.value = balRes.data.map(b => ({
      id: b.leaveTypeId,
      name: b.leaveTypeName
    }))
  } catch (err) {
    console.error("Cannot load balances", err)
  }
}

onMounted(() => {
  fetchData()
})
</script>

<style scoped>
.leave-request-container { max-width: 1000px; }
.header-section { margin-bottom: 30px; }
.header-section h2 { color: #2b3674; margin-bottom: 5px; }
.header-section p { color: #707eae; font-size: 15px; }
.content-wrapper { display: flex; gap: 30px; align-items: flex-start; }
.form-card { flex: 2; background: white; padding: 30px; border-radius: 20px; box-shadow: 0 10px 20px rgba(112,144,176,0.05); }
.form-group { display: flex; flex-direction: column; gap: 8px; margin-bottom: 20px; }
.date-row { display: flex; gap: 20px; }
.date-row .form-group { flex: 1; }
label { font-weight: 600; color: #2b3674; font-size: 14px; }
input, select, textarea {
  padding: 12px 15px;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  font-size: 14px;
  outline: none;
  background: #f4f7fe;
}
input:focus, select:focus, textarea:focus {
  border-color: #4318ff;
  background: white;
  box-shadow: 0 0 0 3px rgba(67,24,255,0.1);
}
.btn-submit {
  width: 100%;
  padding: 14px;
  background: #4318ff;
  color: white;
  border: none;
  border-radius: 12px;
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
.validation-box.error { background: #fee2e2; color: #991b1b; }
.validation-box.success { background: #dcfce7; color: #166534; }
.balance-card {
  flex: 1;
  background: white;
  padding: 25px;
  border-radius: 20px;
  box-shadow: 0 10px 20px rgba(112,144,176,0.05);
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
  background: rgba(67,24,255,0.05);
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
</style>