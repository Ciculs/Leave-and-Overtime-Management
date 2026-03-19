<template>
  <div class="dashboard">

    <!-- HEADER -->
    <div class="header">
      <h1>HR Dashboard</h1>

      <div class="actions">
        <button class="create-btn" @click="goCreateUser">
          + Create Employee
        </button>

        <button class="assign-btn" @click="goAssignManager">
          Assign Manager
        </button>
      </div>
    </div>

    <!-- STATS -->
    <div class="card-container">

      <div class="card">
        <h3>Total Employees</h3>
        <p>{{ users.length }}</p>
      </div>

      <div class="card">
        <h3>Active Users</h3>
        <p>{{ activeCount }}</p>
      </div>

      <div class="card">
        <h3>Inactive Users</h3>
        <p>{{ inactiveCount }}</p>
      </div>

      <div class="card">
        <h3>Managers</h3>
        <p>{{ managerCount }}</p>
      </div>

    </div>

    <!-- SEARCH -->
    <div class="search-bar">
      <input v-model="search" placeholder="Search employee..." />

      <select v-model="role">
        <option value="">All Roles</option>
        <option>Admin</option>
        <option>Manager</option>
        <option>Employee</option>
      </select>

      <button @click="loadUsers">Search</button>
    </div>

    <!-- USER TABLE -->
    <div class="table-container">
      <table>
        <thead>
          <tr>
            <th>Code</th>
            <th>Name</th>
            <th>Email</th>
            <th>Department</th>
            <th>Role</th>
            <th>Manager</th>
            <th>Status</th>
            <th>Action</th>
          </tr>
        </thead>

        <tbody>
          <tr v-for="u in users" :key="u.id">
            <td>{{ u.employeeCode }}</td>
            <td>{{ u.fullName }}</td>
            <td>{{ u.email }}</td>
            <td>{{ u.department }}</td>
            <td>{{ u.role }}</td>
            <td>{{ u.manager || "-" }}</td>

            <td>
              <span v-if="u.isActive" class="active">Active</span>
              <span v-else class="inactive">Inactive</span>
            </td>

            <td>
              <button class="edit-btn" @click="handleEdit(u)">Edit</button>
              <button 
                class="deactivate-btn" 
                @click="handleDeactivate(u.id)"
                v-if="u.isActive"
              >
                Deactivate
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

  </div>
</template>

<script setup>
import { ref, onMounted, computed } from "vue"
import { useRouter } from "vue-router"
import axios from "../services/axios"

const router = useRouter()

const users = ref([])
const search = ref("")
const role = ref("")

// ================= LOAD USERS =================
const loadUsers = async () => {
  const res = await axios.get("/users", {
    params: {
      search: search.value,
      role: role.value
    }
  })
  users.value = res.data
}

// ================= STATS =================
const activeCount = computed(() =>
  users.value.filter(u => u.isActive).length
)

const inactiveCount = computed(() =>
  users.value.filter(u => !u.isActive).length
)

const managerCount = computed(() =>
  users.value.filter(u => u.role === "Manager").length
)

// ================= DEACTIVATE =================
const handleDeactivate = async (id) => {
  if (!confirm("Deactivate this employee?")) return

  await axios.put(`/users/${id}/deactivate`)
  loadUsers()
}

// ================= EDIT =================
const handleEdit = async (user) => {
  const name = prompt("New name", user.fullName)
  if (!name) return

  await axios.put(`/users/${user.id}`, {
    fullName: name,
    roleId: 1,
    departmentId: 1
  })

  loadUsers()
}

// ================= NAVIGATION =================
const goCreateUser = () => {
  router.push("/create-user")
}

const goAssignManager = () => {
  router.push("/assign-manager")
}

onMounted(loadUsers)
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

/* actions */
.actions{
  display:flex;
  gap:12px;
}

/* buttons */
.create-btn{
  background:#4CAF50;
  color:white;
  border:none;
  padding:10px 18px;
  border-radius:6px;
  cursor:pointer;
}

.assign-btn{
  background:#2196F3;
  color:white;
  border:none;
  padding:10px 18px;
  border-radius:6px;
  cursor:pointer;
}

/* cards */
.card-container {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
  gap: 20px;
  margin-bottom: 20px;
}

.card {
  background: white;
  padding: 22px;
  border-radius: 10px;
  box-shadow: 0 3px 12px rgba(0,0,0,0.1);
}

.card p{
  font-size:22px;
  font-weight:600;
}

/* SEARCH */
.search-bar{
  display:flex;
  gap:10px;
  margin-bottom:20px;
}

/* TABLE */
.table-container{
  background:white;
  padding:15px;
  border-radius:10px;
  box-shadow:0 3px 10px rgba(0,0,0,0.1);
}

table{
  width:100%;
  border-collapse:collapse;
}

th, td{
  padding:10px;
  border-bottom:1px solid #eee;
}

/* status */
.active{
  color:green;
}

.inactive{
  color:red;
}

/* buttons */
.edit-btn{
  background:#ffc107;
  border:none;
  padding:6px 10px;
  margin-right:5px;
  border-radius:5px;
}

.deactivate-btn{
  background:#f44336;
  color:white;
  border:none;
  padding:6px 10px;
  border-radius:5px;
}

</style>