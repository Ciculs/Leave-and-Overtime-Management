<template>
  <div class="dashboard">

    <!-- HEADER -->
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

    <!-- SEARCH -->
    <div class="search-bar">
      <input v-model="search" placeholder="Search by name..." />

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
import { ref, onMounted } from "vue"
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

// ================= DEACTIVATE =================
const handleDeactivate = async (id) => {
  if (!confirm("Are you sure to deactivate this user?")) return

  await axios.put(`/users/${id}/deactivate`)
  loadUsers()
}

// ================= EDIT =================
const handleEdit = async (user) => {
  const name = prompt("New name", user.fullName)
  if (!name) return

  await axios.put(`/users/${user.id}`, {
    fullName: name,
    roleId: 1,        // tạm hardcode (có thể nâng cấp sau)
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

/* SEARCH */
.search-bar{
  display:flex;
  gap:10px;
  margin-bottom:20px;
}

.search-bar input,
.search-bar select{
  padding:8px;
  border-radius:6px;
  border:1px solid #ccc;
}

.search-bar button{
  padding:8px 14px;
  background:#333;
  color:white;
  border:none;
  border-radius:6px;
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
  text-align:left;
  border-bottom:1px solid #eee;
}

/* status */
.active{
  color:green;
  font-weight:600;
}

.inactive{
  color:red;
  font-weight:600;
}

/* action buttons */
.edit-btn{
  background:#ffc107;
  border:none;
  padding:6px 10px;
  margin-right:5px;
  border-radius:5px;
  cursor:pointer;
}

.deactivate-btn{
  background:#f44336;
  color:white;
  border:none;
  padding:6px 10px;
  border-radius:5px;
  cursor:pointer;
}

</style>