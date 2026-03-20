<template>
  <div class="dashboard">
    <!-- HR ADMIN DASHBOARD -->
    <div class="dashboard-section">
      <div class="section-header">
        <h2>HR Admin Dashboard</h2>
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
          <h3>Total Users</h3>
          <p>{{ stats.totalUsers }}</p>
        </div>
      </div>
    </div>

    <!-- MANAGEMENT MODULES -->
    <div class="dashboard-section">
      <h2>Management Modules</h2>

      <div class="modules-grid">
        <div class="module-card" @click="goToCreateUser">
          <div class="module-icon">👤</div>
          <div class="module-content">
            <h3>User Management</h3>
            <p>Create new users and maintain employee access.</p>
          </div>
        </div>

        <div class="module-card" @click="goToAssignManager">
          <div class="module-icon">🧩</div>
          <div class="module-content">
            <h3>Manager Assignment</h3>
            <p>Assign or update reporting managers for employees.</p>
          </div>
        </div>

        <div class="module-card" @click="goToLeaveApprovals">
          <div class="module-icon">✅</div>
          <div class="module-content">
            <h3>Leave Approval Center</h3>
            <p>Review and process leave requests waiting for HR/Admin approval.</p>
          </div>
        </div>

        <div class="module-card" @click="goToOTApprovals">
          <div class="module-icon">⏱️</div>
          <div class="module-content">
            <h3>OT Approval Center</h3>
            <p>Review and process overtime requests across the company.</p>
          </div>
        </div>

        <div class="module-card" @click="goToHolidayCalendar">
          <div class="module-icon">📅</div>
          <div class="module-content">
            <h3>Holiday Calendar</h3>
            <p>Maintain public holidays and calendar configuration.</p>
          </div>
        </div>

        <div class="module-card" @click="goToReports">
          <div class="module-icon">📈</div>
          <div class="module-content">
            <h3>Reports & Analytics</h3>
            <p>Track leave and overtime trends through reporting dashboards.</p>
          </div>
        </div>

        <div class="module-card" @click="goToReportDashboard">
          <div class="module-icon">📑</div>
          <div class="module-content">
            <h3>Report Dashboard</h3>
            <p>Open the detailed reporting dashboard for HR/Admin.</p>
          </div>
        </div>
      </div>
    </div>

    <!-- TODAY SUMMARY -->
    <div class="dashboard-section">
      <h2>Today Summary</h2>

      <div class="summary-list">
        <div class="summary-item">
          <span class="summary-label">Leave approvals waiting</span>
          <span class="summary-value">{{ stats.pendingLeave }}</span>
        </div>

        <div class="summary-item">
          <span class="summary-label">OT approvals waiting</span>
          <span class="summary-value">{{ stats.pendingOt }}</span>
        </div>

        <div class="summary-item">
          <span class="summary-label">Total requests waiting</span>
          <span class="summary-value">{{ totalPending }}</span>
        </div>

        <div class="summary-item">
          <span class="summary-label">Total users</span>
          <span class="summary-value">{{ stats.totalUsers }}</span>
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
  totalUsers: 0,
  totalDepartments: 0
})

const totalPending = computed(() => {
  return (stats.value.pendingLeave || 0) + (stats.value.pendingOt || 0)
})

const loadStats = async () => {
  try {
    const [leaveRes, otRes, usersRes, deptRes] = await Promise.all([
      api.get("/Leave/pending-hr"),
      api.get("/OT/hr-pending"),
      api.get("/Users"),
      api.get("/Users/departments")
    ])

    const leaves = leaveRes.data || []
    const ots = otRes.data || []
    const users = usersRes.data || []
    const departments = deptRes.data || []

    stats.value.pendingLeave = leaves.length
    stats.value.pendingOt = ots.filter(x => x.status === "Pending").length
    stats.value.totalUsers = users.length
    stats.value.totalDepartments = departments.length
  } catch (error) {
    console.error("Admin dashboard load error:", error)
  }
}

onMounted(loadStats)

const goToLeaveApprovals = () => {
  router.push("/hr-approvals")
}

const goToOTApprovals = () => {
  router.push("/ot-hr-approval")
}

const goToHolidayCalendar = () => {
  router.push("/holidays")
}

const goToReports = () => {
  router.push("/reports")
}

const goToReportDashboard = () => {
  router.push("/report-dashboard")
}

const goToCreateUser = () => {
  router.push("/create-user")
}

const goToAssignManager = () => {
  router.push("/assign-manager")
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

/* MODULES */
.modules-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(280px, 1fr));
  gap: 20px;
  margin-top: 18px;
}

.module-card {
  display: flex;
  align-items: flex-start;
  gap: 16px;
  background: #f8f9fc;
  border-radius: 14px;
  padding: 20px;
  cursor: pointer;
  transition: 0.25s;
  border: 1px solid transparent;
}

.module-card:hover {
  transform: translateY(-3px);
  border-color: rgba(67, 24, 255, 0.15);
  box-shadow: 0 10px 20px rgba(67, 24, 255, 0.08);
}

.module-icon {
  font-size: 26px;
  line-height: 1;
}

.module-content h3 {
  margin: 0 0 8px;
  font-size: 18px;
  color: #2b3674;
}

.module-content p {
  margin: 0;
  color: #707eae;
  font-size: 14px;
  line-height: 1.5;
}

/* SUMMARY */
.summary-list {
  display: flex;
  flex-direction: column;
  gap: 14px;
  margin-top: 16px;
}

.summary-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  background: #f8f9fc;
  border-radius: 12px;
  padding: 16px 18px;
}

.summary-label {
  color: #2b3674;
  font-weight: 600;
}

.summary-value {
  color: #4318ff;
  font-weight: 700;
  font-size: 18px;
}

/* MOBILE */
@media (max-width: 768px) {
  .dashboard-section {
    padding: 18px;
  }

  .modules-grid {
    grid-template-columns: 1fr;
  }

  .summary-item {
    flex-direction: column;
    align-items: flex-start;
    gap: 6px;
  }
}
</style>