<template>
  <div class="leave-container">
    <div class="page-header">
      <h2>HR Final Leave Approvals</h2>

      <div class="header-actions">
        <input
          v-model="search"
          type="text"
          class="search-input"
          placeholder="Search employee or reason..."
        />

        <select v-model="statusFilter" class="filter-select">
          <option value="All">All</option>
          <option value="Pending HR">Pending HR</option>
          <option value="Approved">Approved</option>
          <option value="Rejected">Rejected</option>
        </select>

        <select v-model="sortBy" class="filter-select">
          <option value="Newest">Newest</option>
          <option value="Oldest">Oldest</option>
        </select>

        <button class="refresh-btn" @click="loadRequests" :disabled="loading">
          {{ loading ? "Loading..." : "Refresh" }}
        </button>
      </div>
    </div>

    <div class="stats-row">
      <div class="stat-box">
        <div class="stat-number">{{ requests.length }}</div>
        <div class="stat-label">Total</div>
      </div>

      <div class="stat-box">
        <div class="stat-number pending-hr-text">{{ countByStatus("Pending HR") }}</div>
        <div class="stat-label">Pending HR</div>
      </div>

      <div class="stat-box">
        <div class="stat-number approved-text">{{ countByStatus("Approved") }}</div>
        <div class="stat-label">Approved</div>
      </div>

      <div class="stat-box">
        <div class="stat-number rejected-text">{{ countByStatus("Rejected") }}</div>
        <div class="stat-label">Rejected</div>
      </div>
    </div>

    <div v-if="loading" class="empty-state">
      Loading HR approvals...
    </div>

    <div v-else-if="pagedRequests.length === 0" class="empty-state">
      No leave requests found.
    </div>

    <div v-else class="leave-list">
      <div
        v-for="leave in pagedRequests"
        :key="leave.id"
        class="leave-card"
        @click="openDetail(leave)"
      >
        <div class="leave-header">
          <span class="leave-type">
            {{ leave.leaveType || "Leave Request" }}
          </span>

          <span :class="['status', statusClass(leave.status)]">
            {{ normalizeStatus(leave.status) }}
          </span>
        </div>

        <div class="employee-name">
          {{ getEmployeeName(leave) }}
        </div>

        <div class="leave-date">
          📅 {{ formatDate(leave.startDate || leave.fromDate) }}
          →
          {{ formatDate(leave.endDate || leave.toDate) }}
        </div>

        <div class="leave-days">
          {{ leave.totalDays ?? calculateDays(leave.startDate || leave.fromDate, leave.endDate || leave.toDate) }}
          day(s)
        </div>

        <p class="leave-reason">
          {{ leave.reason || "No reason provided" }}
        </p>

        <div v-if="leave.managerComment" class="comment-preview">
          Manager: {{ leave.managerComment }}
        </div>

        <div v-if="leave.hrComment" class="comment-preview">
          HR: {{ leave.hrComment }}
        </div>

        <div class="view-detail">
          Tap to view detail →
        </div>
      </div>
    </div>

    <div v-if="totalPages > 1" class="pagination">
      <button class="page-btn" :disabled="currentPage === 1" @click="currentPage--">
        Prev
      </button>

      <button
        v-for="page in totalPages"
        :key="page"
        :class="['page-number', { active: currentPage === page }]"
        @click="currentPage = page"
      >
        {{ page }}
      </button>

      <button class="page-btn" :disabled="currentPage === totalPages" @click="currentPage++">
        Next
      </button>
    </div>

    <div v-if="selectedLeave" class="modal-overlay" @click.self="selectedLeave = null">
      <div class="modal-card">
        <h3>Leave Request Detail</h3>

        <p><strong>Employee:</strong> {{ getEmployeeName(selectedLeave) }}</p>
        <p><strong>Type:</strong> {{ selectedLeave.leaveType || "Leave Request" }}</p>
        <p><strong>From:</strong> {{ formatDate(selectedLeave.startDate || selectedLeave.fromDate) }}</p>
        <p><strong>To:</strong> {{ formatDate(selectedLeave.endDate || selectedLeave.toDate) }}</p>
        <p>
          <strong>Total Days:</strong>
          {{
            selectedLeave.totalDays ?? calculateDays(
              selectedLeave.startDate || selectedLeave.fromDate,
              selectedLeave.endDate || selectedLeave.toDate
            )
          }}
        </p>
        <p><strong>Status:</strong> {{ normalizeStatus(selectedLeave.status) }}</p>

        <div v-if="selectedLeave.managerComment" class="reason-section">
          <strong>Manager Comment:</strong>
          <div class="reason-box">
            {{ selectedLeave.managerComment }}
          </div>
        </div>

        <div v-if="selectedLeave.hrComment" class="reason-section">
          <strong>HR Comment:</strong>
          <div class="reason-box">
            {{ selectedLeave.hrComment }}
          </div>
        </div>

        <div class="reason-section">
          <strong>Reason:</strong>
          <div class="reason-box">
            {{ selectedLeave.reason || "No reason provided" }}
          </div>
        </div>

        <div v-if="isPendingHR(selectedLeave.status)" class="reason-section">
          <strong>Reject Reason:</strong>
          <textarea
            v-model="rejectReason"
            class="reject-textarea"
            rows="4"
            placeholder="Enter reject reason..."
          ></textarea>
        </div>

        <div class="modal-footer">
          <button class="close-btn" @click="selectedLeave = null">
            Close
          </button>

          <div v-if="isPendingHR(selectedLeave.status)" class="action-row">
            <button
              class="reject-btn"
              :disabled="submittingId === selectedLeave.id"
              @click.stop="reject(selectedLeave)"
            >
              {{ submittingId === selectedLeave.id ? "Processing..." : "Reject" }}
            </button>

            <button
              class="approve-btn"
              :disabled="submittingId === selectedLeave.id"
              @click.stop="approve(selectedLeave)"
            >
              {{ submittingId === selectedLeave.id ? "Processing..." : "Final Approve" }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted } from "vue"
import api from "@/services/api"

const requests = ref([])
const selectedLeave = ref(null)
const loading = ref(false)
const submittingId = ref(null)

const search = ref("")
const statusFilter = ref("All")
const sortBy = ref("Newest")

const currentPage = ref(1)
const pageSize = 6

const rejectReason = ref("")

const normalizeStatus = (status) => {
  if (!status) return "Pending HR"

  const s = String(status).trim().toLowerCase()

  if (s === "approved") return "Approved"
  if (s === "rejected") return "Rejected"
  if (s === "pending hr" || s === "pendinghr") return "Pending HR"
  if (s.includes("approved")) return "Approved"
  if (s.includes("rejected")) return "Rejected"
  if (s.includes("pending hr")) return "Pending HR"
  if (s.includes("pending")) return "Pending HR"

  return status
}

const normalizeItem = (item) => {
  return {
    ...item,
    status: normalizeStatus(item.status),
    managerComment: item.managerComment || item.commentManager || "",
    hrComment: item.hrComment || item.commentHR || ""
  }
}

const loadRequests = async () => {
  loading.value = true
  try {
    const res = await api.get("/Leave/pending-hr")
    requests.value = Array.isArray(res.data)
      ? res.data.map(normalizeItem)
      : []
  } catch (error) {
    console.error("Load HR approvals failed", error)
    requests.value = []
  } finally {
    loading.value = false
  }
}

const getEmployeeName = (item) => {
  return (
    item.employeeName ||
    item.fullName ||
    item.userName ||
    item.name ||
    item.employee?.fullName ||
    item.employee?.name ||
    "Unknown Employee"
  )
}

const filteredRequests = computed(() => {
  let data = [...requests.value]

  const keyword = search.value.trim().toLowerCase()
  if (keyword) {
    data = data.filter(item => {
      const employeeName = getEmployeeName(item).toLowerCase()
      const reason = String(item.reason || "").toLowerCase()
      const leaveType = String(item.leaveType || "").toLowerCase()
      const status = normalizeStatus(item.status).toLowerCase()

      return (
        employeeName.includes(keyword) ||
        reason.includes(keyword) ||
        leaveType.includes(keyword) ||
        status.includes(keyword)
      )
    })
  }

  if (statusFilter.value !== "All") {
    data = data.filter(item => normalizeStatus(item.status) === statusFilter.value)
  }

  data.sort((a, b) => {
    const dateA = new Date(a.createdAt || a.startDate || a.fromDate || 0).getTime()
    const dateB = new Date(b.createdAt || b.startDate || b.fromDate || 0).getTime()
    return sortBy.value === "Newest" ? dateB - dateA : dateA - dateB
  })

  return data
})

const totalPages = computed(() => {
  return Math.max(1, Math.ceil(filteredRequests.value.length / pageSize))
})

const pagedRequests = computed(() => {
  const start = (currentPage.value - 1) * pageSize
  return filteredRequests.value.slice(start, start + pageSize)
})

watch([search, statusFilter, sortBy], () => {
  currentPage.value = 1
})

watch(totalPages, (value) => {
  if (currentPage.value > value) currentPage.value = value
})

const countByStatus = (status) => {
  return requests.value.filter(item => normalizeStatus(item.status) === status).length
}

const isPendingHR = (status) => normalizeStatus(status) === "Pending HR"

const statusClass = (status) => {
  const s = normalizeStatus(status)
  if (s === "Approved") return "approved"
  if (s === "Rejected") return "rejected"
  return "pending-hr"
}

const openDetail = (leave) => {
  selectedLeave.value = { ...leave }
  rejectReason.value = ""
}

const approve = async (leave) => {
  submittingId.value = leave.id
  try {
    await api.put(`/Leave/hr-approve/${leave.id}`)
    showToast("Final approval successful", "success")

    selectedLeave.value = null
    rejectReason.value = ""
    await loadRequests()
  } catch (error) {
    console.error("HR approve failed", error)
    console.log(error?.response?.data)
    showToast(error?.response?.data?.message || "Approve failed", "error")
  } finally {
    submittingId.value = null
  }
}

const reject = async (leave) => {
  submittingId.value = leave.id

  if (!rejectReason.value || !rejectReason.value.trim()) {
    showToast("Please enter reject reason", "error")
    submittingId.value = null
    return
  }

  try {
    await api.put(`/Leave/hr-reject/${leave.id}`, {
      reason: rejectReason.value.trim()
    })

    showToast("Rejected successfully", "success")

    selectedLeave.value = null
    rejectReason.value = ""
    await loadRequests()
  } catch (error) {
    console.error("HR reject failed", error)
    console.log(error?.response?.data)
    showToast(error?.response?.data?.message || "Reject failed", "error")
  } finally {
    submittingId.value = null
  }
}

const formatDate = (date) => {
  if (!date) return "--"
  return new Date(date).toLocaleDateString("vi-VN")
}

const calculateDays = (fromDate, toDate) => {
  if (!fromDate || !toDate) return 0
  const start = new Date(fromDate)
  const end = new Date(toDate)
  const diff = Math.ceil((end - start) / (1000 * 60 * 60 * 24)) + 1
  return diff > 0 ? diff : 0
}

const showToast = (message, type = "info") => {
  if (window.$toast) {
    window.$toast(message, type)
  } else {
    alert(message)
  }
}

onMounted(() => {
  loadRequests()
})
</script>

<style scoped>
.leave-container {
  background: white;
  padding: 30px;
  border-radius: 16px;
}

.page-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 16px;
  margin-bottom: 20px;
}

.header-actions {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
  justify-content: flex-end;
}

.search-input,
.filter-select {
  height: 40px;
  border: 1px solid #ddd;
  border-radius: 10px;
  padding: 0 12px;
  outline: none;
  background: white;
}

.search-input {
  min-width: 240px;
}

.refresh-btn {
  background: linear-gradient(135deg, #6a5cff, #4318ff);
  color: white;
  border: none;
  padding: 10px 16px;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
}

.refresh-btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

h2 {
  margin: 0;
}

.stats-row {
  display: grid;
  grid-template-columns: repeat(4, minmax(110px, 1fr));
  gap: 14px;
  margin-bottom: 22px;
}

.stat-box {
  background: #f9fafc;
  border: 1px solid #eee;
  border-radius: 14px;
  padding: 14px;
}

.stat-number {
  font-size: 24px;
  font-weight: 700;
  color: #2b3674;
}

.stat-label {
  margin-top: 4px;
  font-size: 13px;
  color: #666;
}

.approved-text {
  color: #1a9b5c;
}

.rejected-text {
  color: #dc2626;
}

.pending-hr-text {
  color: #7c3aed;
}

.empty-state {
  background: #f9fafc;
  border: 1px solid #eee;
  border-radius: 14px;
  min-height: 180px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #999;
}

.leave-list {
  display: grid;
  grid-template-columns: repeat(3, minmax(260px, 1fr));
  gap: 20px;
}

.leave-card {
  background: #f9fafc;
  padding: 18px;
  border-radius: 14px;
  border: 1px solid #eee;
  cursor: pointer;
  transition: 0.25s;
  overflow: hidden;
  min-height: 190px;
}

.leave-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 6px 16px rgba(0, 0, 0, 0.08);
}

.leave-header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 10px;
  font-weight: 600;
}

.leave-type {
  color: #2b3674;
}

.employee-name {
  margin-top: 6px;
  font-size: 13px;
  color: #7b87b3;
}

.leave-date {
  margin-top: 10px;
  font-size: 14px;
}

.leave-days {
  font-size: 13px;
  color: #666;
  margin-top: 4px;
}

.leave-reason {
  margin-top: 10px;
  font-size: 14px;
  color: #444;
  word-break: break-word;
  overflow-wrap: anywhere;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.comment-preview {
  margin-top: 8px;
  font-size: 12px;
  color: #666;
  word-break: break-word;
}

.view-detail {
  margin-top: 12px;
  font-size: 12px;
  color: #4318ff;
}

.status {
  padding: 4px 10px;
  border-radius: 12px;
  font-size: 12px;
  white-space: nowrap;
}

.approved {
  background: #e6f9f0;
  color: #1a9b5c;
}

.rejected {
  background: #ffe6e6;
  color: #dc2626;
}

.pending-hr {
  background: #f3e8ff;
  color: #7c3aed;
}

.pagination {
  display: flex;
  justify-content: center;
  gap: 8px;
  margin-top: 24px;
  flex-wrap: wrap;
}

.page-btn,
.page-number {
  border: 1px solid #ddd;
  background: white;
  min-width: 38px;
  height: 38px;
  border-radius: 10px;
  cursor: pointer;
  padding: 0 12px;
}

.page-number.active {
  background: #4318ff;
  color: white;
  border-color: #4318ff;
}

.page-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
  z-index: 999;
}

.modal-card {
  background: white;
  padding: 30px;
  border-radius: 16px;
  width: 650px;
  max-width: 90vw;
  max-height: 80vh;
  overflow: auto;
}

.reason-section {
  margin-top: 10px;
}

.reason-box {
  margin-top: 8px;
  padding: 12px;
  border: 1px solid #ddd;
  border-radius: 8px;
  background: #fafafa;
  max-height: 180px;
  overflow-y: auto;
  word-break: break-word;
}

.reject-textarea {
  width: 100%;
  margin-top: 8px;
  padding: 12px;
  border: 1px solid #ddd;
  border-radius: 10px;
  background: #fafafa;
  outline: none;
  resize: vertical;
  font-size: 14px;
  font-family: inherit;
  color: #333;
}

.reject-textarea:focus {
  border-color: #4318ff;
  box-shadow: 0 0 0 3px rgba(67, 24, 255, 0.08);
}

.modal-footer {
  margin-top: 20px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 12px;
}

.action-row {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
}

.reject-btn,
.approve-btn,
.close-btn {
  border: none;
  padding: 10px 16px;
  border-radius: 8px;
  cursor: pointer;
}

.reject-btn {
  background: #ffe6e6;
  color: #dc2626;
}

.approve-btn,
.close-btn {
  background: #4318ff;
  color: white;
}

@media (max-width: 1024px) {
  .leave-list {
    grid-template-columns: repeat(2, 1fr);
  }

  .stats-row {
    grid-template-columns: repeat(2, 1fr);
  }
}

@media (max-width: 768px) {
  .leave-container {
    padding: 20px;
  }

  .page-header {
    flex-direction: column;
    align-items: flex-start;
  }

  .header-actions {
    width: 100%;
    justify-content: flex-start;
    flex-direction: column;
  }

  .search-input,
  .filter-select,
  .refresh-btn {
    width: 100%;
  }

  .leave-list {
    grid-template-columns: 1fr;
    gap: 15px;
  }

  .stats-row {
    grid-template-columns: repeat(2, 1fr);
  }

  .modal-card {
    width: 100%;
    padding: 20px;
  }

  .reason-box {
    max-height: 140px;
  }

  .modal-footer {
    flex-direction: column;
    align-items: stretch;
  }

  .action-row {
    width: 100%;
    justify-content: stretch;
  }

  .action-row button,
  .close-btn {
    width: 100%;
  }
}
</style>