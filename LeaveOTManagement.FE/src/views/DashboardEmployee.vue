<template>
  <div class="dashboard">

    <!-- EMPLOYEE DASHBOARD -->
    <div class="dashboard-section">

      <div class="section-header">

        <h2>Employee Dashboard</h2>

      </div>

      <div class="stats-grid">

        <div class="stat-card">
          <h3>My Leave Requests</h3>
          <p>{{ leaveRequests }}</p>
        </div>

        <div class="stat-card">
          <h3>Pending Requests</h3>
          <p>{{ pendingLeaves }}</p>
        </div>

        <div class="stat-card">
          <h3>My OT Requests</h3>
          <p>{{ otRequests }}</p>
        </div>

        <div class="stat-card">
          <h3>Remaining Leave Days</h3>
          <p>{{ remainingLeave }}</p>
        </div>

        <div class="stat-card">
          <h3>Total OT Hours</h3>
          <p>{{ totalOtHours.toFixed(2) }}</p>
        </div>

      </div>

    </div>



    <!-- QUICK ACTIONS -->
    <div class="dashboard-section">

      <h2>Quick Actions</h2>

      <div class="actions-grid">

        <button @click="goToCreateLeave">
          Request Leave
        </button>

        <button @click="goToCreateOT">
          Request OT
        </button>

        <button @click="goToMyLeaves">
          View My Leaves
        </button>

        <button @click="goToMyOT">
          View My OTs
        </button>

      </div>

    </div>

        <!-- RECENT LEAVE REQUESTS -->
    <div class="dashboard-section">

      <h2>Recent Leave Requests</h2>

      <table>

        <thead>
          <tr>
            <th>Type</th>
            <th>From</th>
            <th>To</th>
            <th>Status</th>
          </tr>
        </thead>

        <tbody>

          <tr v-for="leave in leaves.slice(0, 5)" :key="leave.id">

            <td>{{ leave.leaveType }}</td>

            <td>{{ leave.fromDate }}</td>

            <td>{{ leave.toDate }}</td>

            <td>
              <span :class="'status ' + leave.status.toLowerCase()">
                {{ leave.status }}
              </span>
            </td>

          </tr>

        </tbody>

      </table>

    </div>

    <!-- PERSONAL CALENDAR -->
    <div class="dashboard-section">

      <h2>Personal Calendar</h2>

      <PersonalCalendar />

    </div>


  </div>
</template>



<script setup>

import { ref, onMounted } from "vue"
import { useRouter } from "vue-router"

import { getLeaveList, getLeaveBalance } from "@/services/leaveService"
import { getMyOT } from "@/services/otService"

import PersonalCalendar from "@/views/PersonalCalendar.vue"

const router = useRouter()

const leaveRequests = ref(0)
const pendingLeaves = ref(0)
const remainingLeave = ref(0)
const otRequests = ref(0)
const totalOtHours = ref(0)

const leaves = ref([])



const loadDashboard = async () => {

  try {

    /* LEAVES */

    const leaveRes = await getLeaveList()

    leaves.value = leaveRes.data || []

    leaveRequests.value = leaves.value.length

    pendingLeaves.value =
      leaves.value.filter(l => l.status === "Pending").length



    /* LEAVE BALANCE */

    const balances = (await getLeaveBalance()).data || []

    remainingLeave.value =
      balances.find(b => b.leaveTypeName === "Annual Leave")?.remainingDays || 0



    /* OT */

    const ots = (await getMyOT()).data || []

    otRequests.value = ots.length

    const approvedOT =
      ots.filter(o => o.status === "Approved")



    totalOtHours.value = approvedOT.reduce((sum, ot) => {

      const details = ot.details || []

      const hours = details.reduce((dSum, d) => {

        if (d.hours) return dSum + d.hours

        if (d.totalHours) return dSum + d.totalHours

        if (d.startTime && d.endTime) {

          const start = new Date(`1970-01-01T${d.startTime}`)
          const end = new Date(`1970-01-01T${d.endTime}`)

          return dSum + (end - start) / (1000 * 60 * 60)

        }

        return dSum

      }, 0)

      return sum + hours

    }, 0)

  }

  catch (err) {
    console.error("Dashboard load error:", err)
  }
  console.log("LEAVES:", JSON.parse(JSON.stringify(leaves.value)))

}



onMounted(loadDashboard)



const goToCreateLeave = () => {
  router.push("/leave/new")
}

const goToCreateOT = () => {
  router.push("/my-ot?create=true")
}

const goToMyLeaves = () => {
  router.push("/my-leaves")
}

const goToMyOT = () => {
  router.push("/my-ot")
}

</script>



<style scoped>
.dashboard {
  background: #f4f7fb;
}



/* SECTION CARD */

.dashboard-section {
  background: white;
  border-radius: 16px;
  padding: 25px;
  margin-bottom: 30px;
  box-shadow: 0 6px 18px rgba(0, 0, 0, 0.06);
}



/* HEADER */

.section-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}



/* BUTTON */

.btn-primary {
  background: linear-gradient(135deg, #5b3df5, #3a0df5);
  color: white;
  border: none;
  padding: 10px 18px;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
}



/* STATS GRID */

.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 20px;
}



/* STAT CARD */

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



/* ACTIONS */

.actions-grid {
  display: flex;
  gap: 20px;
  margin-top: 20px;
  flex-wrap: wrap;
}

.actions-grid button {
  background: linear-gradient(135deg, #4318ff, #5b3df5);
  color: white;
  border: none;
  padding: 12px 20px;
  border-radius: 10px;
  font-weight: 600;
  cursor: pointer;
  transition: 0.25s;
}

.actions-grid button:hover {
  transform: translateY(-2px);
  box-shadow: 0 8px 18px rgba(67, 24, 255, 0.2);
}

/* TABLE */

table {
  width: 100%;
  border-collapse: collapse;
  margin-top: 15px;
}

th {
  text-align: left;
  padding: 12px;
  border-bottom: 1px solid #eee;
}

td {
  padding: 12px;
  border-bottom: 1px solid #f1f1f1;
}



/* STATUS */

.status {
  padding: 4px 10px;
  border-radius: 6px;
  font-size: 12px;
}

.status.pending {
  background: #fff3cd;
}

.status.approved {
  background: #d4edda;
}

.status.rejected {
  background: #f8d7da;
}
</style>