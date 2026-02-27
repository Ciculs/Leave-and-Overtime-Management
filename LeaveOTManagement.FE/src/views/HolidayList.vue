<template>
  <div class="card">
    <div class="card-header">
      <h3>Holiday Management</h3>
      <button class="btn-import">Import</button>
    </div>
    <div class="card-body">
      <p class="empty">No holidays found.</p>
    </div>
  </div>
</template>

<style scoped>
.card {
  background: white;
  border-radius: 20px;
  padding: 25px;
  box-shadow: 14px 17px 40px 4px rgba(112, 144, 176, 0.08);
}

.card-header {
  display: flex;
  justify-content: space-between;
  margin-bottom: 20px;
}

.btn-import {
  background: #4318ff;
  color: white;
  border: none;
  padding: 10px 25px;
  border-radius: 12px;
  cursor: pointer;
}
</style>