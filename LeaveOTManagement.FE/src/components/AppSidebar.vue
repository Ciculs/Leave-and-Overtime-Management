<template>
  <aside :class="['sidebar', { collapsed }]">

    <!-- TOP BAR (Mobile Toggle) -->
    <div class="mobile-header">
      <div class="brand">
        <div class="logo-box">D</div>
        <span v-if="!collapsed">Directory</span>
      </div>

      <button class="toggle-btn" @click="collapsed = !collapsed">
        ☰
      </button>
    </div>

    <nav class="nav-menu">

      <router-link :to="dashboardLink" class="nav-item">
        📊 <span v-if="!collapsed">Dashboard</span>
      </router-link>

      <!-- EMPLOYEE -->
      <template v-if="role === 'Employee'">
        <router-link to="/my-leaves" class="nav-item">
          📄 <span v-if="!collapsed">My Leave Requests</span>
        </router-link>

        <router-link to="/my-ot" class="nav-item">
          ⏱ <span v-if="!collapsed">My OT</span>
        </router-link>
      </template>

      <!-- MANAGER -->
      <!-- MANAGER -->
      <template v-if="role === 'Manager'">
        <router-link to="/manager" class="nav-item">
          ✅ <span v-if="!collapsed">Team Approvals</span>
        </router-link>

        <router-link to="/ot-manager-approval" class="nav-item">
          ⏱ <span v-if="!collapsed">OT Approvals</span>
        </router-link>

        <router-link to="/team-calendar" class="nav-item">
          📅 <span v-if="!collapsed">Team Calendar</span>
        </router-link>
      </template>

      <!-- HR -->
      <template v-if="role === 'HR'">
        <router-link to="/holidays" class="nav-item">
          📅 <span v-if="!collapsed">Holiday Calendar</span>
        </router-link>

        <router-link to="/ot-hr-approval" class="nav-item">
          ⏱ <span v-if="!collapsed">OT HR Approval</span>
        </router-link>

        <router-link to="/reports" class="nav-item">
          📈 <span v-if="!collapsed">Reporting</span>
        </router-link>
      </template>

    </nav>
  </aside>
</template>

<script setup>
import { computed, ref } from "vue"

const role = localStorage.getItem("role") || ""
const collapsed = ref(false)

const dashboardLink = computed(() => {
  if (role === "Admin") return "/admin"
  if (role === "Manager") return "/manager"
  if (role === "Employee") return "/employee"
  if (role === "HR") return "/holidays"
  return "/login"
})
</script>

<style scoped>
.sidebar {
  width: 280px;
  background: white;
  padding: 40px 20px;
  border-right: 1px solid #e2e8f0;
  transition: all 0.3s ease;
}

/* COLLAPSED */
.sidebar.collapsed {
  width: 90px;
}

.sidebar.collapsed .nav-item {
  justify-content: center;
}

/* MOBILE HEADER */
.mobile-header {
  display: none;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 30px;
}

.toggle-btn {
  background: none;
  border: none;
  font-size: 22px;
  cursor: pointer;
}

/* Brand */
.brand {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 24px;
  font-weight: 800;
  margin-bottom: 50px;
}

.logo-box {
  width: 40px;
  height: 40px;
  background: #4318ff;
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 10px;
}

/* Nav */
.nav-menu {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 15px 20px;
  text-decoration: none;
  color: #a3aed0;
  font-weight: 600;
  border-radius: 12px;
  transition: 0.3s;
}

.nav-item:hover {
  background: #f4f7fe;
  color: #2b3674;
}

.router-link-active {
  background: #4318ff;
  color: white !important;
}

/* TABLET */
@media (max-width: 1024px) {
  .sidebar {
    width: 220px;
    padding: 30px 15px;
  }
}

/* MOBILE */
@media (max-width: 768px) {

  .sidebar {
    width: 100%;
    padding: 15px;
    border-right: none;
    border-bottom: 1px solid #e2e8f0;
  }

  .mobile-header {
    display: flex;
  }

  .brand {
    margin-bottom: 0;
    font-size: 18px;
  }

  .nav-menu {
    flex-direction: row;
    overflow-x: auto;
    gap: 10px;
    margin-top: 15px;
  }

  .nav-item {
    padding: 10px 14px;
    font-size: 13px;
    white-space: nowrap;
  }
}
</style>