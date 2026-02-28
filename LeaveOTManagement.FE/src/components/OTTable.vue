<template>
  <div class="table-container">
    <h3>Overtime Requests</h3>

    <table>
      <thead>
        <tr>
          <th>Employee</th>
          <th>Date</th>
          <th>Hours</th>
          <th>Reason</th>
          <th>Status</th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="ot in ots" :key="ot.id">
          <td>{{ ot.employee }}</td>
          <td>{{ ot.date }}</td>
          <td>{{ ot.hours }}</td>
          <td>{{ ot.reason }}</td>
          <td>
            <span :class="ot.status.toLowerCase()">
              {{ ot.status }}
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

const ots = ref([])

onMounted(async () => {
  try {
    const res = await api.get("/Overtime")
    ots.value = res.data
  } catch (err) {
    console.log("Error loading OT data")
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
  border-bottom: 1px solid #eee;
}

th {
  background: #f4f7fb;
}
</style>