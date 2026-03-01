<template>
  <div class="layout-wrapper">
    <aside class="sidebar">
      <div class="brand">
        <div class="logo-box">D</div>
        <span>Directory</span>
      </div>

      <nav class="nav-menu">
        <router-link to="/dashboard" class="nav-item">
          <span class="icon">📊</span> Dashboard
        </router-link>
      </nav>
    </aside>

    <div class="main-container">
      <header class="header">
        <div class="header-left">
          <div class="breadcrumb">Dashboard / Directory</div>
          <h2>Directory Dashboard</h2>
        </div>
       <div class="header-right">
  <div class="user-info">
    <div class="avatar-circle">
  <svg xmlns="http://www.w3.org/2000/svg" 
       width="20" height="20" 
       fill="currentColor" 
       viewBox="0 0 16 16">
    <path d="M8 8a3 3 0 1 0 0-6 3 3 0 0 0 0 6z"/>
    <path d="M14 14s-1-4-6-4-6 4-6 4h12z"/>
  </svg>
</div>
    <div class="user-text">
      <span class="name">{{ username }}</span>
      <span class="role">{{ role }}</span>
    </div>
  </div>

  <button class="logout-btn" @click="logout">
    Logout
  </button>
</div>
      </header>

      <main class="content-area">
        <div class="blue-banner">
          <h2>Leave & Overtime Management Platform</h2>
          <p>Internal enterprise system for managing employee leave and overtime approvals securely and efficiently.</p>
        </div>

        <router-view v-slot="{ Component }">
          <transition name="fade" mode="out-in">
            <component :is="Component" />
          </transition>
        </router-view>
      </main>

      <footer class="footer">
        <p>© 2026 LeaveOT System. All rights reserved.</p>
      </footer>
    </div>
  </div>
</template>
<script setup>
import { computed } from "vue"
import { useRouter } from "vue-router"

const router = useRouter()

const role = localStorage.getItem("role")
const username = localStorage.getItem("username")

const dashboardLink = computed(() => {
  if (role === "Admin") return "/admin"
  if (role === "Manager") return "/manager"
  if (role === "Employee") return "/employee"
  return "/login"
})

const logout = () => {
  localStorage.removeItem("token")
  localStorage.removeItem("role")
  localStorage.removeItem("username")
  router.push("/login")
}
</script>
<style scoped>
.header-right {
  display: flex;
  align-items: center;
  gap: 20px;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 12px;
}

.avatar-circle {
  width: 40px;
  height: 40px;
  background: #f4f7fe; /* đổi màu nền */
  color: #2b3674;      /* màu icon */
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
  font-weight: bold;
}

.user-text {
  display: flex;
  flex-direction: column;
  font-size: 14px;
}

.name {
  font-weight: 600;
  color: #2b3674;
}

.role {
  font-size: 12px;
  color: #707eae;
}

.logout-btn {
  background: #4318ff;   /* giống login */
  color: white;
  padding: 10px 25px;
  border: none;
  border-radius: 30px;
  font-weight: 600;
  cursor: pointer;
  transition: 0.3s;
}

.logout-btn:hover {
  background: #2b3674;   /* hover giống login */
}
.logout-btn {
  background: #4bb1f5;
  color: white;
  padding: 10px 25px;
  border: none;
  border-radius: 30px;
  font-weight: 600;
  cursor: pointer;
  transition: 0.3s;
}

.logout-btn:hover {
  background: #d9363e;
}
.layout-wrapper {
  display: flex;
  height: 100vh;
  background: #f4f7fe;
}

/* SIDEBAR */
.sidebar {
  width: 280px;
  background: white;
  display: flex;
  flex-direction: column;
  padding: 40px 20px;
  border-right: 1px solid #e2e8f0;
}

.brand {
  display: flex;
  align-items: center;
  gap: 12px;
  font-size: 24px;
  font-weight: 800;
  color: #2b3674;
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

.nav-menu {
  flex: 1; /* Đẩy footer sidebar xuống dưới */
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
  box-shadow: 0 10px 20px rgba(67, 24, 255, 0.2);
}

/* MAIN CONTENT */
.main-container {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
}

.header {
  padding: 20px 40px;
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.breadcrumb { font-size: 14px; color: #707eae; }
.header-left h2 { font-size: 34px; color: #2b3674; }

.header-right {
  display: flex;
  align-items: center;
  gap: 20px;
  background: white;
  padding: 10px 20px;
  border-radius: 30px;
  box-shadow: 14px 17px 40px 4px rgba(112, 144, 176, 0.08);
}

.search-box input { border: none; outline: none; background: #f4f7fe; padding: 8px 15px; border-radius: 20px; }

.content-area {
  padding: 0 40px 40px;
  flex: 1;
}

.blue-banner {
  background: linear-gradient(135deg, #4299e1 0%, #3182ce 100%);
  padding: 40px;
  border-radius: 20px;
  color: white;
  margin-bottom: 30px;
}

/* FOOTER */
.footer {
  padding: 20px 40px;
  text-align: center;
  color: #a3aed0;
  font-size: 14px;
}

/* TRANSITION */
.fade-enter-active, .fade-leave-active { transition: opacity 0.2s; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>