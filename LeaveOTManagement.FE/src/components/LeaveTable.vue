<template>
  <div class="table-container">
    <h3>Leave Requests</h3>

    <table>
      <thead>
        <tr>
          <th>Employee</th>
          <th>Start Date</th>
          <th>End Date</th>
          <th>Reason</th>
          <th>Status</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="leave in leaves" :key="leave.id">
          <td>{{ leave.employee }}</td>
          <td>{{ leave.startDate }}</td>
          <td>{{ leave.endDate }}</td>
          <td>{{ leave.reason }}</td>
          <td>
            <span :class="leave.status.toLowerCase()">
              {{ leave.status }}
            </span>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue"
import api from "@/services/api"

const leaves = ref([])

onMounted(async () => {
  try {
    const res = await api.get("/Leave")
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
  border-radius: 12px;
  box-shadow: 0 5px 20px rgba(0,0,0,0.05);
}

table {
  width: 100%;
  border-collapse: collapse;
}

th, td {
  padding: 12px;
  text-align: left;
  border-bottom: 1px solid #eee;
}

th {
  background: #f4f7fb;
}

.approved {
  color: green;
  font-weight: bold;
}

.pending {
  color: orange;
  font-weight: bold;
}

.rejected {
  color: red;
  font-weight: bold;
}
</style>