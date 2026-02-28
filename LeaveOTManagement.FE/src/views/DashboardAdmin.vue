<template>
  <div class="dashboard">
    <h1>Admin Dashboard</h1>

    <div class="card-container">
      <div class="card">
        <h3>Total Users</h3>
        <p>120</p>
      </div>

      <div class="card">
        <h3>Total Departments</h3>
        <p>8</p>
      </div>

      <div class="card">
        <h3>Total Leave Requests</h3>
        <p>45</p>
      </div>

      <div class="card">
        <h3>Total OT Requests</h3>
        <p>20</p>
      </div>
    </div>
  </div>
</template>

<script setup>
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