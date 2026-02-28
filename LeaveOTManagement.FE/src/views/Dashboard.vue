<template>
  <div class="dashboard">

    

    <h2>Dashboard Overview</h2>

    <div class="cards">
      <div class="card">
        <div class="icon">📅</div>
        <h3>Total Leave Days</h3>
        <p>{{ stats.totalLeave }}</p>
      </div>

      <div class="card">
        <div class="icon">⏱️</div>
        <h3>Total OT Hours</h3>
        <p>{{ stats.totalOT }}</p>
      </div>

      <div class="card">
        <div class="icon">📌</div>
        <h3>Pending Requests</h3>
        <p>{{ stats.pending }}</p>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref } from "vue"

const stats = ref({
  totalLeave: 10,
  totalOT: 24,
  pending: 3
})
</script>

<style scoped>
.dashboard {
  padding: 30px;
}

/* TOP BAR */
.top-bar {
  display: flex;
  justify-content: flex-end;
  margin-bottom: 20px;
}

.auth-btn {
  padding: 8px 18px;
  border-radius: 8px;
  border: none;
  background: #1e3c72;
  color: white;
  font-weight: bold;
  cursor: pointer;
  transition: 0.3s;
}

.auth-btn:hover {
  background: #7392c0;
}

.logout {
  background: #c0392b;
}

.logout:hover {
  background: #962d22;
}

/* DASHBOARD */
.dashboard h2 {
  margin-bottom: 20px;
}

.cards {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 20px;
}

.card {
  background: white;
  padding: 25px;
  border-radius: 15px;
  box-shadow: 0 8px 20px rgba(0,0,0,0.08);
  text-align: center;
  transition: 0.3s;
}

.card:hover {
  transform: translateY(-5px);
}

.card h3 {
  margin: 10px 0;
  font-weight: 500;
  color: #9f9898;
}

.card p {
  font-size: 28px;
  font-weight: bold;
  color: #6b86a1;
}

.icon {
  font-size: 30px;
}
</style>