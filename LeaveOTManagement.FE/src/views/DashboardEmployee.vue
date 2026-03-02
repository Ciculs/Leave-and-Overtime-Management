<template>
  <div class="dashboard">
    <div class="header-row">
      <h1>Employee Dashboard</h1>
      <button class="btn-create" @click="goToLeaveRequest">+ Create Leave</button>
    </div>

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
import { useRouter } from "vue-router"

const router = useRouter()

const goToLeaveRequest = () => {
  router.push("/leave/new")
}
</script>

<style scoped>
.dashboard {
  padding: 20px;
}
.header-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}
.header-row h1 {
  margin: 0;
}
.btn-create {
  background: #4318ff;
  color: white;
  padding: 10px 20px;
  border: none;
  border-radius: 8px;
  font-weight: 600;
  cursor: pointer;
  transition: 0.3s;
}
.btn-create:hover {
  background: #3311cc;
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