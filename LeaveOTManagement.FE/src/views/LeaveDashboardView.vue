<template>
  <div>
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h3>Leave Dashboard</h3>
      <button @click="goToRequestForm" class="btn btn-primary fw-bold">
        + Create Leave Request
      </button>
    </div>

    <div class="row text-center mb-5">
      <div class="col-md-4 mb-3">
        <div class="card shadow-sm border-primary">
          <div class="card-body">
            <h5 class="card-title text-muted">Total Annual Leave</h5>
            <h2 class="text-primary fw-bold">
              {{ leaveBalance.totalDays }} <span class="fs-6 fw-normal">days</span>
            </h2>
          </div>
        </div>
      </div>

      <div class="col-md-4 mb-3">
        <div class="card shadow-sm border-danger">
          <div class="card-body">
            <h5 class="card-title text-muted">Used</h5>
            <h2 class="text-danger fw-bold">
              {{ leaveBalance.usedDays }} <span class="fs-6 fw-normal">days</span>
            </h2>
          </div>
        </div>
      </div>

      <div class="col-md-4 mb-3">
        <div class="card shadow-sm border-success">
          <div class="card-body">
            <h5 class="card-title text-muted">Current Balance</h5>
            <h2 class="text-success fw-bold">
              {{ leaveBalance.balance }} <span class="fs-6 fw-normal">days</span>
            </h2>
          </div>
        </div>
      </div>
    </div>

    <h4 class="mb-3 text-secondary border-bottom pb-2">Leave Request History</h4>
    <div class="card shadow-sm border-0">
      <table class="table table-hover align-middle mb-0">
        <thead class="table-light">
          <tr>
            <th>Request ID</th>
            <th>From Date</th>
            <th>To Date</th>
            <th>Total Days</th>
            <th>Reason</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          <tr v-if="leaveList.length === 0">
            <td colspan="6" class="text-center text-muted py-4">You have no leave requests yet.</td>
          </tr>
          <tr v-for="leave in leaveList" :key="leave.id">
            <td class="fw-bold text-secondary">#{{ leave.id }}</td>
            <td>{{ leave.fromDate }}</td>
            <td>{{ leave.toDate }}</td>
            <td class="fw-bold">{{ leave.totalDays }} days</td>
            <td>{{ leave.reason }}</td>
            <td>
              <span class="badge" :class="getStatusBadge(leave.status)">
                {{ leave.status }}
              </span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getLeaveBalance, getLeaveList } from '../services/leaveService'

const router = useRouter()

const leaveBalance = ref({
  totalDays: 0,
  usedDays: 0,
  balance: 0
})

const leaveList = ref([])

const fetchData = async () => {
  try {
    const userId = 3; 

    const balanceRes = await getLeaveBalance(userId)
    leaveBalance.value = balanceRes.data

    const listRes = await getLeaveList(userId)
    leaveList.value = listRes.data

  } catch (error) {
    console.error('API Error:', error)
  }
}

onMounted(() => {
  fetchData()
})

const getStatusBadge = (status) => {
  if (status === 'Pending') return 'bg-warning text-dark'
  if (status === 'Approved') return 'bg-success'
  if (status === 'Rejected') return 'bg-danger'
  return 'bg-secondary'
}

const goToRequestForm = () => {
  router.push('/leave/request')
}
</script>