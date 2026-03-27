<template>
  <header class="header">
    <div class="header-left">
      <button class="menu-btn" @click="$emit('toggle-sidebar')">☰</button>

      <div>
        <p class="path">{{ pageSection }}</p>
        <h2 class="current">{{ pageTitle }}</h2>
      </div>
    </div>

    <div class="header-right">
      <button class="user-box" type="button" @click="goToProfile">
        <div class="avatar">
          {{ displayName.charAt(0).toUpperCase() }}
        </div>

        <div class="user-info">
          <span class="name">{{ displayName }}</span>
          <span class="role">{{ displayRole }}</span>
        </div>
      </button>

      <button class="logout-btn" @click="logout">
        Logout
      </button>
    </div>
  </header>
</template>

<script setup>
import { computed } from "vue"
import { useRoute, useRouter } from "vue-router"

const router = useRouter()
const route = useRoute()

const rawName =
  localStorage.getItem("fullName") ||
  localStorage.getItem("username") ||
  "User"

const rawRole = localStorage.getItem("role") || "Employee"

const displayName = computed(() => rawName)

const displayRole = computed(() => {
  if (rawRole === "HR" || rawRole === "Admin") return "HR ADMIN"
  return rawRole
})

const pageTitle = computed(() => route.meta.title || "Dashboard")
const pageSection = computed(() => route.meta.section || "Pages")

const goToProfile = () => {
  router.push("/profile")
}

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

.header-left {
  display: flex;
  align-items: center;
  gap: 10px;
}

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

.header-right {
  display: flex;
  align-items: center;
  gap: 20px;
  background: white;
  padding: 10px 20px;
  border-radius: 40px;
  box-shadow: 14px 17px 40px rgba(112, 144, 176, 0.08);
}

.user-box {
  display: flex;
  align-items: center;
  gap: 10px;
  border: none;
  background: transparent;
  padding: 0;
  cursor: pointer;
  transition: 0.2s;
}

.user-box:hover {
  opacity: 0.9;
  transform: translateY(-1px);
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
  text-align: left;
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

@media (max-width: 1024px) {
  .menu-btn {
    display: block;
  }
}

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
    display: none;
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