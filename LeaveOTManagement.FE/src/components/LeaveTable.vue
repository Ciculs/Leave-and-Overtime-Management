<template>
  <div class="table-container">
    <h2>My Leave Requests</h2>

    <div class="table-wrapper">
      <table>
        <thead>
          <tr>
            <th>Leave Type</th>
            <th>From</th>
            <th>To</th>
            <th>Total Days</th>
            <th class="hide-mobile">Reason</th>
            <th>Status</th>
          </tr>
        </thead>

        <tbody>
          <tr v-for="leave in leaves" :key="leave.id">
            <td>{{ leave.leaveType }}</td>
            <td>{{ formatDate(leave.fromDate) }}</td>
            <td>{{ formatDate(leave.toDate) }}</td>
            <td>{{ leave.totalDays }}</td>
            <td class="hide-mobile">{{ leave.reason }}</td>
            <td>
              <span :class="['status-badge', leave.status.toLowerCase()]">
                {{ leave.status }}
              </span>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <p v-if="leaves.length === 0" class="empty">
      No leave requests found.
    </p>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue"
import { useRoute } from "vue-router"
import api from "@/services/api"

const route = useRoute()
const leaves = ref([])

const apiUrl = route.props?.apiUrl || "/Leave/my"

const formatDate = (date) => {
  return new Date(date).toLocaleDateString("vi-VN")
}

onMounted(async () => {
  try {
    const res = await api.get(apiUrl)
    leaves.value = res.data
  } catch (err) {
    console.log("Error loading leave data")
  }
})
</script>

<style scoped>
.table-container {
  background: white;
  padding: 20px;
  border-radius: 16px;
  box-shadow: 0 5px 20px rgba(0,0,0,0.05);
}

.table-wrapper {
  overflow-x: auto;
}

table {
  width: 100%;
  border-collapse: collapse;
  min-width: 700px;
}

th, td {
  padding: 14px;
  text-align: left;
  border-bottom: 1px solid #eee;
}

th {
  background: #f4f7fb;
}

.status-badge {
  padding: 6px 12px;
  border-radius: 20px;
  font-size: 13px;
  font-weight: 600;
}

.approved {
  background: #e6f9f0;
  color: #1a9b5c;
}

.pending {
  background: #fff5e6;
  color: #d97706;
}

.rejected {
  background: #ffe6e6;
  color: #dc2626;
}

.hide-mobile {
  display: table-cell;
}

@media (max-width: 768px) {
  .hide-mobile {
    display: none;
  }
}

.empty {
  margin-top: 20px;
  color: gray;
}
</style>