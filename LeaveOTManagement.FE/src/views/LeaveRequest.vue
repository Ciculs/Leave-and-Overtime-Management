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
            <select v-model="form.leaveTypeId" required>
              <option value="" disabled>Select leave type</option>
              <option v-for="type in leaveTypes" :key="type.id" :value="type.id">
                {{ type.name }}
              </option>
            </select>
          </div>

          <div class="date-row">
            <div class="form-group">
  <label>Start Date</label>
  <input type="date" v-model="form.startDate" :min="today" :max="endOfYear+1" required />
</div>

<div class="form-group">
  <label>End Date</label>
  <input type="date" v-model="form.endDate" :min="form.startDate || today" :max="endOfYear+1" required />
</div>
          </div>

          <div class="form-group">
            <label>Reason</label>
            <textarea 
              v-model="form.reason" 
              rows="4" 
              placeholder="Provide a detailed reason..." 
              required
            ></textarea>
          </div>

          <div v-if="daysRequested > 0" :class="['validation-box', isBalanceError ? 'error' : 'success']">
            <span class="icon">{{ isBalanceError ? '⚠️' : '✅' }}</span>
            <div class="val-text">
              <strong>Requested Days: {{ daysRequested }}</strong>
              <p v-if="isBalanceError">Exceeds available balance ({{ currentBalance }} days).</p>
              <p v-else>Valid request length.</p>
            </div>
          </div>

          <div v-if="apiError" class="validation-box error">
            <span class="icon">❌</span>
            <div class="val-text">{{ apiError }}</div>
          </div>

          <button 
            type="submit" 
            class="btn-submit" 
            :disabled="isBalanceError || isSubmitting"
          >
            {{ isSubmitting ? 'Submitting...' : 'Submit Request' }}
          </button>
        </form>
      </div>

      <div class="balance-card">
        <h3>Your Leave Balances</h3>
        <div class="balance-list">
          <div 
            v-for="bal in balances" 
            :key="bal.leaveTypeId" 
            :class="['balance-item', { active: form.leaveTypeId === bal.leaveTypeId }]"
          >
            <div class="bal-info">
              <span class="bal-name">{{ bal.leaveTypeName }}</span>
              <span class="bal-used">Used: {{ bal.usedDays }} days</span>
            </div>
            <div class="bal-total">
              <span class="remaining">{{ bal.remainingDays }}</span>
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

// Dữ liệu giả lập tạm thời (Sprint 1 FE) - Sau này sẽ gọi API
const leaveTypes = ref([
  { id: 1, name: "Annual Leave" },
  { id: 2, name: "Sick Leave" },
  { id: 3, name: "Unpaid Leave" }
])

const balances = ref([
  { leaveTypeId: 1, leaveTypeName: "Annual Leave", totalDays: 12, usedDays: 2, remainingDays: 10 },
  { leaveTypeId: 2, leaveTypeName: "Sick Leave", totalDays: 10, usedDays: 0, remainingDays: 10 },
  { leaveTypeId: 3, leaveTypeName: "Unpaid Leave", totalDays: 0, usedDays: 5, remainingDays: 999 } // Unpaid không giới hạn
])

const today = new Date().toISOString().split('T')[0]
const currentYear = new Date().getFullYear() 
const endOfYear = `${currentYear}-12-31` 
const isSubmitting = ref(false)
const apiError = ref("")

const form = reactive({
  leaveTypeId: "",
  startDate: "",
  endDate: "",
  reason: ""
})

// Lấy số dư hiện tại của loại phép đang chọn (US8)
const currentBalance = computed(() => {
  if (!form.leaveTypeId) return 0
  const bal = balances.value.find(b => b.leaveTypeId === form.leaveTypeId)
  return bal ? bal.remainingDays : 0
})

// Tính số ngày xin nghỉ (US10)
const daysRequested = computed(() => {
  if (!form.startDate || !form.endDate) return 0
  const start = new Date(form.startDate)
  const end = new Date(form.endDate)
  const diffTime = end - start
  if (diffTime < 0) return 0
  return Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1
})

// Kiểm tra lỗi vượt số dư (US10)
const isBalanceError = computed(() => {
  if (!form.leaveTypeId) return false
  // Nếu là Unpaid Leave (id:3) thì không check số dư
  if (form.leaveTypeId === 3) return false 
  return daysRequested.value > currentBalance.value
})

const submitLeave = async () => {
  apiError.value = ""
  isSubmitting.value = true

  try {
    // Gọi API lưu xuống DB
    await api.post("/Leave", {
      leaveTypeId: form.leaveTypeId,
      fromDate: form.startDate, // Trùng tên cột FromDate trong DB
      toDate: form.endDate,     // Trùng tên cột ToDate trong DB
      totalDays: daysRequested.value,
      reason: form.reason
    })
    
    alert("Leave request submitted successfully!")
    // Reset form
    form.leaveTypeId = ""
    form.startDate = ""
    form.endDate = ""
    form.reason = ""

  } catch (error) {
    // US34: Hiển thị lỗi từ Backend nếu Backend phát hiện trùng ngày
    if (error.response && error.response.data) {
      apiError.value = error.response.data.message || "Failed to submit. Duplicate dates may exist."
    } else {
      apiError.value = "An error occurred while submitting."
    }
  } finally {
    isSubmitting.value = false
  }
}

onMounted(async () => {
  try {
    // Gọi API lấy số dư
    const balRes = await api.get("/Leave/balances")
    balances.value = balRes.data
    
    // Trích xuất LeaveTypes từ mảng balances để đổ vào Dropdown
    leaveTypes.value = balRes.data.map(b => ({
      id: b.leaveTypeId,
      name: b.leaveTypeName
    }))
  } catch (err) {
    console.error("Không thể tải dữ liệu số dư phép", err)
  }
})
</script>

<style scoped>
.leave-request-container {
  max-width: 1000px;
}

.header-section {
  margin-bottom: 30px;
}
.header-section h2 { color: #2b3674; margin-bottom: 5px; }
.header-section p { color: #707eae; font-size: 15px; }

.content-wrapper {
  display: flex;
  gap: 30px;
  align-items: flex-start;
}

/* FORM CARD */
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
.date-row .form-group { flex: 1; }

label { font-weight: 600; color: #2b3674; font-size: 14px; }

input, select, textarea {
  padding: 12px 15px;
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  font-size: 14px;
  outline: none;
  background: #f4f7fe;
  transition: 0.3s;
}

input:focus, select:focus, textarea:focus {
  border-color: #4318ff;
  background: white;
  box-shadow: 0 0 0 3px rgba(67, 24, 255, 0.1);
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
  transition: 0.3s;
  margin-top: 10px;
}
.btn-submit:hover:not(:disabled) { background: #3311cc; }
.btn-submit:disabled { background: #a3aed0; cursor: not-allowed; }

/* VALIDATION BOX */
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
.val-text p { margin: 5px 0 0 0; font-size: 13px; }

/* BALANCE CARD */
.balance-card {
  flex: 1;
  background: white;
  padding: 25px;
  border-radius: 20px;
  box-shadow: 0 10px 20px rgba(112, 144, 176, 0.05);
}
.balance-card h3 { color: #2b3674; margin-bottom: 20px; font-size: 18px; }

.balance-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 15px;
  border-radius: 12px;
  background: #f4f7fe;
  margin-bottom: 12px;
  border: 2px solid transparent;
  transition: 0.3s;
}
.balance-item.active {
  border-color: #4318ff;
  background: rgba(67, 24, 255, 0.05);
}

.bal-info { display: flex; flex-direction: column; gap: 4px; }
.bal-name { font-weight: 600; color: #2b3674; font-size: 14px; }
.bal-used { font-size: 12px; color: #707eae; }

.bal-total { text-align: right; }
.remaining { display: block; font-size: 20px; font-weight: 700; color: #4318ff; }
.lbl { font-size: 11px; color: #707eae; text-transform: uppercase; }
</style>