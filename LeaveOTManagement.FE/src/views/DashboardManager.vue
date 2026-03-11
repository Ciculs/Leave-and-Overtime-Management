<template>
  <div class="dashboard">
    <h1>Manager Dashboard</h1>

    <div class="card-container">
      <div class="card">
        <h3>Pending Leave Approval</h3>
        <p>{{ stats.pendingLeave }}</p>
      </div>

      <div class="card">
        <h3>Pending OT Approval</h3>
        <p>{{ stats.pendingOt }}</p>
      </div>

      <div class="card">
        <h3>Team Members</h3>
        <p>{{ stats.teamMembers }}</p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue"
import api from "@/services/api"

const stats = ref({
  pendingLeave: 0,
  pendingOt: 0,
  teamMembers: 0
})

const loadStats = async () => {
  try {
    const res = await api.get("/Leave/manager-stats")
    stats.value = res.data
  } catch (error) {
    console.error(error)
  }
}

onMounted(() => {
  loadStats()
})
</script>

<style scoped>
.dashboard {
  padding: 20px;
}
.card-container {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 20px;
}
.card {
  background: white;
  padding: 20px;
  border-radius: 10px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.1);
}
</style>