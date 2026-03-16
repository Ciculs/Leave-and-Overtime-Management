<template>
  <div class="dashboard">

    <div class="header">
      <h1>Admin Dashboard</h1>

      <div class="actions">
        <button class="create-btn" @click="goCreateUser">
          + Create User
        </button>

        <button class="assign-btn" @click="goAssignManager">
          Assign Manager
        </button>
      </div>

    </div>

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
import { useRouter } from "vue-router"

const router = useRouter()

const goCreateUser = () => {
  router.push("/create-user")
}

const goAssignManager = () => {
  router.push("/assign-manager")
}
</script>

<style scoped>

.dashboard {
  padding: 20px;
}

/* header */
.header{
  display:flex;
  justify-content:space-between;
  align-items:center;
  margin-bottom:25px;
}

/* action buttons */
.actions{
  display:flex;
  gap:12px;
}

/* create user button */
.create-btn{
  background:#4CAF50;
  color:white;
  border:none;
  padding:10px 18px;
  border-radius:6px;
  cursor:pointer;
  font-weight:600;
}

.create-btn:hover{
  background:#45a049;
}

/* assign manager button */
.assign-btn{
  background:#2196F3;
  color:white;
  border:none;
  padding:10px 18px;
  border-radius:6px;
  cursor:pointer;
  font-weight:600;
}

.assign-btn:hover{
  background:#1e88e5;
}

/* cards */
.card-container {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 20px;
}

.card {
  background: white;
  padding: 22px;
  border-radius: 10px;
  box-shadow: 0 3px 12px rgba(0,0,0,0.1);
}

.card h3{
  margin-bottom:10px;
}

.card p{
  font-size:22px;
  font-weight:600;
}

</style>