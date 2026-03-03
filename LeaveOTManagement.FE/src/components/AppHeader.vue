<template>
  <header class="header">

    <!-- LEFT -->
    <div class="header-left">
      <button class="menu-btn" @click="$emit('toggle-sidebar')">☰</button>
      <p class="path">Pages / Dashboard</p>
      <h2 class="current">Main Dashboard</h2>
    </div>

    <!-- RIGHT -->
    <div class="header-right">

      <!-- USER -->
      <div class="user-box">
        <div class="avatar">
          {{ username.charAt(0).toUpperCase() }}
        </div>
        <div class="user-info">
          <span class="name">{{ username }}</span>
          <span class="role">{{ role }}</span>
        </div>
      </div>

      <!-- LOGOUT -->
      <button class="logout-btn" @click="logout">
        Logout
      </button>

    </div>
  </header>
</template>

<script setup>
import { useRouter } from "vue-router"

const router = useRouter()
const username = localStorage.getItem("username") || "User"
const role = localStorage.getItem("role") || "Employee"

const logout = () => {
  localStorage.clear()
  router.push("/login")
}
</script>

<style scoped>
.header {
  padding: 25px 40px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
}

/* ---------------- LEFT ---------------- */
.path {
  font-size: 14px;
  color: #707eae;
  margin-bottom: 5px;
}

.current {
  font-size: 32px;
  font-weight: 700;
  color: #2b3674;
  line-height: 1.2;
}

/* ---------------- RIGHT ---------------- */
.header-right {
  display: flex;
  align-items: center;
  gap: 20px;
  background: white;
  padding: 10px 20px;
  border-radius: 40px;
  box-shadow: 14px 17px 40px rgba(112, 144, 176, 0.08);
}

/* ---------------- USER ---------------- */
.user-box {
  display: flex;
  align-items: center;
  gap: 10px;
}

.avatar {
  width: 38px;
  height: 38px;
  background: linear-gradient(135deg, #4318ff, #3182ce);
  color: white;
  font-weight: 600;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 50%;
}

.user-info {
  display: flex;
  flex-direction: column;
  font-size: 13px;
}

.name {
  font-weight: 600;
  color: #2b3674;
}

.role {
  font-size: 12px;
  color: #707eae;
}

/* ---------------- LOGOUT ---------------- */
.logout-btn {
  background: #4318ff;
  color: white;
  padding: 8px 20px;
  border: none;
  border-radius: 25px;
  cursor: pointer;
  transition: 0.3s;
  font-weight: 600;
  white-space: nowrap;
}

.logout-btn:hover {
  background: #2b3674;
}

.menu-btn {
  display: none;
  font-size: 22px;
  background: none;
  border: none;
  cursor: pointer;
  margin-right: 10px;
}

/* Hiện trên mobile */
@media (max-width: 1024px) {
  .menu-btn {
    display: block;
  }
}

/* MOBILE */
@media (max-width: 768px) {

  .header {
    flex-direction: column;
    align-items: flex-start;
    gap: 15px;
    padding: 15px;
  }

  .current {
    font-size: 22px;
  }

  .header-right {
    width: 100%;
    justify-content: space-between;
    padding: 10px 15px;
    border-radius: 20px;
  }

  .user-info {
    display: none; /* Ẩn name + role cho gọn */
  }

  .avatar {
    width: 32px;
    height: 32px;
    font-size: 14px;
  }

  .logout-btn {
    padding: 6px 15px;
    font-size: 13px;
  }
}

/* SMALL MOBILE */
@media (max-width: 480px) {

  .path {
    font-size: 12px;
  }

  .current {
    font-size: 18px;
  }

  .header-right {
    flex-wrap: wrap;
    gap: 10px;
  }
}
</style>