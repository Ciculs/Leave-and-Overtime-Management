<template>
  <aside :class="['sidebar', { collapsed, dark: isDark }]">
    <!-- TOP -->
    <div class="sidebar-top" :class="{ collapsed }">
      <template v-if="!collapsed">
        <div class="brand-row">
          <div class="brand-box">
            <div class="logo-box">LOM</div>
            <div class="brand-meta">
              <div class="brand-title">LeaveOT</div>
              <div class="brand-sub">Management</div>
            </div>
          </div>

          <button class="toggle-btn" @click="toggleSidebar" aria-label="Collapse sidebar">
            <svg viewBox="0 0 24 24" fill="none" class="menu-icon">
              <path d="M4 7H20" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
              <path d="M4 12H20" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
              <path d="M4 17H14" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
            </svg>
          </button>
        </div>
      </template>

      <template v-else>
        <div class="collapsed-top">
          <button class="toggle-btn collapsed-toggle" @click="toggleSidebar" aria-label="Expand sidebar">
            <svg viewBox="0 0 24 24" fill="none" class="menu-icon">
              <path d="M4 7H20" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
              <path d="M4 12H20" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
              <path d="M4 17H14" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
            </svg>
          </button>
        </div>
      </template>
    </div>

    <!-- MENU -->
    <nav class="nav-menu">
      <router-link :to="dashboardLink" class="nav-item" active-class="router-link-active">
        <span class="nav-icon">
          <svg viewBox="0 0 24 24" fill="none">
            <rect x="3" y="3" width="7" height="7" rx="2" stroke="currentColor" stroke-width="2" />
            <rect x="14" y="3" width="7" height="7" rx="2" stroke="currentColor" stroke-width="2" />
            <rect x="3" y="14" width="7" height="7" rx="2" stroke="currentColor" stroke-width="2" />
            <rect x="14" y="14" width="7" height="7" rx="2" stroke="currentColor" stroke-width="2" />
          </svg>
        </span>
        <span v-if="!collapsed" class="nav-label">Dashboard</span>
      </router-link>

      <template v-if="role === 'Employee'">
        <router-link to="/my-leaves" class="nav-item" active-class="router-link-active">
          <span class="nav-icon">
            <svg viewBox="0 0 24 24" fill="none">
              <path d="M7 3H17L21 7V19C21 20.1 20.1 21 19 21H7C5.9 21 5 20.1 5 19V5C5 3.9 5.9 3 7 3Z"
                stroke="currentColor" stroke-width="2" />
              <path d="M15 3V8H20" stroke="currentColor" stroke-width="2" />
            </svg>
          </span>
          <span v-if="!collapsed" class="nav-label">My Leave Requests</span>
        </router-link>

        <router-link to="/my-ot" class="nav-item" active-class="router-link-active">
          <span class="nav-icon">
            <svg viewBox="0 0 24 24" fill="none">
              <circle cx="12" cy="12" r="9" stroke="currentColor" stroke-width="2" />
              <path d="M12 7V12L15 15" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
            </svg>
          </span>
          <span v-if="!collapsed" class="nav-label">My OT Requests</span>
        </router-link>

        <router-link to="/personal-calendar" class="nav-item" active-class="router-link-active">
          <span class="nav-icon">
            <svg viewBox="0 0 24 24" fill="none">
              <rect x="3" y="5" width="18" height="16" rx="3" stroke="currentColor" stroke-width="2" />
              <path d="M8 3V7" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
              <path d="M16 3V7" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
              <path d="M3 10H21" stroke="currentColor" stroke-width="2" />
            </svg>
          </span>
          <span v-if="!collapsed" class="nav-label">Personal Calendar</span>
        </router-link>
      </template>

      <template v-else-if="role === 'Manager'">
        <router-link to="/my-leaves" class="nav-item" active-class="router-link-active">
          <span class="nav-icon">
            <svg viewBox="0 0 24 24" fill="none">
              <path d="M7 3H17L21 7V19C21 20.1 20.1 21 19 21H7C5.9 21 5 20.1 5 19V5C5 3.9 5.9 3 7 3Z"
                stroke="currentColor" stroke-width="2" />
              <path d="M15 3V8H20" stroke="currentColor" stroke-width="2" />
            </svg>
          </span>
          <span v-if="!collapsed" class="nav-label">My Leave Requests</span>
        </router-link>

        <router-link to="/team-approvals" class="nav-item" active-class="router-link-active">
          <span class="nav-icon">
            <svg viewBox="0 0 24 24" fill="none">
              <path d="M9 12L11 14L15 10" stroke="currentColor" stroke-width="2" stroke-linecap="round"
                stroke-linejoin="round" />
              <circle cx="12" cy="12" r="9" stroke="currentColor" stroke-width="2" />
            </svg>
          </span>
          <span v-if="!collapsed" class="nav-label">Leave Approvals</span>
        </router-link>

        <router-link to="/ot-manager-approval" class="nav-item" active-class="router-link-active">
          <span class="nav-icon">
            <svg viewBox="0 0 24 24" fill="none">
              <circle cx="12" cy="12" r="9" stroke="currentColor" stroke-width="2" />
              <path d="M12 7V12L15 15" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
            </svg>
          </span>
          <span v-if="!collapsed" class="nav-label">OT Approvals</span>
        </router-link>

        <router-link to="/team-calendar" class="nav-item" active-class="router-link-active">
          <span class="nav-icon">
            <svg viewBox="0 0 24 24" fill="none">
              <rect x="3" y="5" width="18" height="16" rx="3" stroke="currentColor" stroke-width="2" />
              <path d="M8 3V7" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
              <path d="M16 3V7" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
              <path d="M3 10H21" stroke="currentColor" stroke-width="2" />
            </svg>
          </span>
          <span v-if="!collapsed" class="nav-label">Team Calendar</span>
        </router-link>
      </template>

      <template v-else-if="role === 'HR' || role === 'Admin'">
        <router-link to="/hr-approvals" class="nav-item" active-class="router-link-active">
          <span class="nav-icon">
            <svg viewBox="0 0 24 24" fill="none">
              <path d="M9 12L11 14L15 10" stroke="currentColor" stroke-width="2" stroke-linecap="round"
                stroke-linejoin="round" />
              <circle cx="12" cy="12" r="9" stroke="currentColor" stroke-width="2" />
            </svg>
          </span>
          <span v-if="!collapsed" class="nav-label">Leave Approvals</span>
        </router-link>

        <router-link to="/ot-hr-approval" class="nav-item" active-class="router-link-active">
          <span class="nav-icon">
            <svg viewBox="0 0 24 24" fill="none">
              <circle cx="12" cy="12" r="9" stroke="currentColor" stroke-width="2" />
              <path d="M12 7V12L15 15" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
            </svg>
          </span>
          <span v-if="!collapsed" class="nav-label">OT Approvals</span>
        </router-link>

        <router-link to="/holidays" class="nav-item" active-class="router-link-active">
          <span class="nav-icon">
            <svg viewBox="0 0 24 24" fill="none">
              <rect x="3" y="5" width="18" height="16" rx="3" stroke="currentColor" stroke-width="2" />
              <path d="M8 3V7" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
              <path d="M16 3V7" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
              <path d="M3 10H21" stroke="currentColor" stroke-width="2" />
            </svg>
          </span>
          <span v-if="!collapsed" class="nav-label">Holiday Calendar</span>
        </router-link>

        <router-link to="/reports" class="nav-item" active-class="router-link-active">
          <span class="nav-icon">
            <svg viewBox="0 0 24 24" fill="none">
              <path d="M5 19V10" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
              <path d="M12 19V5" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
              <path d="M19 19V13" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
            </svg>
          </span>
          <span v-if="!collapsed" class="nav-label">Analytics</span>
        </router-link>

        <router-link to="/report-dashboard" class="nav-item" active-class="router-link-active">
          <span class="nav-icon">
            <svg viewBox="0 0 24 24" fill="none">
              <path d="M4 19H20" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
              <path d="M7 15L10 12L13 14L17 9" stroke="currentColor" stroke-width="2" stroke-linecap="round"
                stroke-linejoin="round" />
              <circle cx="7" cy="15" r="1" fill="currentColor" />
              <circle cx="10" cy="12" r="1" fill="currentColor" />
              <circle cx="13" cy="14" r="1" fill="currentColor" />
              <circle cx="17" cy="9" r="1" fill="currentColor" />
            </svg>
          </span>
          <span v-if="!collapsed" class="nav-label">Report Dashboard</span>
        </router-link>

        <div class="nav-group">
          <button type="button" class="nav-item nav-group-btn"
            :class="{ 'group-open': userMenuOpen, 'group-active': isUserGroupActive }" @click="toggleUserMenu">
            <div class="nav-left">
              <span class="nav-icon">
                <svg viewBox="0 0 24 24" fill="none">
                  <path d="M16 21V19C16 17.9 15.1 17 14 17H8C6.9 17 6 17.9 6 19V21" stroke="currentColor"
                    stroke-width="2" stroke-linecap="round" />
                  <circle cx="11" cy="9" r="4" stroke="currentColor" stroke-width="2" />
                  <path d="M18 8C19.7 8 21 9.3 21 11C21 12.7 19.7 14 18 14" stroke="currentColor" stroke-width="2"
                    stroke-linecap="round" />
                </svg>
              </span>
              <span v-if="!collapsed" class="nav-label">User Management</span>
            </div>

            <span v-if="!collapsed" class="arrow" :class="{ rotate: userMenuOpen }">
              <svg viewBox="0 0 24 24" fill="none">
                <path d="M7 10L12 15L17 10" stroke="currentColor" stroke-width="2" stroke-linecap="round"
                  stroke-linejoin="round" />
              </svg>
            </span>
          </button>

          <transition name="submenu">
            <div v-if="userMenuOpen && !collapsed" class="sub-menu">
              <router-link to="/user-management" class="sub-item" active-class="sub-item-active">
                User List
              </router-link>
              <router-link to="/create-user" class="sub-item" active-class="sub-item-active">
                Create User
              </router-link>
              <router-link to="/assign-manager" class="sub-item" active-class="sub-item-active">
                Assign Manager
              </router-link>
            </div>
          </transition>
        </div>
      </template>
    </nav>

    <!-- BOTTOM -->
    <div class="sidebar-bottom">
      <button class="nav-item bottom-item logout-btn" @click="logout">
        <span class="nav-icon">
          <svg viewBox="0 0 24 24" fill="none">
            <path d="M15 16L19 12L15 8" stroke="currentColor" stroke-width="2" stroke-linecap="round"
              stroke-linejoin="round" />
            <path d="M19 12H9" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
            <path d="M13 5V4C13 3.4 12.6 3 12 3H6C4.9 3 4 3.9 4 5V19C4 20.1 4.9 21 6 21H12C12.6 21 13 20.6 13 20V19"
              stroke="currentColor" stroke-width="2" stroke-linecap="round" />
          </svg>
        </span>
        <span v-if="!collapsed" class="nav-label">Logout</span>
      </button>

    <router-link to="/profile" class="user-box profile-link">
        <div class="user-avatar">
          {{ displayName.charAt(0).toUpperCase() }}
        </div>

        <div v-if="!collapsed" class="user-info">
          <div class="user-name">{{ displayName }}</div>
          <div class="user-role">{{ displayRole }}</div>
        </div>
      </router-link>
    </div>
  </aside>
</template>

<script setup>
import { computed, onBeforeUnmount, onMounted, ref, watch } from "vue"
import { useRoute, useRouter } from "vue-router"

const router = useRouter()
const route = useRoute()

const role = localStorage.getItem("role") || ""
const fullName = localStorage.getItem("fullName") || ""
const username = localStorage.getItem("username") || ""

const displayName = computed(() => fullName || username || "User")
const displayRole = computed(() => {
  if (role === "HR" || role === "Admin") return "HR ADMIN"
  return role || "Employee"
})

const collapsed = ref(false)
const isDark = ref(document.documentElement.classList.contains("dark-mode"))

const syncThemeFromDom = () => {
  isDark.value = document.documentElement.classList.contains("dark-mode")
}

let observer

onMounted(() => {
  syncThemeFromDom()

  observer = new MutationObserver(() => {
    syncThemeFromDom()
  })

  observer.observe(document.documentElement, {
    attributes: true,
    attributeFilter: ["class"],
  })
})

onBeforeUnmount(() => {
  if (observer) observer.disconnect()
})

const dashboardLink = computed(() => {
  if (role === "Admin") return "/admin"
  if (role === "HR") return "/hr-admin"
  if (role === "Manager") return "/manager"
  if (role === "Employee") return "/employee"
  return "/login"
})

const userManagementRoutes = [
  "/user-management",
  "/create-user",
  "/assign-manager",
]

const isUserGroupActive = computed(() =>
  userManagementRoutes.includes(route.path)
)

const userMenuOpen = ref(isUserGroupActive.value)

watch(
  () => route.path,
  (newPath) => {
    if (userManagementRoutes.includes(newPath) && !collapsed.value) {
      userMenuOpen.value = true
    }
  },
  { immediate: true }
)

const toggleSidebar = () => {
  collapsed.value = !collapsed.value

  if (collapsed.value) {
    userMenuOpen.value = false
  } else if (isUserGroupActive.value) {
    userMenuOpen.value = true
  }
}

const toggleUserMenu = () => {
  if (collapsed.value) {
    collapsed.value = false
    userMenuOpen.value = true
    return
  }

  userMenuOpen.value = !userMenuOpen.value
}

const logout = () => {
  localStorage.clear()
  router.push("/login")
}
</script>

<style scoped>
.sidebar {
  width: 280px;
  height: 100vh;
  background: #ffffff;
  border-right: 1px solid #eef1f7;
  padding: 20px 16px;
  display: flex;
  flex-direction: column;
  transition: width 0.3s ease, padding 0.3s ease, background 0.3s ease, border-color 0.3s ease;
  overflow: hidden;
}

.sidebar.collapsed {
  width: 92px;
  padding: 20px 10px;
}

/* TOP */
.sidebar-top {
  flex-shrink: 0;
  margin-bottom: 18px;
}

.brand-row {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.brand-box {
  display: flex;
  align-items: center;
  gap: 12px;
  min-width: 0;
  flex: 1;
}

.brand-meta {
  min-width: 0;
}

.brand-title {
  font-size: 16px;
  font-weight: 800;
  color: #1e293b;
  line-height: 1.1;
}

.brand-sub {
  font-size: 12px;
  color: #94a3b8;
  line-height: 1.1;
  margin-top: 2px;
}

.logo-box {
  width: 42px;
  height: 42px;
  border-radius: 14px;
  background: linear-gradient(135deg, #2563eb, #3b82f6);
  color: white;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 800;
  flex-shrink: 0;
  box-shadow: 0 10px 24px rgba(37, 99, 235, 0.22);
}

.toggle-btn {
  width: 40px;
  height: 40px;
  border: 1px solid #e5e7eb;
  background: #ffffff;
  color: #64748b;
  border-radius: 12px;
  cursor: pointer;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  transition: 0.25s ease;
  flex-shrink: 0;
  padding: 0;
}

.toggle-btn:hover {
  background: #f4f7fe;
  color: #2563eb;
}

.menu-icon {
  width: 18px;
  height: 18px;
}

.brand-text {
  font-size: 22px;
  font-weight: 800;
  color: #1e293b;
  white-space: nowrap;
}

/* COLLAPSED TOP */
.sidebar-top.collapsed {
  display: flex;
  justify-content: center;
}

.collapsed-top {
  width: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 12px;
}

.collapsed-brand {
  width: 44px;
  height: 44px;
  border-radius: 14px;
  background: linear-gradient(135deg, #2563eb, #3b82f6);
  color: white;
  font-size: 12px;
  font-weight: 800;
  display: flex;
  align-items: center;
  justify-content: center;
  letter-spacing: 0.5px;
  box-shadow: 0 10px 24px rgba(37, 99, 235, 0.22);
}

.collapsed-toggle {
  margin: 0 auto;
}

/* MENU */
.nav-menu {
  flex: 1;
  min-height: 0;
  overflow-y: auto;
  overflow-x: hidden;
  display: flex;
  flex-direction: column;
  gap: 8px;
  padding-right: 4px;
}

.nav-menu::-webkit-scrollbar {
  width: 6px;
}

.nav-menu::-webkit-scrollbar-thumb {
  background: #d8dfeb;
  border-radius: 999px;
}

.nav-menu::-webkit-scrollbar-track {
  background: transparent;
}

.nav-item {
  width: 100%;
  min-height: 48px;
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 12px 14px;
  border-radius: 14px;
  border: none;
  background: transparent;
  text-decoration: none;
  color: #64748b;
  font-weight: 600;
  cursor: pointer;
  transition: all 0.25s ease;
}

.nav-item:hover {
  background: #f4f7fe;
  color: #1e293b;
}

.nav-icon {
  width: 20px;
  height: 20px;
  flex-shrink: 0;
  display: inline-flex;
  align-items: center;
  justify-content: center;
}

.nav-icon svg {
  width: 20px;
  height: 20px;
}

.nav-label {
  flex: 1;
  text-align: left;
  white-space: nowrap;
}

.router-link-active {
  background: #eef4ff;
  color: #2563eb;
}

/* COLLAPSED MENU */
.sidebar.collapsed .nav-item {
  justify-content: center;
  align-items: center;
  padding: 12px 0;
  width: 100%;
}

.sidebar.collapsed .nav-icon {
  margin: 0 auto;
}

.sidebar.collapsed .nav-label,
.sidebar.collapsed .brand-text,
.sidebar.collapsed .arrow,
.sidebar.collapsed .user-info,
.sidebar.collapsed .brand-meta {
  display: none;
}

.nav-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.nav-group-btn {
  justify-content: space-between;
}

.nav-left {
  display: flex;
  align-items: center;
  gap: 12px;
  min-width: 0;
}

.group-open,
.group-active {
  background: #eef4ff;
  color: #2563eb;
}

.arrow {
  width: 18px;
  height: 18px;
  display: inline-flex;
  transition: transform 0.25s ease;
}

.arrow svg {
  width: 18px;
  height: 18px;
}

.arrow.rotate {
  transform: rotate(180deg);
}

.sub-menu {
  position: relative;
  margin-left: 22px;
  padding-left: 16px;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.sub-menu::before {
  content: "";
  position: absolute;
  left: 0;
  top: 6px;
  bottom: 6px;
  width: 1px;
  background: #e5e7eb;
}

.sub-item {
  text-decoration: none;
  color: #5f6b7a;
  font-weight: 500;
  padding: 12px 14px;
  border-radius: 12px;
  transition: 0.25s ease;
}

.sub-item:hover {
  background: #f4f7fe;
  color: #1e293b;
}

.sub-item-active {
  background: #eaf3ff;
  color: #2563eb;
  font-weight: 600;
}

/* BOTTOM */
.sidebar-bottom {
  flex-shrink: 0;
  margin-top: 16px;
  padding-top: 16px;
  border-top: 1px solid #eef1f7;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.bottom-item {
  color: #64748b;
}

.logout-btn {
  font: inherit;
}

.user-box {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 6px 6px 0;
}

.user-avatar {
  width: 40px;
  height: 40px;
  border-radius: 14px;
  background: linear-gradient(135deg, #2563eb, #3b82f6);
  color: #fff;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  flex-shrink: 0;
}

.user-info {
  min-width: 0;
}

.user-name {
  font-size: 14px;
  font-weight: 700;
  color: #111827;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.user-role {
  font-size: 12px;
  color: #94a3b8;
}

.user-box {
  text-decoration: none;
  cursor: pointer;
}

.user-box:hover {
  background: #f4f7fe;
  border-radius: 12px;
}

.sidebar.dark .user-box:hover {
  background: #111;
}

.profile-link {
  text-decoration: none;
  cursor: pointer;
  transition: 0.25s ease;
}

.profile-link:hover {
  background: #f4f7fe;
  border-radius: 12px;
}

.sidebar.dark .profile-link:hover {
  background: #111;
}

/* COLLAPSED BOTTOM */
.sidebar.collapsed .sidebar-bottom {
  align-items: center;
}

.sidebar.collapsed .bottom-item {
  justify-content: center;
  width: 100%;
}

.sidebar.collapsed .user-box {
  justify-content: center;
  width: 100%;
  padding: 0;
}

.sidebar.collapsed .user-avatar {
  margin: 0 auto;
}

/* ANIMATION */
.submenu-enter-active,
.submenu-leave-active {
  transition: all 0.25s ease;
  overflow: hidden;
}

.submenu-enter-from,
.submenu-leave-to {
  opacity: 0;
  max-height: 0;
  transform: translateY(-4px);
}

.submenu-enter-to,
.submenu-leave-from {
  opacity: 1;
  max-height: 240px;
  transform: translateY(0);
}

/* DARK MODE */
/* DARK MODE - BLACK STYLE */
.sidebar.dark {
  background: #000000;
  border-right-color: #111;
}

/* TEXT */
.sidebar.dark .brand-text,
.sidebar.dark .brand-title {
  color: #ffffff;
}

.sidebar.dark .brand-sub {
  color: #9ca3af;
}

/* TOGGLE */
.sidebar.dark .toggle-btn {
  background: #111;
  border-color: #333;
  color: #e5e7eb;
}

.sidebar.dark .toggle-btn:hover {
  background: #1f2937;
  color: #60a5fa;
}

/* MENU */
.sidebar.dark .nav-item {
  color: #e5e7eb;
}

.sidebar.dark .nav-item:hover {
  background: #111;
  color: #ffffff;
}

.sidebar.dark .router-link-active,
.sidebar.dark .group-open,
.sidebar.dark .group-active {
  background: #1f2937;
  color: #60a5fa;
}

/* SUB MENU */
.sidebar.dark .sub-menu::before {
  background: #333;
}

.sidebar.dark .sub-item {
  color: #e5e7eb;
}

.sidebar.dark .sub-item:hover {
  background: #111;
  color: #ffffff;
}

.sidebar.dark .sub-item-active {
  background: #1f2937;
  color: #60a5fa;
}

/* BOTTOM */
.sidebar.dark .sidebar-bottom {
  border-top-color: #111;
}

.sidebar.dark .user-name {
  color: #ffffff;
}

.sidebar.dark .user-role {
  color: #9ca3af;
}

/* SCROLL */
.sidebar.dark .nav-menu::-webkit-scrollbar-thumb {
  background: #333;
}

/* RESPONSIVE */
@media (max-width: 1024px) {
  .sidebar {
    width: 240px;
  }

  .sidebar.collapsed {
    width: 86px;
  }
}

@media (max-width: 768px) {
  .sidebar {
    width: 100%;
    height: auto;
    max-height: 100vh;
    border-right: none;
    border-bottom: 1px solid #eef1f7;
    border-radius: 0 0 18px 18px;
  }

  .sidebar.dark {
    border-bottom-color: #1e293b;
  }

  .sidebar.collapsed {
    width: 100%;
  }

  .nav-menu {
    max-height: 50vh;
  }
}
</style>