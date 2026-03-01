<template>
  <div class="dashboard">
    <h1>Manager Dashboard</h1>

    <div class="card-container">
      <div class="card">
        <h3>Pending Leave Approval</h3>
        <p>6</p>
      </div>

      <div class="card">
        <h3>Pending OT Approval</h3>
        <p>3</p>
      </div>

      <div class="card">
        <h3>Team Members</h3>
        <p>15</p>
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