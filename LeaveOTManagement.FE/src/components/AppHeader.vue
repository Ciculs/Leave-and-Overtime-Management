<template>
  <header class="header" :class="{ dark: isDark }">
    <div class="header-left">
      <button class="menu-btn" @click="$emit('toggle-sidebar')" aria-label="Toggle sidebar">
        <svg viewBox="0 0 24 24" fill="none">
          <path d="M4 7H20" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
          <path d="M4 12H20" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
          <path d="M4 17H14" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
        </svg>
      </button>

      <div class="title-block">
        <p class="path">{{ pageSection }}</p>
        <h2 class="current">{{ pageTitle }}</h2>
      </div>
    </div>

    <div class="header-right">
      <button class="theme-switch" :class="{ active: isDark }" type="button" @click="toggleTheme"
        :aria-label="isDark ? 'Switch to light mode' : 'Switch to dark mode'">
        <span class="theme-icon sun">
          <svg viewBox="0 0 24 24" fill="none">
            <circle cx="12" cy="12" r="4" stroke="currentColor" stroke-width="2" />
            <path d="M12 2V4" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
            <path d="M12 20V22" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
            <path d="M4.93 4.93L6.34 6.34" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
            <path d="M17.66 17.66L19.07 19.07" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
            <path d="M2 12H4" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
            <path d="M20 12H22" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
            <path d="M4.93 19.07L6.34 17.66" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
            <path d="M17.66 6.34L19.07 4.93" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
          </svg>
        </span>

        <span class="switch-track">
          <span class="switch-thumb"></span>
        </span>

        <span class="theme-icon moon">
          <svg viewBox="0 0 24 24" fill="none">
            <path d="M20 15.5A8.5 8.5 0 1 1 8.5 4C8.5 10 14 15.5 20 15.5Z" stroke="currentColor" stroke-width="2"
              stroke-linejoin="round" />
          </svg>
        </span>
      </button>

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
        <span class="logout-icon">
          <svg viewBox="0 0 24 24" fill="none">
            <path d="M15 16L19 12L15 8" stroke="currentColor" stroke-width="2" stroke-linecap="round"
              stroke-linejoin="round" />
            <path d="M19 12H9" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
            <path d="M13 5V4C13 3.4 12.6 3 12 3H6C4.9 3 4 3.9 4 5V19C4 20.1 4.9 21 6 21H12C12.6 21 13 20.6 13 20V19"
              stroke="currentColor" stroke-width="2" stroke-linecap="round" />
          </svg>
        </span>
        <span>Logout</span>
      </button>
    </div>
  </header>
</template>

<script setup>
import { computed, onMounted, ref, watch } from "vue"
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

const isDark = ref(localStorage.getItem("theme") === "dark")

const applyTheme = () => {
  document.documentElement.classList.toggle("dark-mode", isDark.value)
  localStorage.setItem("theme", isDark.value ? "dark" : "light")
}

const toggleTheme = () => {
  isDark.value = !isDark.value
}

watch(isDark, applyTheme)

onMounted(() => {
  applyTheme()
})

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
  padding: 24px 32px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  gap: 18px;
  flex-wrap: wrap;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 14px;
  min-width: 0;
}

.title-block {
  min-width: 0;
}

.path {
  font-size: 13px;
  font-weight: 600;
  color: #94a3b8;
  margin: 0 0 4px;
  letter-spacing: 0.2px;
}

.current {
  font-size: 30px;
  font-weight: 800;
  color: #1e293b;
  line-height: 1.15;
  margin: 0;
}

.menu-btn {
  width: 46px;
  height: 46px;
  border: 1px solid #e5e7eb;
  background: #ffffff;
  color: #64748b;
  border-radius: 14px;
  cursor: pointer;
  display: none;
  align-items: center;
  justify-content: center;
  transition: 0.25s ease;
  flex-shrink: 0;
  box-shadow: 0 8px 24px rgba(15, 23, 42, 0.06);
}

.menu-btn:hover {
  background: #f8fafc;
  color: #2563eb;
  transform: translateY(-1px);
}

.menu-btn svg {
  width: 20px;
  height: 20px;
}

.header-right {
  display: flex;
  align-items: center;
  gap: 14px;
  background: #ffffff;
  padding: 10px 12px;
  border-radius: 22px;
  border: 1px solid #edf2f7;
  box-shadow: 0 14px 34px rgba(15, 23, 42, 0.06);
}

.theme-switch {
  height: 44px;
  padding: 0 12px;
  border: 1px solid #e2e8f0;
  border-radius: 999px;
  background: #f8fafc;
  color: #64748b;
  display: flex;
  align-items: center;
  gap: 10px;
  cursor: pointer;
  transition: 0.25s ease;
}

.theme-switch:hover {
  border-color: #cbd5e1;
  background: #f1f5f9;
}

.theme-switch.active {
  background: #000000;
  color: #ffffff;
  border-color: #111;
}

.theme-icon {
  width: 18px;
  height: 18px;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.theme-icon svg {
  width: 18px;
  height: 18px;
}

.switch-track {
  width: 42px;
  height: 22px;
  background: rgba(148, 163, 184, 0.3);
  border-radius: 999px;
  position: relative;
  transition: 0.25s ease;
}

.theme-switch.active .switch-track {
  background: #333;
}

.switch-thumb {
  width: 16px;
  height: 16px;
  border-radius: 50%;
  background: #ffffff;
  position: absolute;
  top: 3px;
  left: 3px;
  transition: 0.25s ease;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.theme-switch.active .switch-thumb {
  left: 23px;
}

.user-box {
  display: flex;
  align-items: center;
  gap: 10px;
  border: none;
  background: transparent;
  padding: 4px 8px 4px 4px;
  border-radius: 16px;
  cursor: pointer;
  transition: 0.2s ease;
}

.user-box:hover {
  background: #f8fafc;
}

.avatar {
  width: 40px;
  height: 40px;
  background: linear-gradient(135deg, #2563eb, #3b82f6);
  color: white;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 14px;
  flex-shrink: 0;
}

.user-info {
  display: flex;
  flex-direction: column;
  font-size: 13px;
  text-align: left;
  line-height: 1.2;
}

.name {
  font-weight: 700;
  color: #1e293b;
}

.role {
  font-size: 12px;
  color: #94a3b8;
  font-weight: 600;
}

.logout-btn {
  height: 44px;
  padding: 0 16px;
  border: none;
  border-radius: 999px;
  background: linear-gradient(135deg, #2563eb, #3b82f6);
  color: white;
  cursor: pointer;
  transition: 0.25s ease;
  font-weight: 700;
  display: flex;
  align-items: center;
  gap: 8px;
  white-space: nowrap;
}

.logout-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 10px 22px rgba(37, 99, 235, 0.25);
}

.logout-icon {
  width: 16px;
  height: 16px;
  display: inline-flex;
}

.logout-icon svg {
  width: 16px;
  height: 16px;
}

/* DARK HEADER MODE */
.header.dark .path {
  color: #64748b;
}

.header.dark .current {
  color: #111827;
}

.header.dark .menu-btn {
  background: #111;
  border-color: #333;
  color: #e5e7eb;
  box-shadow: none;
}

.header.dark .menu-btn:hover {
  background: #1f2937;
  color: #60a5fa;
}

.header.dark .header-right {
  background: #000000;
  border-color: #111;
  box-shadow: none;
}

.header.dark .user-box:hover {
  background: #111;
}

.header.dark .name {
  color: #ffffff;
}

.header.dark .role {
  color: #9ca3af;
}

@media (max-width: 1024px) {
  .menu-btn {
    display: inline-flex;
  }

  .header {
    padding: 20px 22px;
  }

  .current {
    font-size: 26px;
  }
}

@media (max-width: 768px) {
  .header {
    padding: 16px;
    flex-direction: column;
    align-items: stretch;
  }

  .header-left {
    width: 100%;
  }

  .header-right {
    width: 100%;
    justify-content: space-between;
    border-radius: 20px;
    padding: 10px;
    gap: 10px;
  }

  .theme-switch {
    padding: 0 10px;
  }

  .user-info {
    display: none;
  }

  .logout-btn span:last-child {
    display: none;
  }

  .logout-btn {
    width: 44px;
    min-width: 44px;
    justify-content: center;
    padding: 0;
  }
}

@media (max-width: 480px) {
  .path {
    font-size: 12px;
  }

  .current {
    font-size: 20px;
  }

  .header-right {
    flex-wrap: nowrap;
  }

  .theme-switch {
    flex: 1;
    justify-content: center;
  }
}
</style>