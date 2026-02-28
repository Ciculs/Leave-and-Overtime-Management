<template>
  <div class="dashboard">
    <h1>Employee Dashboard</h1>

    <div class="card-container">
      <div class="card">
        <h3>My Leave Requests</h3>
        <p>4</p>
      </div>

      <div class="card">
        <h3>My OT Requests</h3>
        <p>2</p>
      </div>

      <div class="card">
        <h3>Remaining Leave Days</h3>
        <p>10</p>
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