<template>
  <div class="dashboard">
    <!-- MANAGER DASHBOARD -->
    <div class="dashboard-section">
      <div class="section-header">
        <h2>Manager Dashboard</h2>
      </div>

      <div class="stats-grid">
        <div class="stat-card">
          <h3>Pending Leave Approval</h3>
          <p>{{ stats.pendingLeave }}</p>
        </div>

        <div class="stat-card">
          <h3>Pending OT Approval</h3>
          <p>{{ stats.pendingOt }}</p>
        </div>

        <div class="stat-card">
          <h3>Total Pending Requests</h3>
          <p>{{ totalPending }}</p>
        </div>

        <div class="stat-card">
          <h3>Team Members</h3>
          <p>{{ stats.teamMembers }}</p>
        </div>
      </div>
    </div>

    <!-- QUICK ACTIONS -->
    <div class="dashboard-section">
      <h2>Quick Actions</h2>

      <div class="actions-grid">
        <button @click="goToLeaveApprovals">
          Leave Approvals
        </button>

        <button @click="goToOTApprovals">
          OT Approvals
        </button>

        <button @click="goToTeamCalendar">
          Team Calendar
        </button>
      </div>
    </div>

    <!-- APPROVAL SUMMARY -->
    <div class="dashboard-section">
      <h2>Approval Summary</h2>

      <div class="table-wrap">
        <table class="summary-table">
          <thead>
            <tr>
              <th>Category</th>
              <th>Pending</th>
            </tr>
          </thead>

          <tbody>
            <tr>
              <td>Leave Requests</td>
              <td>
                <span class="status pending">{{ stats.pendingLeave }}</span>
              </td>
            </tr>

            <tr>
              <td>OT Requests</td>
              <td>
                <span class="status pending">{{ stats.pendingOt }}</span>
              </td>
            </tr>

            <tr>
              <td>Total</td>
              <td>
                <span class="status approved">{{ totalPending }}</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- RECENT LEAVE REQUESTS -->
    <div class="dashboard-section">
      <div class="section-header">
        <h2>Recent Leave Requests</h2>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th class="col-employee">Employee</th>
              <th class="col-date">Date</th>
              <th class="col-details">Details</th>
              <th class="col-status">Status</th>
              <th class="col-action">Action</th>
            </tr>
          </thead>

          <tbody>
            <tr v-if="recentPendingLeaves.length === 0">
              <td colspan="5" class="empty-cell">No pending leave requests</td>
            </tr>

            <tr v-for="leave in recentPendingLeaves" :key="leave.id">
              <td>{{ leave.employeeName || "Employee" }}</td>
              <td>{{ formatDate(leave.fromDate) }} → {{ formatDate(leave.toDate) }}</td>
              <td>
                <div
                  class="truncate-cell"
                  :title="leave.reason || leave.leaveType || 'Leave request'"
                >
                  {{ leave.reason || leave.leaveType || "Leave request" }}
                </div>
              </td>
              <td>
                <span class="status pending">{{ leave.status || "Pending" }}</span>
              </td>
              <td>
                <div class="action-group">
                  <button class="btn-action btn-detail" @click="openLeaveModal(leave)">
                    Details
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- RECENT OT REQUESTS -->
    <div class="dashboard-section">
      <div class="section-header">
        <h2>Recent OT Requests</h2>
      </div>

      <div class="table-wrap">
        <table class="data-table">
          <thead>
            <tr>
              <th class="col-employee">Employee</th>
              <th class="col-date">Date</th>
              <th class="col-hours">Hours</th>
              <th class="col-reason">Reason</th>
              <th class="col-status">Status</th>
              <th class="col-action">Action</th>
            </tr>
          </thead>

          <tbody>
            <tr v-if="recentPendingOTs.length === 0">
              <td colspan="6" class="empty-cell">No pending OT requests</td>
            </tr>

            <tr v-for="ot in recentPendingOTs" :key="ot.id">
              <td>{{ ot.employeeName || "Employee" }}</td>
              <td>{{ formatDate(ot.details?.[0]?.workDate) }}</td>
              <td>{{ calculateOTHours(ot.details?.[0]) }} hours</td>
              <td>
                <div
                  class="truncate-cell"
                  :title="ot.reason || 'OT request'"
                >
                  {{ ot.reason || "OT request" }}
                </div>
              </td>
              <td>
                <span class="status pending">{{ ot.status || "Pending" }}</span>
              </td>
              <td>
                <div class="action-group">
                  <button class="btn-action btn-detail" @click="openOTModal(ot)">
                    Details
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- LEAVE MODAL -->
    <div v-if="selectedLeave" class="modal-overlay" @click.self="selectedLeave = null">
      <div class="modal-card">
        <button class="modal-close" @click="selectedLeave = null">✕</button>

        <h3>Leave Request Details</h3>

        <div class="modal-grid">
          <div class="modal-item">
            <span class="modal-label">Employee</span>
            <span>{{ selectedLeave.employeeName || "Employee" }}</span>
          </div>

          <div class="modal-item">
            <span class="modal-label">Date</span>
            <span>{{ formatDate(selectedLeave.fromDate) }} → {{ formatDate(selectedLeave.toDate) }}</span>
          </div>

          <div class="modal-item full">
            <span class="modal-label">Reason</span>
            <div class="modal-text-box">
              {{ selectedLeave.reason || selectedLeave.leaveType || "Leave request" }}
            </div>
          </div>

          <div class="modal-item">
            <span class="modal-label">Status</span>
            <span class="status pending">{{ selectedLeave.status || "Pending" }}</span>
          </div>
        </div>

        <div class="modal-footer">
          <button class="btn-primary" @click="goToLeaveApprovals">
            Go to Leave Approvals
          </button>
        </div>
      </div>
    </div>

    <!-- OT MODAL -->
    <div v-if="selectedOT" class="modal-overlay" @click.self="selectedOT = null">
      <div class="modal-card">
        <button class="modal-close" @click="selectedOT = null">✕</button>

        <h3>OT Request Details</h3>

        <div class="modal-grid">
          <div class="modal-item">
            <span class="modal-label">Employee</span>
            <span>{{ selectedOT.employeeName || "Employee" }}</span>
          </div>

          <div class="modal-item">
            <span class="modal-label">Date</span>
            <span>{{ formatDate(selectedOT.details?.[0]?.workDate) }}</span>
          </div>

          <div class="modal-item">
            <span class="modal-label">Hours</span>
            <span>{{ calculateOTHours(selectedOT.details?.[0]) }} hours</span>
          </div>

          <div class="modal-item">
            <span class="modal-label">Status</span>
            <span class="status pending">{{ selectedOT.status || "Pending" }}</span>
          </div>

          <div class="modal-item full">
            <span class="modal-label">Reason</span>
            <div class="modal-text-box">
              {{ selectedOT.reason || "OT request" }}
            </div>
          </div>
        </div>

        <div class="modal-footer">
          <button class="btn-primary" @click="goToOTApprovals">
            Go to OT Approvals
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue"
import { useRouter } from "vue-router"
import api from "@/services/api"

const router = useRouter()

const stats = ref({
  pendingLeave: 0,
  pendingOt: 0,
  teamMembers: 0
})

const pendingLeaves = ref([])
const pendingOTs = ref([])

const selectedLeave = ref(null)
const selectedOT = ref(null)

const totalPending = computed(() => {
  return (stats.value.pendingLeave || 0) + (stats.value.pendingOt || 0)
})

const formatDate = (date) => {
  if (!date) return ""
  return new Date(date).toLocaleDateString("vi-VN")
}

const calculateOTHours = (detail) => {
  if (!detail?.fromTime || !detail?.toTime) return "0.00"

  const [h1, m1] = detail.fromTime.split(":").map(Number)
  const [h2, m2] = detail.toTime.split(":").map(Number)

  const start = h1 + m1 / 60
  const end = h2 + m2 / 60

  return (end - start).toFixed(2)
}

const recentPendingLeaves = computed(() => {
  return [...pendingLeaves.value]
    .sort((a, b) => new Date(b.fromDate || 0) - new Date(a.fromDate || 0))
    .slice(0, 5)
})

const recentPendingOTs = computed(() => {
  return [...pendingOTs.value]
    .sort((a, b) => {
      const d1 = new Date(a.details?.[0]?.workDate || 0)
      const d2 = new Date(b.details?.[0]?.workDate || 0)
      return d2 - d1
    })
    .slice(0, 5)
})

const loadStats = async () => {
  try {
    const res = await api.get("/Leave/manager-stats")
    stats.value = res.data || {
      pendingLeave: 0,
      pendingOt: 0,
      teamMembers: 0
    }
  } catch (error) {
    console.error("Manager stats load error:", error)
  }
}

const loadPendingRequests = async () => {
  try {
    const [leaveRes, otRes] = await Promise.all([
      api.get("/Leave/pending-manager"),
      api.get("/OT/pending")
    ])

    pendingLeaves.value = leaveRes.data || []
    pendingOTs.value = otRes.data || []
  } catch (error) {
    console.error("Pending requests load error:", error)
  }
}

const loadDashboard = async () => {
  await Promise.all([loadStats(), loadPendingRequests()])
}

onMounted(loadDashboard)

const openLeaveModal = (leave) => {
  selectedLeave.value = leave
}

const openOTModal = (ot) => {
  selectedOT.value = ot
}

const goToLeaveApprovals = () => {
  selectedLeave.value = null
  router.push("/team-approvals")
}

const goToOTApprovals = () => {
  selectedOT.value = null
  router.push("/ot-manager-approval")
}

const goToTeamCalendar = () => {
  router.push("/team-calendar")
}
</script>

<style scoped>
.dashboard {
  background: #f4f7fb;
}

.dashboard-section {
  background: white;
  border-radius: 16px;
  padding: 25px;
  margin-bottom: 30px;
  box-shadow: 0 6px 18px rgba(0, 0, 0, 0.06);
}

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
}

.stat-card {
  background: #f8f9fc;
  border-radius: 12px;
  padding: 20px;
}

.stat-card h3 {
  font-size: 14px;
  color: #666;
}

.stat-card p {
  font-size: 26px;
  font-weight: 700;
  color: #4318ff;
  margin-top: 10px;
}

.actions-grid {
  display: flex;
  gap: 20px;
  margin-top: 20px;
  flex-wrap: wrap;
}

.actions-grid button,
.btn-primary {
  background: linear-gradient(135deg, #4318ff, #5b3df5);
  color: white;
  border: none;
  padding: 12px 20px;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  transition: 0.25s;
}

.actions-grid button:hover,
.btn-primary:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 18px rgba(67, 24, 255, 0.2);
}

.table-wrap {
  width: 100%;
  overflow-x: auto;
}

.data-table,
.summary-table {
  width: 100%;
  border-collapse: collapse;
}

.data-table {
  table-layout: fixed;
}

th {
  text-align: left;
  padding: 12px;
  border-bottom: 1px solid #eee;
}

td {
  padding: 12px;
  border-bottom: 1px solid #f1f1f1;
  vertical-align: middle;
}

.empty-cell {
  text-align: center;
  color: #94a3b8;
  padding: 24px;
}

.col-employee {
  width: 16%;
}

.col-date {
  width: 20%;
}

.col-details,
.col-reason {
  width: 30%;
}

.col-hours {
  width: 12%;
}

.col-status {
  width: 10%;
}

.col-action {
  width: 12%;
}

.truncate-cell {
  overflow: hidden;
  white-space: nowrap;
  text-overflow: ellipsis;
  max-width: 100%;
  display: block;
}

.status {
  padding: 4px 10px;
  border-radius: 6px;
  font-size: 12px;
  display: inline-block;
  min-width: 48px;
  text-align: center;
}

.status.pending {
  background: #fff3cd;
  color: #a16207;
}

.status.approved {
  background: #d4edda;
  color: #166534;
}

.action-group {
  display: flex;
  gap: 8px;
  flex-wrap: wrap;
}

.btn-action {
  border: none;
  border-radius: 8px;
  padding: 7px 12px;
  font-size: 12px;
  font-weight: 600;
  cursor: pointer;
}

.btn-detail {
  background: #eef2ff;
  color: #4318ff;
}

.btn-go {
  background: #4318ff;
  color: white;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background: rgba(15, 23, 42, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 20px;
  z-index: 1000;
}

.modal-card {
  background: white;
  width: 700px;
  max-width: 95vw;
  max-height: 85vh;
  overflow-y: auto;
  border-radius: 18px;
  padding: 24px;
  position: relative;
  box-shadow: 0 16px 40px rgba(0, 0, 0, 0.16);
}

.modal-close {
  position: absolute;
  top: 14px;
  right: 16px;
  border: none;
  background: transparent;
  font-size: 20px;
  cursor: pointer;
}

.modal-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 16px;
  margin-top: 18px;
}

.modal-item {
  display: flex;
  flex-direction: column;
  gap: 8px;
  background: #f8f9fc;
  border-radius: 12px;
  padding: 14px;
}

.modal-item.full {
  grid-column: 1 / -1;
}

.modal-label {
  font-size: 12px;
  font-weight: 700;
  color: #707eae;
  text-transform: uppercase;
}

.modal-text-box {
  background: white;
  border: 1px solid #e9edf7;
  border-radius: 10px;
  padding: 12px;
  line-height: 1.6;
  word-break: break-word;
  white-space: pre-wrap;
}

.modal-footer {
  margin-top: 18px;
  display: flex;
  justify-content: flex-end;
}

@media (max-width: 768px) {
  .dashboard-section {
    padding: 18px;
  }

  .actions-grid {
    flex-direction: column;
  }

  .actions-grid button {
    width: 100%;
  }

  .data-table,
  .summary-table {
    min-width: 860px;
  }

  .modal-grid {
    grid-template-columns: 1fr;
  }
}
</style>